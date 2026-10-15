using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
#if UI_CHECKS
using Avalonia.Headless;
#endif
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Tracker.Core;

namespace Tracker.Desktop;

public sealed partial class MainWindow : Window
{
    private readonly CancellationTokenSource _lifetime = new();
    private readonly TrackerEngine _engine;
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(20) };
    private WindowsTray? _tray;
    private bool _quitting;
    private bool _adding;
    private bool _started;
    private string? _lastOpenedFolder;
    private int _testNotificationsSent;
    private readonly string _qaDirectory;

    public MainWindow()
    {
        Program.Log("window constructor");
        AvaloniaXamlLoader.Load(this);
        var projectRoot = new DirectoryInfo(AppContext.BaseDirectory);
        while (projectRoot is not null && !File.Exists(Path.Combine(projectRoot.FullName, "modern", "Tracker.slnx"))) projectRoot = projectRoot.Parent;
        _qaDirectory = Path.Combine(projectRoot?.FullName ?? AppContext.BaseDirectory, "qa-output");
        var dataPath = Program.TestMode ? Path.Combine(_qaDirectory, "compact-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"))
            : Program.DataDirectory;
        _engine = new(new Storage(dataPath), new TwitchClient(_http));
        _engine.Updated += () => { if (Dispatcher.UIThread.CheckAccess()) RenderState(); else Dispatcher.UIThread.Post(RenderState); };
        _engine.Notify += entry => Dispatcher.UIThread.Post(() => ShowNotification(entry.Login, entry.Message));
        PropertyChanged += (_, e) =>
        {
            if (e.Property == WindowStateProperty && WindowState == WindowState.Minimized && (_tray is not null || Program.QaMode))
                Hide();
        };
        Closing += (_, e) =>
        {
            if (!_quitting && _engine.Data.Settings.CloseToTray && !Program.TestMode) { e.Cancel = true; Hide(); return; }
            if (!_quitting) { e.Cancel = true; Quit(); }
        };
        Opened += async (_, _) =>
        {
            if (_started) return;
            _started = true;
            Program.Log("window opened");
            try
            {
                if (Program.TestMode) { if (Program.NativeSmoke) InitializeTray(); await RunQaAsync(); }
                else { InitializeTray(); await _engine.RunAsync(_lifetime.Token); }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Program.Log(ex.ToString());
                ShowError(TrackerEngine.FriendlyError(ex));
                if (Program.TestMode) { Directory.CreateDirectory(_qaDirectory); await File.WriteAllTextAsync(Path.Combine(_qaDirectory, "failure.txt"), ex.ToString()); Environment.ExitCode = 1; Quit(); }
            }
        };
        RenderState();
        LoadSettingsFields();
        SizeChanged += (_, e) => { if (e.NewSize.Height < 450) SetHistoryVisible(false); C<Border>("SettingsPanel").MaxHeight = Math.Max(170, e.NewSize.Height - 120); };
        if (_engine.Storage.RecoveryMessage is { } warning) ShowError(warning);
        Program.Log("window constructed");
    }

    private void InitializeTray()
    {
        _tray = new WindowsTray(() => Dispatcher.UIThread.Post(Restore),
            () => Dispatcher.UIThread.Post(() => _engine.TogglePause()), () => Dispatcher.UIThread.Post(Quit));
        Program.Log("tray initialized");
    }

    private T C<T>(string name) where T : Control => this.FindControl<T>(name) ?? throw new InvalidOperationException(name);
    private void ShowError(string message) { C<TextBlock>("ErrorText").Text = message; C<Border>("ErrorPanel").IsVisible = true; }
    private void ClearError() => C<Border>("ErrorPanel").IsVisible = false;
    private void RenderState()
    {
        C<TextBlock>("EmptyState").IsVisible = _engine.Data.Channels.Count == 0;
        C<ItemsControl>("ChannelList").ItemsSource = _engine.Data.Channels.Select(c => new ChannelRow(c)).ToList();
        C<ItemsControl>("HistoryList").ItemsSource = _engine.Data.History.AsEnumerable().Reverse().Select(e => new HistoryRow(e)).ToList();
        C<TextBlock>("HistoryEmpty").IsVisible = _engine.Data.History.Count == 0;
        C<TextBlock>("HistoryCount").Text = "Журнал · " + _engine.Data.History.Count;
        C<Button>("RefreshButton").IsEnabled = !_engine.Busy;
        C<Button>("PauseButton").Content = _engine.Paused ? "▶" : "Ⅱ";
    }

    private async void AddChannel(object? s, RoutedEventArgs e) => await AddFromInputAsync();
    private async void OnInputKey(object? s, KeyEventArgs e) { if (e.Key == Key.Enter) { e.Handled = true; await AddFromInputAsync(); } }
    private async Task AddFromInputAsync()
    {
        if (_adding) return;
        _adding = true;
        ClearError();
        C<Button>("AddButton").IsEnabled = false;
        try { await _engine.AddAsync(C<TextBox>("ChannelInput").Text ?? "", _lifetime.Token); C<TextBox>("ChannelInput").Text = ""; RenderState(); }
        catch (Exception ex) { ShowError(TrackerEngine.FriendlyError(ex)); }
        finally { _adding = false; C<Button>("AddButton").IsEnabled = true; }
    }
    private async void RemoveChannel(object? s, RoutedEventArgs e) { if (s is Button { Tag: string login }) await Guard(() => _engine.RemoveAsync(login)); }
    private void OpenChannel(object? s, RoutedEventArgs e) { if (s is Button { Tag: string login }) OpenExternal("https://www.twitch.tv/" + Uri.EscapeDataString(login)); }
    private async void Refresh(object? s, RoutedEventArgs e) => await Guard(() => _engine.PollAsync(_lifetime.Token));
    private void Pause(object? s, RoutedEventArgs e) => _engine.TogglePause();
    private void ToggleHistory(object? s, RoutedEventArgs e) => SetHistoryVisible(!C<ScrollViewer>("HistoryContent").IsVisible);
    private void SetHistoryVisible(bool visible) { C<ScrollViewer>("HistoryContent").IsVisible = visible; C<Button>("HistoryToggle").Content = visible ? "⌃" : "⌄"; }
    private async void ClearHistory(object? s, RoutedEventArgs e) => await Guard(() => _engine.ClearHistoryAsync());
    private async void ExportHistory(object? s, RoutedEventArgs e) => await Guard(async () => { await _engine.ExportAsync(); OpenDataFolder(); });
    private void OpenData(object? s, RoutedEventArgs e) => OpenDataFolder();
    private void OpenDataFolder()
    {
        try
        {
            ClearError();
            var request = PortableData.OpenFolderRequest(_engine.Storage.DirectoryPath);
            if (!Program.TestMode) Process.Start(request);
            _lastOpenedFolder = _engine.Storage.DirectoryPath;
        }
        catch (Exception ex) { ShowError("Не удалось открыть папку: " + ex.Message); }
    }
    private async void ToggleChannelNotifications(object? s, RoutedEventArgs e)
    {
        if (s is CheckBox { Tag: string login } toggle)
            await Guard(() => _engine.SetChannelNotificationsAsync(login, toggle.IsChecked == true));
    }
    private void ToggleSettings(object? s, RoutedEventArgs e)
    { var panel = C<Border>("SettingsPanel"); panel.IsVisible = !panel.IsVisible; if (panel.IsVisible) LoadSettingsFields(); }
    private void LoadSettingsFields()
    {
        var settings = _engine.Data.Settings;
        C<NumericUpDown>("IntervalInput").Value = settings.IntervalSeconds;
        C<CheckBox>("NotificationsInput").IsChecked = settings.Notifications;
        C<CheckBox>("OnlineInput").IsChecked = settings.NotifyOnline;
        C<CheckBox>("TrayInput").IsChecked = settings.CloseToTray;
    }
    private async void SaveSettings(object? s, RoutedEventArgs e) => await Guard(async () =>
    {
        await _engine.UpdateSettingsAsync(settings =>
        {
            settings.IntervalSeconds = Math.Clamp((int)(C<NumericUpDown>("IntervalInput").Value ?? 30), 30, 3600);
            settings.Notifications = C<CheckBox>("NotificationsInput").IsChecked == true;
            settings.NotifyOnline = C<CheckBox>("OnlineInput").IsChecked == true;
            settings.CloseToTray = C<CheckBox>("TrayInput").IsChecked == true;
        });
        C<Border>("SettingsPanel").IsVisible = false;
    });
    private void TestNotification(object? sender, RoutedEventArgs e)
    {
        ClearError();
        try
        {
            if (_tray is null) throw new InvalidOperationException("Значок в трее ещё не готов. Повторите проверку через секунду.");
            _tray.ShowNotification("Тестовое уведомление", "Twitch Category Tracker: так будут выглядеть уведомления о смене категории.");
            _testNotificationsSent++;
        }
        catch (Exception ex) { ShowError(ex.Message); }
    }
    private void ShowNotification(string title, string message)
    { try { _tray?.ShowNotification(title, message); } catch (Exception ex) { ShowError(ex.Message); } }
    private async Task Guard(Func<Task> action) { ClearError(); try { await action(); } catch (Exception ex) { ShowError(TrackerEngine.FriendlyError(ex)); } }
    private void OpenExternal(string target) { try { if (!Program.TestMode) Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); } catch (Exception ex) { ShowError(ex.Message); } }
    private void Restore() { Show(); WindowState = WindowState.Normal; Activate(); }
    private void Quit()
    {
        if (_quitting) return;
        _quitting = true;
        _lifetime.Cancel();
        _tray?.Dispose();
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }

    private async Task RunQaAsync()
    {
        Directory.CreateDirectory(_qaDirectory);
        await CaptureAsync("compact-empty.png");
        foreach (var login in new[] { "ethenpov", "twitchdev" })
        {
            C<TextBox>("ChannelInput").Text = login;
            await AddFromInputAsync();
            if (!_engine.Data.Channels.Any(c => c.Login == login)) throw new Exception("UI add failed: " + login);
        }
        await _engine.PollAsync(_lifetime.Token);
        if (_engine.Data.Channels.Any(c => c.Error is not null)) throw new Exception("Live Twitch poll failed");
        var historyCount = _engine.Data.History.Count;
        await _engine.PollAsync(_lifetime.Token);
        if (_engine.Data.History.Count != historyCount) throw new Exception("Repeated unchanged poll created duplicate history");
        await Task.Delay(150);
        UpdateLayout();
        var channelToggle = this.GetVisualDescendants().OfType<CheckBox>().Single(c => c.Tag is string login && login == "ethenpov");
        await ClickAsync(channelToggle);
        if (new Storage(_engine.Storage.DirectoryPath).Load().Channels.Single(c => c.Login == "ethenpov").Notifications)
            throw new Exception("Per-channel notification setting did not persist");
        await CaptureAsync("compact-channels.png");
        C<Border>("SettingsPanel").IsVisible = true;
        await Task.Delay(100);
        UpdateLayout();
        await ClickAsync(C<Button>("OpenDataButton"));
        if (_lastOpenedFolder != _engine.Storage.DirectoryPath) throw new Exception("Data-folder button used the wrong directory");
        if (Program.NativeSmoke)
        {
            await ClickAsync(C<Button>("TestNotificationButton"));
            if (_testNotificationsSent != 1) throw new Exception("Windows rejected the test notification");
        }
        await CaptureAsync("compact-settings.png");
        C<Border>("SettingsPanel").IsVisible = false;
        Width = 620; Height = 400;
        await CaptureAsync("compact-minimum.png");
        var reloaded = new Storage(_engine.Storage.DirectoryPath).Load();
        if (reloaded.Channels.Count != 2) throw new Exception("Persistence failed");
        await _engine.ExportAsync();
        await _engine.RemoveAsync("twitchdev");
        if (_engine.Data.Channels.Count != 1) throw new Exception("Remove failed");
        WindowState = WindowState.Minimized;
        await Task.Delay(50);
        if (IsVisible) throw new Exception("Minimized window was not hidden to tray");
        Restore();
        if (!IsVisible || WindowState != WindowState.Normal) throw new Exception("Tray restore failed");
        var report = new UiSmokeReport(true, DateTimeOffset.UtcNow, reloaded.Channels, historyCount, 760, 520,
            new[] { "add ethenpov", "live polling", "deduplication", "persistence", "per-channel notifications", "data-folder button", "remove", "export", "compact rendering", "minimize to tray and restore" },
            Program.NativeSmoke ? "Native Windows rendering and native tray creation; actual notification display still requires interactive verification."
                : "Headless rendering; actual Windows notification display still requires interactive verification.");
        await File.WriteAllTextAsync(Path.Combine(_qaDirectory, Program.NativeSmoke ? "aot-result.json" : "compact-result.json"),
            JsonSerializer.Serialize(report, DesktopJsonContext.Default.UiSmokeReport));
        Quit();
    }
    private async Task CaptureAsync(string name)
    {
        await Task.Delay(150);
        UpdateLayout();
#if UI_CHECKS
        using Bitmap? screenshot = Program.QaMode ? this.CaptureRenderedFrame() : RenderFrame();
#else
        using var screenshot = RenderFrame();
#endif
        if (screenshot is null) throw new Exception("No rendered frame: " + name);
        using var file = File.Create(Path.Combine(_qaDirectory, name));
        screenshot.Save(file, PngBitmapEncoderOptions.Default);
    }
    private RenderTargetBitmap RenderFrame()
    {
        var bitmap = new RenderTargetBitmap(new PixelSize((int)Math.Ceiling(Bounds.Width), (int)Math.Ceiling(Bounds.Height)), new Vector(96, 96));
        bitmap.Render(this);
        return bitmap;
    }
    private async Task ClickAsync(Control control)
    {
        var point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), this)
            ?? throw new Exception("Cannot locate test control");
