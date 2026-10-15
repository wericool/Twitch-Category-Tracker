using System.Text.Json;

namespace Tracker.Core;

public sealed class Storage
{
    public Storage(string directory) => DirectoryPath = directory;
    public string DirectoryPath { get; }
    public static readonly JsonSerializerOptions Json = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    public string? RecoveryMessage { get; private set; }

    public TrackerData Load()
    {
        Directory.CreateDirectory(DirectoryPath);
        var path = Path.Combine(DirectoryPath, "tracker.json");
        if (!File.Exists(path)) return new();
        try
        {
            var data = JsonSerializer.Deserialize<TrackerData>(File.ReadAllText(path), Json)
                ?? throw new JsonException("Empty tracker data");
            if (data.Version != 1 || data.Settings is null || data.Channels is null || data.History is null)
                throw new JsonException("Unsupported tracker data");
            data.Settings.IntervalSeconds = Math.Clamp(data.Settings.IntervalSeconds, 30, 3600);
            data.History = data.History.TakeLast(1000).ToList();
            data.Channels = data.Channels.GroupBy(c => c.Login, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).Take(100).ToList();
            return data;
        }
        catch (JsonException)
        {
            var backup = path + ".damaged-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            File.Copy(path, backup);
            RecoveryMessage = "Файл настроек повреждён. Исходный файл сохранён рядом с настройками.";
            return new();
        }
    }

    public void Save(TrackerData data)
    {
        Directory.CreateDirectory(DirectoryPath);
        var path = Path.Combine(DirectoryPath, "tracker.json");
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(data, Json));
        if (File.Exists(path)) File.Copy(path, path + ".bak", true);
        File.Move(temporary, path, true);
    }

}
