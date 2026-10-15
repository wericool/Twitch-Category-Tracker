#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

mod model;
mod notify;
mod storage;
mod twitch;

use model::{compare, normalize_login, now_rfc3339, HistoryEntry, Settings, TrackerData, WatchedChannel};
use serde_json::json;
use std::path::PathBuf;
use std::process::Command;
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::{Arc, Mutex, OnceLock};
use std::time::Duration;
use storage::Storage;
use tauri::menu::{Menu, MenuItem};
use tauri::tray::{TrayIconBuilder, TrayIconEvent};
use tauri::{AppHandle, Emitter, Manager, State, WindowEvent};
use tokio::sync::Notify;


struct Tracker {
    data: Mutex<TrackerData>,
    storage: Storage,
    twitch: twitch::Twitch,
    paused: AtomicBool,
    busy: AtomicBool,
    last_poll: Mutex<Option<String>>,
    recovery: Option<String>,
    wake: Notify,
    polling: tokio::sync::Mutex<()>,
    handle: OnceLock<AppHandle>,
}

type Shared = Arc<Tracker>;

impl Tracker {
    fn view(&self) -> serde_json::Value {
        let data = self.data.lock().unwrap();
        json!({
            "channels": data.channels,
            "history": data.history,
            "settings": data.settings,
            "paused": self.paused.load(Ordering::SeqCst),
            "busy": self.busy.load(Ordering::SeqCst),
            "lastPoll": *self.last_poll.lock().unwrap(),
            "dataDir": self.storage.dir.display().to_string(),
            "recovery": self.recovery,
        })
    }

    fn emit(&self) {
        if let Some(h) = self.handle.get() {
            let _ = h.emit("state", self.view());
        }
    }

    fn save(&self) -> Result<(), String> {
        let data = self.data.lock().unwrap();
        self.storage.save(&data).map_err(|e| format!("Не удалось сохранить данные: {e}"))
    }

    /// Применяет свежий снимок; уведомляет, если это включено.
    fn apply(&self, login: &str, snapshot: model::Snapshot) {
        let now = now_rfc3339();
        let mut notify: Option<(String, HistoryEntry)> = None;
        {
            let mut data = self.data.lock().unwrap();
            let settings = data.settings.clone();
            let Some(channel) = data.channels.iter_mut().find(|c| c.login == login) else { return };
            let entry = compare(channel.snapshot.as_ref(), &snapshot, &now);
            let display = snapshot.display_name.clone();
            channel.snapshot = Some(snapshot);
            channel.checked_at = Some(now);
            channel.error = None;
            let wanted = channel.notifications;
            if let Some(entry) = entry {
                let send = settings.notifications && wanted && (entry.kind == "category" || (settings.notify_online && matches!(entry.kind.as_str(), "online" | "offline")));
                if send {
                    notify = Some((display, entry.clone()));
                }
                data.history.push(entry);
                if data.history.len() > 1000 {
                    let extra = data.history.len() - 1000;
                    data.history.drain(..extra);
                }
            }
        }
        if let Some((title, entry)) = notify {
            if let Some(app) = self.handle.get() {
                let _ = toast(app, &title, &entry.message);
            }
        }
    }

    async fn poll(&self) {
        let Ok(_guard) = self.polling.try_lock() else { return };
        let logins: Vec<String> = self.data.lock().unwrap().channels.iter().map(|c| c.login.clone()).collect();
        if logins.is_empty() {
            return;
        }
        self.busy.store(true, Ordering::SeqCst);
        self.emit();
        for login in logins {
            match self.twitch.channel(&login).await {
                Ok(Some(snapshot)) => self.apply(&login, snapshot),
                Ok(None) => self.set_error(&login, "Канал больше не найден".into()),
                Err(e) => self.set_error(&login, e),
            }
        }
        *self.last_poll.lock().unwrap() = Some(now_rfc3339());
        self.busy.store(false, Ordering::SeqCst);
        let _ = self.save();
        self.emit();
    }

