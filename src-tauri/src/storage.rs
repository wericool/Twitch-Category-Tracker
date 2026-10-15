use crate::model::TrackerData;
use std::fs;
use std::io;
use std::path::{Path, PathBuf};

pub struct Storage {
    pub dir: PathBuf,
}

impl Storage {
    fn file(&self) -> PathBuf {
        self.dir.join("tracker.json")
    }

    /// Возвращает данные и предупреждение, если файл пришлось отложить как повреждённый.
    pub fn load(&self) -> (TrackerData, Option<String>) {
        let _ = fs::create_dir_all(&self.dir);
        let path = self.file();
        let Ok(text) = fs::read_to_string(&path) else { return (TrackerData::default(), None) };
        match serde_json::from_str::<TrackerData>(text.trim_start_matches('\u{feff}')) {
            Ok(mut data) if data.version == 1 => {
                data.settings.interval_seconds = data.settings.interval_seconds.clamp(30, 3600);
                if data.history.len() > 1000 {
                    data.history.drain(..data.history.len() - 1000);
                }
                let mut seen = std::collections::HashSet::new();
                data.channels.retain(|c| seen.insert(c.login.to_lowercase()));
                data.channels.truncate(100);
                (data, None)
            }
            _ => {
                let stamp = crate::model::now_rfc3339().replace([':', '.', '+', '-', 'T'], "");
                let _ = fs::copy(&path, path.with_extension(format!("json.damaged-{stamp}")));
                (TrackerData::default(), Some("Файл настроек повреждён. Исходный файл сохранён рядом с настройками.".into()))
            }
        }
    }

    /// Атомарная запись: временный файл, копия прошлой версии в `.bak`, замена.
    pub fn save(&self, data: &TrackerData) -> io::Result<()> {
        fs::create_dir_all(&self.dir)?;
        let path = self.file();
        let temp = path.with_extension("json.tmp");
        fs::write(&temp, serde_json::to_string_pretty(data).map_err(io::Error::other)?)?;
        if path.exists() {
            let _ = fs::copy(&path, path.with_extension("json.bak"));
        }
        fs::rename(&temp, &path)
    }
}

/// Данные лежат в папке `data` рядом с exe; данные старой версии подхватываются один раз.
pub fn prepare_portable(exe_dir: &Path, legacy: Option<&Path>) -> PathBuf {
    let dir = exe_dir.join("data");
    let _ = fs::create_dir_all(&dir);
    let tracker = dir.join("tracker.json");
    if !tracker.exists() {
        if let Some(old) = legacy.map(|l| l.join("tracker.json")).filter(|p| p.exists()) {
            let _ = fs::copy(old, &tracker);
        }
    }
    dir
}
