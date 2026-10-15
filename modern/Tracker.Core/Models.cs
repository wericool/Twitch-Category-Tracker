namespace Tracker.Core;

public record ChannelSnapshot(string Login, string DisplayName, bool Live, string? CategoryId,
    string? Category, string? Title, int Viewers = 0);

public sealed class WatchedChannel
{
    public string Login { get; set; } = "";
    public bool Notifications { get; set; } = true;
    public ChannelSnapshot? Snapshot { get; set; }
    public DateTimeOffset? CheckedAt { get; set; }
    public string? Error { get; set; }
}

public record HistoryEntry(DateTimeOffset At, string Login, string Kind, string Message);

public sealed class TrackerSettings
{
    public int IntervalSeconds { get; set; } = 30;
    public bool Notifications { get; set; } = true;
    public bool NotifyOnline { get; set; } = true;
    public bool CloseToTray { get; set; } = true;
}

public sealed class TrackerData
{
    public int Version { get; set; } = 1;
    public TrackerSettings Settings { get; set; } = new();
    public List<WatchedChannel> Channels { get; set; } = [];
    public List<HistoryEntry> History { get; set; } = [];
}

public static class ChannelLogin
{
    public static string Normalize(string input)
    {
        input = input.Trim();
        if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
        {
            if (uri.Host is not ("twitch.tv" or "www.twitch.tv" or "m.twitch.tv"))
                throw new ArgumentException("Нужна ссылка на Twitch или ник стримера.");
            input = uri.AbsolutePath.Trim('/');
        }
        input = input.TrimStart('@').ToLowerInvariant();
        if (!System.Text.RegularExpressions.Regex.IsMatch(input, "^[a-z0-9_]{1,25}$"))
            throw new ArgumentException("Введите ник Twitch: латинские буквы, цифры и подчёркивание.");
        return input;
    }
}

public static class Changes
{
    public static HistoryEntry? Compare(ChannelSnapshot? previous, ChannelSnapshot current, DateTimeOffset now)
    {
        if (previous is null)
            return new(now, current.Login, "initial", current.Live ? $"Отслеживание начато · {current.Category ?? "Без категории"}" : "Отслеживание начато · Не в эфире");
        if (previous.Live != current.Live)
            return new(now, current.Login, current.Live ? "online" : "offline", current.Live ? $"В эфире · {current.Category ?? "Без категории"}" : "Трансляция завершена");
        if (current.Live && (previous.CategoryId != current.CategoryId || previous.Category != current.Category))
            return new(now, current.Login, "category", $"{previous.Category ?? "Без категории"} → {current.Category ?? "Без категории"}");
        return null;
    }
}
