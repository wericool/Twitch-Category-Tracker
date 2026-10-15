using System.Diagnostics;

namespace Tracker.Core;

public static class PortableData
{
    public static string Prepare(string executableDirectory, string? legacyDirectory = null)
    {
        var directory = Path.GetFullPath(Path.Combine(executableDirectory, "data"));
        Directory.CreateDirectory(directory);
        var marker = Path.Combine(directory, ".portable");
        var tracker = Path.Combine(directory, "tracker.json");
        if (File.Exists(marker)) return directory;
        if (!File.Exists(tracker) && legacyDirectory is not null && File.Exists(Path.Combine(legacyDirectory, "tracker.json")))
        {
            // Preserve the source and any existing portable data. Copy the main
            // state last so an interrupted migration can resume safely.
            foreach (var source in Directory.EnumerateFiles(legacyDirectory))
            {
                var name = Path.GetFileName(source);
                if (name == "tracker.json.bak" || name == "startup.log" || name.StartsWith("tracker.json.damaged-", StringComparison.Ordinal)
                    || (name.StartsWith("history-", StringComparison.Ordinal) && name.EndsWith(".json", StringComparison.Ordinal)))
                    CopyIfMissing(source, Path.Combine(directory, name));
            }
            CopyIfMissing(Path.Combine(legacyDirectory, "tracker.json"), tracker);
        }
        File.WriteAllText(marker, "Twitch Category Tracker portable data v1\n");
        return directory;
    }

    private static void CopyIfMissing(string source, string target)
    {
        if (File.Exists(target)) return;
        var temporary = target + ".migrating.tmp";
        File.Copy(source, temporary, true);
        File.Move(temporary, target, false);
    }

    public static ProcessStartInfo OpenFolderRequest(string directory)
    {
        var fullPath = Path.GetFullPath(directory);
        if (!Directory.Exists(fullPath)) throw new DirectoryNotFoundException(fullPath);
        var request = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"))
        { UseShellExecute = false };
        request.ArgumentList.Add(fullPath);
        return request;
    }
}
