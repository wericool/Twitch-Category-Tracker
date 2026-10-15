use serde::{Deserialize, Serialize};
use std::time::{SystemTime, UNIX_EPOCH};

// Формат tracker.json совместим с версией 2 (C#): те же имена полей.

#[derive(Clone, Serialize, Deserialize, Default, Debug, PartialEq)]
#[serde(rename_all = "PascalCase", default)]
pub struct Snapshot {
    pub login: String,
    pub display_name: String,
    pub live: bool,
    pub category_id: Option<String>,
    pub category: Option<String>,
    pub title: Option<String>,
    pub viewers: i64,
}

#[derive(Clone, Serialize, Deserialize, Debug)]
#[serde(rename_all = "PascalCase", default)]
pub struct WatchedChannel {
    pub login: String,
    pub notifications: bool,
    pub snapshot: Option<Snapshot>,
    pub checked_at: Option<String>,
    pub error: Option<String>,
}

impl Default for WatchedChannel {
    fn default() -> Self {
        Self { login: String::new(), notifications: true, snapshot: None, checked_at: None, error: None }
    }
}

#[derive(Clone, Serialize, Deserialize, Debug, Default)]
#[serde(rename_all = "PascalCase", default)]
pub struct HistoryEntry {
    pub at: String,
    pub login: String,
    pub kind: String,
    pub message: String,
}

#[derive(Clone, Serialize, Deserialize, Debug)]
#[serde(rename_all = "PascalCase", default)]
pub struct Settings {
    pub interval_seconds: i64,
    pub notifications: bool,
    pub notify_online: bool,
    pub close_to_tray: bool,
}

impl Default for Settings {
    fn default() -> Self {
        Self { interval_seconds: 30, notifications: true, notify_online: true, close_to_tray: true }
    }
}

#[derive(Clone, Serialize, Deserialize, Debug)]
#[serde(rename_all = "PascalCase", default)]
pub struct TrackerData {
    pub version: i32,
    pub settings: Settings,
    pub channels: Vec<WatchedChannel>,
    pub history: Vec<HistoryEntry>,
}

impl Default for TrackerData {
    fn default() -> Self {
        Self { version: 1, settings: Settings::default(), channels: Vec::new(), history: Vec::new() }
    }
}

/// Ник или ссылка twitch.tv/… → логин в нижнем регистре.
pub fn normalize_login(input: &str) -> Result<String, String> {
    let mut value = input.trim().to_string();
    let lower = value.to_lowercase();
    if lower.contains("://") || lower.starts_with("twitch.tv") || lower.starts_with("www.twitch.tv") || lower.starts_with("m.twitch.tv") {
        let rest = lower.split("://").last().unwrap_or("");
        let (host, path) = rest.split_once('/').unwrap_or((rest, ""));
        if !matches!(host, "twitch.tv" | "www.twitch.tv" | "m.twitch.tv") {
            return Err("Нужна ссылка на Twitch или ник стримера.".into());
        }
        value = path.split(['/', '?', '#']).next().unwrap_or("").to_string();
    }
    let value = value.trim_start_matches('@').to_lowercase();
    let ok = (1..=25).contains(&value.len()) && value.bytes().all(|b| b.is_ascii_lowercase() || b.is_ascii_digit() || b == b'_');
    if ok { Ok(value) } else { Err("Введите ник Twitch: латинские буквы, цифры и подчёркивание.".into()) }
}

/// Что изменилось между двумя проверками (None — ничего).
pub fn compare(previous: Option<&Snapshot>, current: &Snapshot, now: &str) -> Option<HistoryEntry> {
    let cat = |s: &Snapshot| s.category.clone().unwrap_or_else(|| "Без категории".into());
    let entry = |kind: &str, message: String| HistoryEntry { at: now.into(), login: current.login.clone(), kind: kind.into(), message };
    let Some(prev) = previous else {
        return Some(entry("initial", if current.live { format!("Отслеживание начато · {}", cat(current)) } else { "Отслеживание начато · Не в эфире".into() }));
    };
    if prev.live != current.live {
        return Some(if current.live { entry("online", format!("В эфире · {}", cat(current))) } else { entry("offline", "Трансляция завершена".into()) });
    }
    if current.live && (prev.category_id != current.category_id || prev.category != current.category) {
        return Some(entry("category", format!("{} → {}", cat(prev), cat(current))));
    }
    None
}

pub fn now_rfc3339() -> String {
    let d = SystemTime::now().duration_since(UNIX_EPOCH).unwrap_or_default();
    let secs = d.as_secs() as i64;
    let (y, m, day) = civil(secs.div_euclid(86400));
    let rem = secs.rem_euclid(86400);
    format!("{y:04}-{m:02}-{day:02}T{:02}:{:02}:{:02}.{:03}+00:00", rem / 3600, rem % 3600 / 60, rem % 60, d.subsec_millis())
}

fn civil(days: i64) -> (i64, i64, i64) {
    let z = days + 719_468;
    let era = z.div_euclid(146_097);
    let doe = z.rem_euclid(146_097);
    let yoe = (doe - doe / 1460 + doe / 36_524 - doe / 146_096) / 365;
    let doy = doe - (365 * yoe + yoe / 4 - yoe / 100);
    let mp = (5 * doy + 2) / 153;
    let d = doy - (153 * mp + 2) / 5 + 1;
    let m = if mp < 10 { mp + 3 } else { mp - 9 };
    (yoe + era * 400 + i64::from(m <= 2), m, d)
}

#[cfg(test)]
mod tests {
    use super::*;

    fn snap(live: bool, id: Option<&str>, cat: Option<&str>) -> Snapshot {
        Snapshot { login: "x".into(), display_name: "X".into(), live, category_id: id.map(Into::into), category: cat.map(Into::into), ..Default::default() }
    }

    #[test]
    fn normalizes_nicks_and_links() {
        assert_eq!(normalize_login("  @EthenPOV ").unwrap(), "ethenpov");
        assert_eq!(normalize_login("https://www.twitch.tv/TwitchDev/videos?x=1").unwrap(), "twitchdev");
        assert_eq!(normalize_login("twitch.tv/abc").unwrap(), "abc");
        assert!(normalize_login("https://example.com/abc").is_err());
        assert!(normalize_login("плохой ник").is_err());
        assert!(normalize_login("").is_err());
    }

    #[test]
    fn detects_changes() {
        let off = snap(false, None, None);
        let a = snap(true, Some("1"), Some("Chess"));
        let b = snap(true, Some("2"), Some("Just Chatting"));
        assert_eq!(compare(None, &a, "t").unwrap().kind, "initial");
        assert_eq!(compare(Some(&off), &a, "t").unwrap().kind, "online");
        assert_eq!(compare(Some(&a), &off, "t").unwrap().kind, "offline");
        let c = compare(Some(&a), &b, "t").unwrap();
        assert_eq!((c.kind.as_str(), c.message.as_str()), ("category", "Chess → Just Chatting"));
        assert!(compare(Some(&a), &a, "t").is_none());
        assert!(compare(Some(&off), &off, "t").is_none());
    }

    #[test]
    fn formats_time() {
        assert!(now_rfc3339().starts_with("20"));
        assert_eq!(civil(0), (1970, 1, 1));
        assert_eq!(civil(20_741), (2026, 10, 15));
    }
}