#if UI_CHECKS
        if (Program.QaMode) { this.MouseDown(point, MouseButton.Left); this.MouseUp(point, MouseButton.Left); }
        else
#endif
        {
            if (control is CheckBox checkBox) checkBox.IsChecked = checkBox.IsChecked != true;
            control.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        await Task.Delay(100);
    }
}

public sealed class ChannelRow(WatchedChannel channel)
{
    public string Login => channel.Login;
    public bool Notifications => channel.Notifications;
    public string Name => channel.Snapshot?.DisplayName ?? channel.Login;
    public string Status => channel.Error is not null ? "● Устарело" : channel.Snapshot?.Live == true ? "● В эфире" : "○ Офлайн";
    public IBrush StatusColor => Brush.Parse(channel.Error is not null ? "#E6BD79" : channel.Snapshot?.Live == true ? "#8CD9B8" : "#8994AC");
    public string Category => channel.Snapshot?.Live == true ? channel.Snapshot.Category ?? "Без категории" : "—";
    public string Detail => channel.Error ?? ((channel.Snapshot?.Title is { } title ? title + "\n" : "") + "Проверено " + channel.CheckedAt?.ToLocalTime().ToString("HH:mm:ss"));
}
public sealed class HistoryRow(HistoryEntry entry)
{
    public string Time => entry.At.ToLocalTime().ToString("HH:mm:ss");
    public string Login => entry.Login;
    public string Message => entry.Message;
}

public sealed record UiSmokeReport(bool Ok, DateTimeOffset CheckedAt, System.Collections.Generic.List<WatchedChannel> Channels,
    int HistoryEntries, int Width, int Height, string[] Checks, string Limitations);

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(UiSmokeReport))]
internal partial class DesktopJsonContext : JsonSerializerContext;
