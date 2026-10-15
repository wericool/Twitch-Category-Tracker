//! Уведомления Windows через системный значок в трее (как в прошлой версии): никаких ярлыков и регистраций,
//! Windows сам показывает имя и иконку программы.

use std::sync::atomic::{AtomicU32, Ordering};
use windows_sys::Win32::Foundation::HWND;
use windows_sys::Win32::UI::Shell::{
    ExtractIconW, Shell_NotifyIconW, NIF_ICON, NIF_INFO, NIF_TIP, NIIF_INFO, NIM_ADD, NIM_DELETE, NOTIFYICONDATAW,
};
use windows_sys::Win32::UI::WindowsAndMessaging::DestroyIcon;

static NEXT_ID: AtomicU32 = AtomicU32::new(9000);

fn wide<const N: usize>(text: &str) -> [u16; N] {
    let mut out = [0u16; N];
    for (slot, unit) in out.iter_mut().take(N - 1).zip(text.encode_utf16()) {
        *slot = unit;
    }
    out
}

/// Показывает уведомление и через несколько секунд убирает временный значок.
pub fn balloon(hwnd: isize, title: &str, text: &str) -> Result<(), String> {
    let exe = std::env::current_exe().map_err(|e| e.to_string())?;
    let path: Vec<u16> = exe.to_string_lossy().encode_utf16().chain(std::iter::once(0)).collect();
    let id = NEXT_ID.fetch_add(1, Ordering::SeqCst);
    unsafe {
        let icon = ExtractIconW(std::ptr::null_mut(), path.as_ptr(), 0);
        let mut data: NOTIFYICONDATAW = std::mem::zeroed();
        data.cbSize = std::mem::size_of::<NOTIFYICONDATAW>() as u32;
        data.hWnd = hwnd as HWND;
        data.uID = id;
        data.uFlags = NIF_ICON | NIF_TIP | NIF_INFO;
        data.hIcon = if (icon as usize) > 1 { icon } else { std::ptr::null_mut() };
        data.szTip = wide("Twitch Category Tracker");
        data.szInfoTitle = wide(title);
        data.szInfo = wide(text);
        data.dwInfoFlags = NIIF_INFO;
        if Shell_NotifyIconW(NIM_ADD, &data) == 0 {
            return Err("Не удалось показать уведомление.".into());
        }
        let (hwnd, icon) = (hwnd, data.hIcon as isize);
        std::thread::spawn(move || {
            std::thread::sleep(std::time::Duration::from_secs(10));
            let mut gone: NOTIFYICONDATAW = std::mem::zeroed();
            gone.cbSize = std::mem::size_of::<NOTIFYICONDATAW>() as u32;
            gone.hWnd = hwnd as HWND;
            gone.uID = id;
            Shell_NotifyIconW(NIM_DELETE, &gone);
            if icon != 0 {
                DestroyIcon(icon as _);
            }
        });
    }
    Ok(())
}
