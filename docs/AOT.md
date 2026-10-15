# Native AOT deployment

Starting with v2.1.0, the main release uses Native AOT and is published into `dist/aot/`. The managed build remains available through `build.ps1`. Keep the EXE and the three graphics DLLs together.

Changes required for AOT:

- Replace the Windows Forms tray dependency with a Win32 tray adapter using source-generated interop.
- Compile Avalonia data bindings with explicit template types.
- Generate JSON serialization metadata for saved state, exports and Twitch requests.
- Exclude Avalonia.Headless from the AOT build; keep headless checks available in managed builds.

Build on Windows with the .NET 10 SDK and the Visual Studio C++ build tools:

```powershell
./build-aot.ps1
```

The script can also use this workspace's project-local LLVM linker and downloaded MSVC/Windows SDK libraries. These toolchain files are not distributed with the application.

The EXE and the graphics DLLs form one portable application. Keep them together; copying just the EXE is insufficient. Data is stored in `data/` beside this separate build.

Run a native smoke check with isolated data:

```powershell
./dist/aot/TwitchCategoryTracker.exe --smoke
```

This uses a real Windows window and tray, live Twitch requests, compiled UI bindings, JSON persistence, per-channel notification settings, export, and minimize/restore. It captures rendered views and writes `qa-output/aot-result.json`. Actual notification visibility, manual context-menu interaction and prolonged unattended use require separate interactive acceptance.