    fn set_error(&self, login: &str, error: String) {
        if let Some(c) = self.data.lock().unwrap().channels.iter_mut().find(|c| c.login == login) {
            c.error = Some(error);
        }
    }
}

fn toast(app: &AppHandle, title: &str, message: &str) -> Result<(), String> {
    let hwnd = app.get_webview_window("main").ok_or("Окно не найдено.")?.hwnd().map_err(|e| e.to_string())?;
    notify::balloon(hwnd.0 as isize, title, message)
}

#[tauri::command]
fn get_state(t: State<Shared>) -> serde_json::Value {
    t.view()
}

#[tauri::command]
async fn add_channel(t: State<'_, Shared>, input: String) -> Result<(), String> {
    let login = normalize_login(&input)?;
    {
        let data = t.data.lock().unwrap();
        if data.channels.iter().any(|c| c.login == login) {
            return Err("Этот стример уже в списке.".into());
        }
        if data.channels.len() >= 100 {
            return Err("Можно отслеживать до 100 каналов.".into());
        }
    }
    let snapshot = t.twitch.channel(&login).await?.ok_or("Канал не найден. Проверьте ник стримера.")?;
    t.data.lock().unwrap().channels.push(WatchedChannel { login: login.clone(), ..Default::default() });
    t.apply(&login, snapshot);
    t.last_poll.lock().unwrap().get_or_insert_with(now_rfc3339);
    let saved = t.save();
    t.emit();
    saved
}

#[tauri::command]
fn remove_channel(t: State<Shared>, login: String) -> Result<(), String> {
    t.data.lock().unwrap().channels.retain(|c| c.login != login);
    let saved = t.save();
    t.emit();
    saved
}

#[tauri::command]
fn set_channel_notifications(t: State<Shared>, login: String, enabled: bool) -> Result<(), String> {
    if let Some(c) = t.data.lock().unwrap().channels.iter_mut().find(|c| c.login == login) {
        c.notifications = enabled;
    }
    let saved = t.save();
    t.emit();
    saved
}

#[tauri::command]
fn save_settings(t: State<Shared>, settings: Settings) -> Result<(), String> {
    {
        let mut data = t.data.lock().unwrap();
        data.settings = Settings { interval_seconds: settings.interval_seconds.clamp(30, 3600), ..settings };
    }
    t.wake.notify_one();
    let saved = t.save();
    t.emit();
    saved
}

#[tauri::command]
fn refresh(t: State<Shared>) {
    t.wake.notify_one();
}

#[tauri::command]
fn toggle_pause(t: State<Shared>) {
    t.paused.fetch_xor(true, Ordering::SeqCst);
    t.wake.notify_one();
    t.emit();
}

#[tauri::command]
fn clear_history(t: State<Shared>) -> Result<(), String> {
    t.data.lock().unwrap().history.clear();
    let saved = t.save();
    t.emit();
    saved
}

#[tauri::command]
fn export_history(t: State<Shared>) -> Result<(), String> {
    let stamp = now_rfc3339().replace([':', '.', '+', '-', 'T'], "");
    let path = t.storage.dir.join(format!("history-{stamp}.json"));
    let history = t.data.lock().unwrap().history.clone();
    std::fs::write(&path, serde_json::to_string_pretty(&history).map_err(|e| e.to_string())?).map_err(|e| e.to_string())?;
    let _ = Command::new("explorer.exe").arg(format!("/select,{}", path.display())).spawn();
    Ok(())
}

#[tauri::command]
fn open_data_folder(t: State<Shared>) -> Result<(), String> {
    Command::new("explorer.exe").arg(&t.storage.dir).spawn().map(|_| ()).map_err(|e| format!("Не удалось открыть папку: {e}"))
}

#[tauri::command]
fn open_channel(login: String) -> Result<(), String> {
    let login = normalize_login(&login)?;
    Command::new("explorer.exe").arg(format!("https://www.twitch.tv/{login}")).spawn().map(|_| ()).map_err(|e| e.to_string())
}

#[tauri::command]
fn test_notification(app: AppHandle) -> Result<(), String> {
    toast(&app, "Тестовое уведомление", "Так будут выглядеть уведомления о смене категории.")
}

