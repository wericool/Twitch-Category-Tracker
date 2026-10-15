using System.Net;
using System.Text;
using System.Text.Json;
using Tracker.Core;

var passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name);
    passed++;
}
async Task Throws(Func<Task> action, string name)
{
    try { await action(); }
    catch { Check(true, name); return; }
    throw new Exception("FAIL (expected rejection): " + name);
}

Check(ChannelLogin.Normalize(" https://www.twitch.tv/Shroud/ ") == "shroud", "channel URLs normalize");
Check(ChannelLogin.Normalize("@SHROUD") == "shroud", "case and at sign normalize");
await Throws(() => Task.FromResult(ChannelLogin.Normalize("https://evil.test/shroud")), "foreign URLs rejected");
await Throws(() => Task.FromResult(ChannelLogin.Normalize("shroud?foo=bar")), "query injection rejected");
var now = DateTimeOffset.UtcNow;
var live = new ChannelSnapshot("tester", "Tester", true, "1", "Игра", "Title");
var offline = live with { Live = false, CategoryId = null, Category = null };
Check(Changes.Compare(null, live, now)?.Kind == "initial", "first observation is baseline");
Check(Changes.Compare(live, live, now) is null, "unchanged observations deduplicated");
Check(Changes.Compare(live, live with { CategoryId = "2", Category = "Other" }, now)?.Kind == "category", "category transition detected");
Check(Changes.Compare(live, offline, now)?.Kind == "offline", "offline transition distinct from category");
Check(Changes.Compare(offline, live, now)?.Kind == "online", "online transition distinct from category");
Check(Changes.Compare(offline, offline with { Category = "Configured" }, now) is null, "offline configured categories do not notify");

var testDirectory = Path.Combine(Path.GetTempPath(), "TwitchCategoryTracker-tests-" + Guid.NewGuid().ToString("N"));
var storage = new Storage(testDirectory);
var guestPayload = """{"data":{"user":{"login":"tester","displayName":"Тестер","stream":{"id":"1","title":"Test","viewersCount":12,"game":{"id":"1","name":"Игра"}}}}} """;
var failNetwork = false;
var requests = new List<string>();
var handler = new FakeHandler(async request =>
{
    requests.Add(request.RequestUri!.AbsoluteUri);
    if (failNetwork) throw new HttpRequestException("simulated disconnect");
    if (request.Method == HttpMethod.Get) return FakeHandler.Json("<script>clientId=\"publicidfortestonly\"</script>");
    var body = await request.Content!.ReadAsStringAsync();
    Check(!request.Headers.Contains("Authorization") && !request.Headers.Contains("Cookie"), "guest request carries no account credentials");
    if (body.Contains("missing")) return FakeHandler.Json("""{"data":{"user":null}}""");
    return FakeHandler.Json(guestPayload);
});
var engine = new TrackerEngine(storage, new TwitchClient(new HttpClient(handler)));
var notifications = new List<HistoryEntry>();
engine.Notify += notifications.Add;
await engine.AddAsync("Tester");
Check(engine.Data.Channels.Single().Snapshot?.Category == "Игра", "guest payload parsed and channel added");
Check(notifications.Count == 0, "baseline does not notify");
await Throws(() => engine.AddAsync("tester"), "duplicate channel rejected");
await Throws(() => engine.AddAsync("missing"), "nonexistent channel rejected");
await engine.PollAsync();
Check(engine.Data.History.Count == 1, "repeated poll creates no duplicate history");
guestPayload = guestPayload.Replace("Игра", "Другая игра");
await engine.PollAsync();
Check(notifications.Count == 1 && notifications[0].Kind == "category", "category change emits exactly one notification");
failNetwork = true;
await engine.PollAsync();
Check(engine.Data.Channels[0].Error is not null, "network error shown separately");
Check(engine.Data.Channels[0].Snapshot?.Category == "Другая игра" && engine.Data.History.Count == 2, "network failure preserves last good category and history");
failNetwork = false;
await engine.PollAsync();
Check(engine.Data.Channels[0].Error is null && notifications.Count == 1, "recovery clears error without false notification");
Check(storage.Load().Channels[0].Snapshot?.DisplayName == "Тестер", "Unicode data survives restart");
var exported = await engine.ExportAsync();
Check(File.Exists(exported) && File.ReadAllText(exported).Contains("category"), "history export contains actual events");
await engine.RemoveAsync("tester");
Check(storage.Load().Channels.Count == 0, "removal persists");
File.WriteAllText(Path.Combine(testDirectory, "tracker.json"), "broken");
var recovery = new Storage(testDirectory);
Check(recovery.Load().Channels.Count == 0 && recovery.RecoveryMessage is not null && Directory.GetFiles(testDirectory, "*.damaged-*").Length == 1, "damaged settings backed up before recovery");

var pollEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var releasePoll = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var delayGuest = false;
var delayedRequests = 0;
var serializedEngine = new TrackerEngine(new Storage(Path.Combine(testDirectory, "concurrency")), new TwitchClient(new HttpClient(new FakeHandler(async request =>
{
    if (request.Method == HttpMethod.Get) return FakeHandler.Json("clientId=\"publicidfortestonly\"");
    if (delayGuest) { delayedRequests++; pollEntered.TrySetResult(); await releasePoll.Task; }
    return FakeHandler.Json(guestPayload);
}))));
await serializedEngine.AddAsync("tester");
delayGuest = true;
var firstPoll = serializedEngine.PollAsync();
await pollEntered.Task;
await serializedEngine.PollAsync();
Check(delayedRequests == 1, "poll rounds never overlap");
var removal = serializedEngine.RemoveAsync("tester");
Check(!removal.IsCompleted, "list mutation waits for active observation");
releasePoll.SetResult();
await Task.WhenAll(firstPoll, removal);
Check(serializedEngine.Data.Channels.Count == 0, "removal during poll finishes without enumerator failure");

