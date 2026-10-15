# Twitch Category Tracker 2

Compact Windows utility built with C# / .NET 10 and Avalonia 12. Add a Twitch channel name or URL and track category changes, stream starts, and stream endings. A 760×520 window contains the channel list, a collapsible event log, and a small settings panel. Russian interface, tray menu, Windows notifications, local history, and JSON export. No Twitch account or sign-in flow. Each channel has its own notification checkbox in the **Увед.** column; muted channels are still tracked and recorded in history. The global notification setting overrides all channel switches.

## Run

Open `dist/TwitchCategoryTracker/TwitchCategoryTracker.exe` after building, or use `Launch.cmd`. The published Windows x64 build includes .NET; users do not need a separate runtime or developer tools. Minimizing hides the window to the tray; double-clicking its icon restores it. Closing the window keeps monitoring in the tray by default; use the tray menu's **Выход** to quit.

No Twitch account, personal API keys, password, or cookies are needed. The provider reads public channel data through Twitch's internal website GraphQL endpoint. It is unofficial and may break when Twitch changes its website. Network errors preserve the last good observation; stale observations are visibly marked and do not generate category-change events.

The app polls every 30 seconds by default (configurable up to 3600 seconds). A single cycle never overlaps another. In guest mode channels are checked sequentially, so large lists or slow networks add to the time between observations. Polling can miss changes that occur between checks. History starts when a channel is added; the app does not retrieve earlier Twitch history.

## Data

Settings, the channel list, notification switches, logs, exports and the latest 1000 events are stored in **`data/` beside the EXE**. Copy the application together with this folder to preserve its state. The previous JSON save is retained as `tracker.json.bak`. Malformed JSON is copied to a timestamped `.damaged-*` file before recovery. History exports contain channel names and events. No account sessions or tokens are obtained or stored. The **Папка данных** button opens this directory explicitly through Windows Explorer.

On the first portable launch, existing data from `%LOCALAPPDATA%/TwitchCategoryTracker/v2/` is copied into `data/` if no portable state exists. The old files are preserved. Subsequent launches use the portable directory and never overwrite it from AppData. The EXE directory must be writable.

Existing WinForms sources remain in `WindowsFormsApp10/`. The new app uses a separate data directory and does not import or modify the old plaintext `settings.txt` credentials.

## Build and verify

Install the .NET 10 SDK, then run `./build.ps1` in PowerShell on Windows. Visual Studio and C++ Build Tools are not required. If a project-local SDK exists at `.tooling/dotnet`, the build script uses it.

```powershell
dotnet run --project modern/Tracker.Tests -c Release
dotnet build modern/Tracker.slnx -c Release
./build.ps1
```

For a UI smoke check with live public Twitch requests:

```powershell
dotnet modern/Tracker.Desktop/bin/Release/net10.0-windows/TwitchCategoryTracker.dll --qa
```

This creates isolated QA data and four compact-layout screenshots under `qa-output/`, checks live/offline fetching, repeated-poll deduplication, persistence, removal, export, and actual headless mouse clicks on the per-channel notification checkbox and data-folder button. It uses Avalonia's headless renderer; folder launches are intercepted in QA. It does not prove Windows notification delivery, interactive tray actions, or long-running reliability. Core checks also exercise portable migration and preservation of existing state.

For an observable production-code check of a real channel (three samples, thirty seconds apart):

```powershell
dotnet run --project modern/Tracker.Probe -c Release -- ethenpov 3 30
```

The probe uses the same engine as the utility and rereads its saved JSON after every observation. It writes `observations.jsonl` and `tracker.json` to an isolated directory under `qa-output/`. Compare category ID and category name; CSS classes such as `CoreText-sc-...` are presentation details and are not used.

On 15 October 2026, three live observations of `ethenpov` returned `HITMAN World of Assassination`, ID `1780647523`, with one baseline event and no duplicates. No real category transition occurred during that observation. Category transitions, disk persistence, and network failure recovery are tested separately using simulated HTTP responses.

`tools/Test-TwitchAccess.ps1` is an independent public endpoint probe. See [`AUDIT-2026-10-15.txt`](../AUDIT-2026-10-15.txt) for the legacy code audit and validation boundaries.
