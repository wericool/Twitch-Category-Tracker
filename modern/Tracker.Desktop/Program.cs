using System;
using System.Linq;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Win32;
using Tracker.Core;

namespace Tracker.Desktop;

internal static class Program
{
    public static bool QaMode;
    private static readonly Lazy<string> PortableDirectory = new(() => PortableData.Prepare(AppContext.BaseDirectory,
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TwitchCategoryTracker", "v2")));
    public static string DataDirectory => PortableDirectory.Value;
    public static void Log(string message)
    {
        if (QaMode) return;
        try
        {
            var directory = DataDirectory;
            File.AppendAllText(Path.Combine(directory, "startup.log"), DateTimeOffset.Now.ToString("o") + " " + message + Environment.NewLine);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }
    [STAThread]
    public static void Main(string[] args)
    {
        QaMode = args.Contains("--qa");
        Log("starting");
        using var mutex = new Mutex(true, "Local\\WericoolTwitchCategoryTrackerV2" + (QaMode ? "Qa" : ""), out var first);
        if (!first) return;
        var builder = AppBuilder.Configure<App>();
        if (QaMode) builder = builder.UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).UseSkia();
        else builder = builder.UsePlatformDetect().With(new Win32PlatformOptions { RenderingMode = [Win32RenderingMode.Software] });
        try { builder.StartWithClassicDesktopLifetime(args); }
        catch (Exception ex) { Log(ex.ToString()); throw; }
    }
}