fn show_main(app: &AppHandle) {
    if let Some(w) = app.get_webview_window("main") {
        let _ = w.show();
        let _ = w.unminimize();
        let _ = w.set_focus();
    }
}

fn main() {
    let exe_dir = std::env::current_exe().ok().and_then(|p| p.parent().map(PathBuf::from)).unwrap_or_default();
    let dir = match std::env::args().skip_while(|a| a != "--data").nth(1) {
        Some(custom) => PathBuf::from(custom),
        None => {
            let legacy = std::env::var_os("LOCALAPPDATA").map(|b| PathBuf::from(b).join("TwitchCategoryTracker").join("v2"));
            storage::prepare_portable(&exe_dir, legacy.as_deref())
        }
    };
    let storage = Storage { dir };
    let (data, recovery) = storage.load();
    let tracker: Shared = Arc::new(Tracker {
        data: Mutex::new(data),
        storage,
        twitch: twitch::Twitch::new(),
        paused: AtomicBool::new(false),
        busy: AtomicBool::new(false),
        last_poll: Mutex::new(None),
        recovery,
        wake: Notify::new(),
        polling: tokio::sync::Mutex::new(()),
        handle: OnceLock::new(),
    });
    let background = tracker.clone();

    tauri::Builder::default()
        .plugin(tauri_plugin_single_instance::init(|app, _, _| show_main(app)))
        .manage(tracker)
        .invoke_handler(tauri::generate_handler![
            get_state, add_channel, remove_channel, set_channel_notifications, save_settings, refresh, toggle_pause,
            clear_history, export_history, open_data_folder, open_channel, test_notification
        ])
        .setup(move |app| {
            let _ = background.handle.set(app.handle().clone());
            let t = background.clone();
            tauri::async_runtime::spawn(async move {
                loop {
                    if !t.paused.load(Ordering::SeqCst) {
                        t.poll().await;
                    }
                    let secs = t.data.lock().unwrap().settings.interval_seconds.max(30) as u64;
                    tokio::select! {
                        _ = tokio::time::sleep(Duration::from_secs(secs)) => {}
                        _ = t.wake.notified() => {}
                    }
                }
            });
            let open = MenuItem::with_id(app, "show", "Открыть", true, None::<&str>)?;
            let pause = MenuItem::with_id(app, "pause", "Пауза / продолжить", true, None::<&str>)?;
            let quit = MenuItem::with_id(app, "quit", "Выход", true, None::<&str>)?;
            let menu = Menu::with_items(app, &[&open, &pause, &quit])?;
            let mut tray = TrayIconBuilder::with_id("main")
                .tooltip("Twitch Category Tracker")
                .menu(&menu)
                .show_menu_on_left_click(false)
                .on_menu_event(|app, event| match event.id.as_ref() {
                    "show" => show_main(app),
                    "pause" => {
                        let t = app.state::<Shared>();
                        t.paused.fetch_xor(true, Ordering::SeqCst);
                        t.wake.notify_one();
                        t.emit();
                    }
                    "quit" => app.exit(0),
                    _ => {}
                })
                .on_tray_icon_event(|tray, event| {
                    if let TrayIconEvent::DoubleClick { .. } = event {
                        show_main(tray.app_handle());
                    }
                });
            if let Some(icon) = app.default_window_icon() {
                tray = tray.icon(icon.clone());
            }
            tray.build(app)?;
            Ok(())
        })
        .on_window_event(|window, event| match event {
            WindowEvent::CloseRequested { api, .. } => {
                if window.app_handle().state::<Shared>().data.lock().unwrap().settings.close_to_tray {
                    api.prevent_close();
                    let _ = window.hide();
                } else {
                    window.app_handle().exit(0);
                }
            }
            WindowEvent::Resized(_) => {
                if window.is_minimized().unwrap_or(false) {
                    let _ = window.hide();
                }
            }
            _ => {}
        })
        .run(tauri::generate_context!())
        .expect("ошибка запуска приложения");
}
