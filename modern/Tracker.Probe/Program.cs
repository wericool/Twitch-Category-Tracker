using System.Text.Json;
using Tracker.Core;

var channel = ChannelLogin.Normalize(args.Length > 0 ? args[0] : "ethenpov");
var samples = args.Length > 1 ? Math.Clamp(int.Parse(args[1]), 1, 120) : 3;
var interval = args.Length > 2 ? Math.Clamp(int.Parse(args[2]), 30, 3600) : 30;
var root = new DirectoryInfo(AppContext.BaseDirectory);
while (root is not null && !File.Exists(Path.Combine(root.FullName, "modern", "Tracker.slnx"))) root = root.Parent;
var output = Path.Combine(root?.FullName ?? AppContext.BaseDirectory, "qa-output", "live-" + channel + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
var storage = new Storage(output);
using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
var engine = new TrackerEngine(storage, new TwitchClient(http));
using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
Console.WriteLine($"Guest observation of {channel}; {samples} samples, interval {interval}s. No account credentials. Data: {output}");
try
{
    await engine.AddAsync(channel, cancellation.Token);
    for (var sample = 1; sample <= samples; sample++)
    {
        if (sample > 1)
        {
            await Task.Delay(TimeSpan.FromSeconds(interval), cancellation.Token);
            await engine.PollAsync(cancellation.Token);
        }
        var current = engine.Data.Channels.Single();
        var reloaded = new Storage(output).Load();
        if (reloaded.Channels.Single().Snapshot != current.Snapshot || reloaded.History.Count != engine.Data.History.Count)
            throw new Exception("Disk persistence does not match the current observation.");
        var record = new { sample, at = current.CheckedAt, current.Snapshot, current.Error, historyCount = engine.Data.History.Count, changes = engine.Data.History.Where(e => e.Kind != "initial").ToList(), persisted = true };
        var line = JsonSerializer.Serialize(record);
        await File.AppendAllTextAsync(Path.Combine(output, "observations.jsonl"), line + Environment.NewLine);
        Console.WriteLine(line);
        if (current.Error is not null) Environment.ExitCode = 1;
    }
    await engine.ExportAsync();
    Console.WriteLine($"Finished. Category transitions actually observed: {engine.Data.History.Count(e => e.Kind == "category")}. History on disk: {Path.Combine(output, "tracker.json")}");
}
catch (OperationCanceledException) { Console.WriteLine("Observation stopped. Collected data has been preserved."); }
