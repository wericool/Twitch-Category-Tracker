using System.Text.Json.Serialization;

namespace Tracker.Core;

public sealed record TwitchQuery(string query, TwitchVariables variables);
public sealed record TwitchVariables(string login);

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(TrackerData))]
[JsonSerializable(typeof(List<HistoryEntry>))]
[JsonSerializable(typeof(TwitchQuery))]
public partial class TrackerJsonContext : JsonSerializerContext;
