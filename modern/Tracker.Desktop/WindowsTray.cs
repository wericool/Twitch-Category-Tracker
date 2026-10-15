using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Tracker.Desktop;

// A small, UI-thread-only Windows tray adapter. No Windows Forms runtime.
internal sealed unsafe partial class WindowsTray : IDisposable
{
    private const uint CallbackMessage = 0x8001;
    private const uint IconId = 1;
    private const string ClassName = "TwitchCategoryTracker.NativeTray";
    private static readonly Dictionary<nint, WindowsTray> Instances = new();
    private readonly Action _restore, _pause, _quit;
    private readonly uint _taskbarCreated;
    private nint _window, _icon;
    private bool _ownsIcon, _disposed;
    private NotifyIconData _data;

    public WindowsTray(Action restore, Action pause, Action quit)
    {
        _restore = restore; _pause = pause; _quit = quit;
        _taskbarCreated = RegisterWindowMessageW("TaskbarCreated");
        var instance = GetModuleHandleW(null);
        var className = Marshal.StringToHGlobalUni(ClassName);
        try
        {
            var windowClass = new WindowClass
            {
                Instance = instance, ClassName = className,
                WindowProcedure = (nint)(delegate* unmanaged[Stdcall]<nint, uint, nuint, nint, nint>)&WindowProc
            };
            var atom = RegisterClassW(ref windowClass);
            if (atom == 0 && Marshal.GetLastPInvokeError() != 1410) throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
        finally { Marshal.FreeHGlobal(className); }
        _window = CreateWindowExW(0, ClassName, "", 0x80000000, 0, 0, 0, 0, 0, 0, instance, 0);
        if (_window == 0) throw new Win32Exception(Marshal.GetLastPInvokeError());
        Instances.Add(_window, this);
        if (Environment.ProcessPath is { } path && ExtractIconExW(path, 0, out var large, out var small, 1) > 0)
        {
            _icon = small != 0 ? small : large;
            _ownsIcon = _icon != 0;
            if (small != 0 && large != 0) DestroyIcon(large);
        }
        if (_icon == 0) _icon = LoadIconW(0, 32512);
        _data.Size = (uint)sizeof(NotifyIconData);
        _data.Window = _window; _data.Id = IconId;
        _data.Flags = 1 | 2 | 4; // NIF_MESSAGE | NIF_ICON | NIF_TIP
        _data.Callback = CallbackMessage; _data.Icon = _icon;
        fixed (char* tip = _data.Tip) SetText(tip, 128, "Twitch Category Tracker");
        if (!Shell_NotifyIconW(0, ref _data)) { Dispose(); throw new Win32Exception("Windows не удалось создать значок в трее."); }
    }

    public void ShowNotification(string title, string message)
    {
        if (_disposed) return;
        _data.Flags = 0x10; // NIF_INFO
        _data.InfoFlags = 1; // NIIF_INFO, system notification sound
        fixed (char* text = _data.Info) SetText(text, 256, message);
        fixed (char* heading = _data.InfoTitle) SetText(heading, 64, title);
        if (!Shell_NotifyIconW(1, ref _data)) throw new Win32Exception("Windows не удалось передать уведомление.");
    }

    private void ShowMenu()
    {
        var menu = CreatePopupMenu();
        if (menu == 0) return;
        try
        {
            AppendMenuW(menu, 0, 1, "Открыть");
            AppendMenuW(menu, 0, 2, "Пауза / продолжить");
            AppendMenuW(menu, 0x800, 0, null);
            AppendMenuW(menu, 0, 3, "Выход");
            GetCursorPos(out var point);
            SetForegroundWindow(_window);
            var command = TrackPopupMenuEx(menu, 0x100 | 2, point.X, point.Y, _window, 0);
            PostMessageW(_window, 0, 0, 0);
            if (command == 1) _restore();
            else if (command == 2) _pause();
            else if (command == 3) _quit();
        }
        finally { DestroyMenu(menu); }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static nint WindowProc(nint window, uint message, nuint wParam, nint lParam)
    {
        try
        {
            if (Instances.TryGetValue(window, out var tray))
            {
                if (message == tray._taskbarCreated)
                {
                    tray._data.Flags = 1 | 2 | 4;
                    Shell_NotifyIconW(0, ref tray._data);
                    return 0;
                }
                if (message == CallbackMessage)
                {
                    if ((uint)lParam is 0x203 or 0x405) tray._restore(); // double click or balloon click
                    else if ((uint)lParam is 0x205 or 0x7B) tray.ShowMenu();
                    return 0;
                }
            }
        }
        catch (Exception ex) { Program.Log("tray callback: " + ex.Message); }
        return DefWindowProcW(window, message, wParam, lParam);
    }

    private static void SetText(char* target, int capacity, string value)
    {
        var count = Math.Min(value.Length, capacity - 1);
        if (count > 0 && char.IsHighSurrogate(value[count - 1])) count--;
        for (var i = 0; i < count; i++) target[i] = value[i];
        target[count] = '\0';
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_window != 0)
        {
            Shell_NotifyIconW(2, ref _data);
            Instances.Remove(_window);
            DestroyWindow(_window);
            _window = 0;
        }
        if (_ownsIcon) DestroyIcon(_icon);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowClass
    {
        public uint Style;
        public nint WindowProcedure;
        public int ClassExtra, WindowExtra;
        public nint Instance, Icon, Cursor, Background, MenuName, ClassName;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct NotifyIconData
    {
        public uint Size;
        public nint Window;
        public uint Id, Flags, Callback;
        public nint Icon;
        public fixed char Tip[128];
        public uint State, StateMask;
        public fixed char Info[256];
        public uint TimeoutOrVersion;
        public fixed char InfoTitle[64];
        public uint InfoFlags;
        public Guid ItemGuid;
        public nint BalloonIcon;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint { public int X, Y; }

    [LibraryImport("user32.dll", EntryPoint = "RegisterClassW", SetLastError = true)] private static partial ushort RegisterClassW(ref WindowClass value);
    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)] private static partial nint GetModuleHandleW(string? name);
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)] private static partial uint RegisterWindowMessageW(string name);
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)] private static partial nint CreateWindowExW(uint extended, string className, string name, uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [LibraryImport("user32.dll")] private static partial nint DefWindowProcW(nint window, uint message, nuint wParam, nint lParam);
    [LibraryImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool DestroyWindow(nint window);
    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)] private static partial uint ExtractIconExW(string path, int index, out nint large, out nint small, uint count);
    [LibraryImport("user32.dll")] private static partial nint LoadIconW(nint instance, nint id);
    [LibraryImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool DestroyIcon(nint icon);
    [LibraryImport("shell32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool Shell_NotifyIconW(uint operation, ref NotifyIconData data);
    [LibraryImport("user32.dll")] private static partial nint CreatePopupMenu();
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool AppendMenuW(nint menu, uint flags, nuint id, string? text);
    [LibraryImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool DestroyMenu(nint menu);
    [LibraryImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool GetCursorPos(out NativePoint point);
    [LibraryImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool SetForegroundWindow(nint window);
    [LibraryImport("user32.dll")] private static partial uint TrackPopupMenuEx(nint menu, uint flags, int x, int y, nint owner, nint parameters);
    [LibraryImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool PostMessageW(nint window, uint message, nuint wParam, nint lParam);
}
