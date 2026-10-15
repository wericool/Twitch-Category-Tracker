using System.Text.Json;

namespace Tracker.Core;

public sealed class TrackerEngine
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly SemaphoreSlim _wake = new(0, 1);
    public TrackerData Data { get; }
    public Storage Storage { get; }
    public TwitchClient Client { get; }
    public bool Paused { get; private set; }
    public bool Busy { get; private set; }
    public DateTimeOffset? LastPoll { get; private set; }
    public event Action? Updated;
    public event Action<HistoryEntry>? Notify;

    public TrackerEngine(Storage storage, TwitchClient client)
    {
        Storage = storage;
        Client = client;
        Data = storage.Load();
    }

    public async Task AddAsync(string input, CancellationToken ct = default)
    {
        var login = ChannelLogin.Normalize(input);
        await _gate.WaitAsync(ct);
        try
        {
            if (Data.Channels.Any(c => c.Login == login)) throw new InvalidOperationException("Этот стример уже в списке.");
            if (Data.Channels.Count >= 100) throw new InvalidOperationException("Можно отслеживать до 100 каналов.");
            var snapshot = await Client.GetChannelAsync(login, ct);
            if (snapshot is null) throw new InvalidOperationException("Канал не найден. Проверьте ник стримера.");
            var channel = new WatchedChannel { Login = login };
            Data.Channels.Add(channel);
            Apply(channel, snapshot);
            Storage.Save(Data);
        }
        finally { _gate.Release(); Updated?.Invoke(); }
    }

    public async Task RemoveAsync(string login)
    {
        await _gate.WaitAsync();
        try { Data.Channels.RemoveAll(c => c.Login == login); Storage.Save(Data); }
        finally { _gate.Release(); Updated?.Invoke(); }
    }

    public async Task UpdateSettingsAsync(Action<TrackerSettings> change)
    {
        await _gate.WaitAsync();
        try { change(Data.Settings); Storage.Save(Data); Wake(); }
        finally { _gate.Release(); Updated?.Invoke(); }
    }

    public async Task PollAsync(CancellationToken ct = default)
    {
        if (!await _gate.WaitAsync(0, ct)) return;
        Busy = true;
        Updated?.Invoke();
        try
        {
            if (Data.Channels.Count == 0) return;
            foreach (var channel in Data.Channels)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var snapshot = await Client.GetChannelAsync(channel.Login, ct);
                    if (snapshot is null) channel.Error = "Канал больше не найден";
                    else Apply(channel, snapshot);
                }
                catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
                { channel.Error = FriendlyError(ex); }
            }
            LastPoll = DateTimeOffset.UtcNow;
            Storage.Save(Data);
        }
        finally { Busy = false; _gate.Release(); Updated?.Invoke(); }
    }

    public async Task SetChannelNotificationsAsync(string login, bool enabled)
    {
        await _gate.WaitAsync();
        try
        {
            var channel = Data.Channels.FirstOrDefault(c => c.Login == login);
            if (channel is null) return;
            channel.Notifications = enabled;
            Storage.Save(Data);
        }
        finally { _gate.Release(); Updated?.Invoke(); }
    }

    private void Apply(WatchedChannel channel, ChannelSnapshot snapshot)
    {
        var now = DateTimeOffset.UtcNow;
        var entry = Changes.Compare(channel.Snapshot, snapshot, now);
        channel.Snapshot = snapshot;
        channel.CheckedAt = now;
        channel.Error = null;
        if (entry is null) return;
        Data.History.Add(entry);
        if (Data.History.Count > 1000) Data.History.RemoveRange(0, Data.History.Count - 1000);
        if (Data.Settings.Notifications && channel.Notifications && (entry.Kind == "category" || (Data.Settings.NotifyOnline && entry.Kind is "online" or "offline")))
            Notify?.Invoke(entry);
    }

    public async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (!Paused) await PollAsync(ct);
            await _wake.WaitAsync(TimeSpan.FromSeconds(Data.Settings.IntervalSeconds), ct);
        }
    }

    public void TogglePause() { Paused = !Paused; Wake(); Updated?.Invoke(); }
    private void Wake() { try { _wake.Release(); } catch (SemaphoreFullException) { } }

    public async Task ClearHistoryAsync()
    {
        await _gate.WaitAsync();
        try { Data.History.Clear(); Storage.Save(Data); }
        finally { _gate.Release(); Updated?.Invoke(); }
    }

    public async Task<string> ExportAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var path = Path.Combine(Storage.DirectoryPath, "history-" + DateTime.Now.ToString("yyyyMMdd-HHmmssfff") + ".json");
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(Data.History, TrackerJsonContext.Default.ListHistoryEntry));
            return path;
        }
        finally { _gate.Release(); }
    }

    public static string FriendlyError(Exception ex) => ex switch
    {
        HttpRequestException h when (int?)h.StatusCode == 429 => "Лимит Twitch. Следующая попытка — в следующем цикле.",
        HttpRequestException => "Нет связи с Twitch. Последние данные сохранены.",
        OperationCanceledException => "Twitch не ответил вовремя. Последние данные сохранены.",
        JsonException => "Twitch изменил формат ответа. Последние данные сохранены.",
        _ => ex.Message
    };
}