var channelGames = new Dictionary<string, int> { ["alpha"] = 1, ["beta"] = 1 };
var offlineChannels = new HashSet<string>();
var perChannelStorage = new Storage(Path.Combine(testDirectory, "per-channel"));
var perChannelEngine = new TrackerEngine(perChannelStorage, new TwitchClient(new HttpClient(new FakeHandler(async request =>
{
    if (request.Method == HttpMethod.Get) return FakeHandler.Json("clientId=\"publicidfortestonly\"");
    using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
    var name = body.RootElement.GetProperty("variables").GetProperty("login").GetString()!;
    object? stream = offlineChannels.Contains(name) ? null : new { id = "1", title = "Test", viewersCount = 1, game = new { id = channelGames[name].ToString(), name = "Game " + channelGames[name] } };
    return FakeHandler.Json(JsonSerializer.Serialize(new { data = new { user = new { login = name, displayName = name, stream } } }));
}))));
var channelNotices = new List<HistoryEntry>();
perChannelEngine.Notify += channelNotices.Add;
await perChannelEngine.AddAsync("alpha");
await perChannelEngine.AddAsync("beta");
await perChannelEngine.SetChannelNotificationsAsync("alpha", false);
channelGames["alpha"] = channelGames["beta"] = 2;
await perChannelEngine.PollAsync();
Check(channelNotices.Count == 1 && channelNotices[0].Login == "beta", "muting alpha leaves beta notifications enabled");
Check(perChannelEngine.Data.History.Count(e => e.Kind == "category") == 2, "muted channel still records category history");
Check(!perChannelStorage.Load().Channels.Single(c => c.Login == "alpha").Notifications, "per-channel mute survives storage reload");
await perChannelEngine.SetChannelNotificationsAsync("alpha", true);
await perChannelEngine.SetChannelNotificationsAsync("beta", false);
channelGames["alpha"] = channelGames["beta"] = 3;
await perChannelEngine.PollAsync();
Check(channelNotices.Count == 2 && channelNotices[^1].Login == "alpha", "re-enabling alpha restores its notifications independently");
await perChannelEngine.UpdateSettingsAsync(settings => settings.Notifications = false);
channelGames["alpha"] = channelGames["beta"] = 4;
await perChannelEngine.PollAsync();
Check(channelNotices.Count == 2 && perChannelEngine.Data.History.Count(e => e.Kind == "category") == 6, "global switch overrides channel flags while keeping history");
await perChannelEngine.UpdateSettingsAsync(settings => settings.Notifications = true);
offlineChannels.Add("alpha"); offlineChannels.Add("beta");
await perChannelEngine.PollAsync();
Check(channelNotices.Count == 3 && channelNotices[^1].Login == "alpha" && channelNotices[^1].Kind == "offline", "per-channel switch also controls stream-ending notifications");
Check(JsonSerializer.Deserialize<WatchedChannel>("""{"Login":"older_saved_channel"}""")!.Notifications, "older saved channels default to notifications enabled");

var legacyDirectory = Path.Combine(testDirectory, "legacy");
var legacyStorage = new Storage(legacyDirectory);
var legacyData = new TrackerData { Channels = [new() { Login = "preserved", Notifications = false }], History = [new(now, "preserved", "initial", "Сохранённая история")] };
legacyStorage.Save(legacyData);
legacyStorage.Save(legacyData);
File.WriteAllText(Path.Combine(legacyDirectory, "history-saved.json"), "[]");
var originalLegacyBytes = File.ReadAllBytes(Path.Combine(legacyDirectory, "tracker.json"));
var executableDirectory = Path.Combine(testDirectory, "portable utility with spaces");
var portableDirectory = PortableData.Prepare(executableDirectory, legacyDirectory);
Check(portableDirectory == Path.Combine(executableDirectory, "data"), "portable data directory is next to the executable");
var migrated = new Storage(portableDirectory).Load();
Check(migrated.Channels.Single().Login == "preserved" && !migrated.Channels[0].Notifications && migrated.History.Single().Message == "Сохранённая история", "migration preserves channels mute settings and Unicode history");
Check(File.Exists(Path.Combine(portableDirectory, "tracker.json.bak")) && File.Exists(Path.Combine(portableDirectory, "history-saved.json")), "migration includes backup and history exports");
Check(File.ReadAllBytes(Path.Combine(legacyDirectory, "tracker.json")).SequenceEqual(originalLegacyBytes), "migration leaves the original AppData file intact");
migrated.Channels.Clear();
new Storage(portableDirectory).Save(migrated);
legacyData.Channels.Add(new() { Login = "must_not_reimport" });
legacyStorage.Save(legacyData);
PortableData.Prepare(executableDirectory, legacyDirectory);
Check(new Storage(portableDirectory).Load().Channels.Count == 0, "later launches never overwrite portable changes with old AppData");
var folderRequest = PortableData.OpenFolderRequest(portableDirectory);
Check(folderRequest.FileName.EndsWith("explorer.exe", StringComparison.OrdinalIgnoreCase) && folderRequest.ArgumentList.Single() == portableDirectory && !folderRequest.UseShellExecute,
    "folder button uses explicit Explorer path with a safely quoted directory argument");
Console.WriteLine($"All {passed} checks passed. Test data: {testDirectory}");

sealed class FakeHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => handler(request);
    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
}
