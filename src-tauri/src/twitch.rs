use crate::model::Snapshot;
use serde_json::{json, Value};
use std::sync::Mutex;
use std::time::Duration;

const QUERY: &str = "query Tracker($login: String!) { user(login: $login) { login displayName stream { id title viewersCount game { id name } } } }";

/// Гостевой доступ к публичным данным Twitch: токены и вход не нужны.
pub struct Twitch {
    http: reqwest::Client,
    client_id: Mutex<Option<String>>,
}

fn friendly(e: &reqwest::Error) -> String {
    if e.status().map(|s| s.as_u16()) == Some(429) {
        "Лимит Twitch. Следующая попытка — в следующем цикле.".into()
    } else if e.is_timeout() {
        "Twitch не ответил вовремя. Последние данные сохранены.".into()
    } else if e.is_decode() {
        "Twitch изменил формат ответа. Последние данные сохранены.".into()
    } else {
        "Нет связи с Twitch. Последние данные сохранены.".into()
    }
}

fn text(v: &Value, key: &str) -> Option<String> {
    v.get(key).and_then(Value::as_str).map(String::from)
}

impl Twitch {
    pub fn new() -> Self {
        let http = reqwest::Client::builder()
            .timeout(Duration::from_secs(20))
            .user_agent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) TwitchCategoryTracker/3")
            .build()
            .expect("http client");
        Self { http, client_id: Mutex::new(None) }
    }

    async fn ensure_client_id(&self, login: &str) -> Result<String, String> {
        if let Some(id) = self.client_id.lock().unwrap().clone() {
            return Ok(id);
        }
        let html = self.http.get(format!("https://www.twitch.tv/{login}")).send().await.and_then(|r| r.error_for_status()).map_err(|e| friendly(&e))?
            .text().await.map_err(|e| friendly(&e))?;
        let id = html.split("clientId=\"").nth(1).and_then(|rest| rest.split('"').next()).filter(|id| !id.is_empty() && id.bytes().all(|b| b.is_ascii_alphanumeric()));
        let id = id.ok_or("Twitch изменил формат сайта. Гостевой доступ временно недоступен.")?.to_string();
        *self.client_id.lock().unwrap() = Some(id.clone());
        Ok(id)
    }

    /// `Ok(None)` — такого канала нет.
    pub async fn channel(&self, login: &str) -> Result<Option<Snapshot>, String> {
        for attempt in 0..2 {
            let id = self.ensure_client_id(login).await?;
            let response = self.http.post("https://gql.twitch.tv/gql").header("Client-ID", id)
                .json(&json!({ "query": QUERY, "variables": { "login": login } }))
                .send().await.map_err(|e| friendly(&e))?;
            if attempt == 0 && matches!(response.status().as_u16(), 401 | 403) {
                *self.client_id.lock().unwrap() = None;
                continue;
            }
            let body: Value = response.error_for_status().map_err(|e| friendly(&e))?.json().await.map_err(|e| friendly(&e))?;
            if body.get("errors").is_some() {
                return Err("Twitch отклонил гостевой запрос. Повторим проверку позже.".into());
            }
            let user = &body["data"]["user"];
            if user.is_null() {
                return Ok(None);
            }
            let display = text(user, "displayName").unwrap_or_else(|| login.to_string());
            let stream = &user["stream"];
            if stream.is_null() {
                return Ok(Some(Snapshot { login: login.into(), display_name: display, ..Default::default() }));
            }
            let game = &stream["game"];
            return Ok(Some(Snapshot {
                login: login.into(),
                display_name: display,
                live: true,
                category_id: text(game, "id"),
                category: text(game, "name"),
                title: text(stream, "title"),
                viewers: stream["viewersCount"].as_i64().unwrap_or(0),
            }));
        }
        Err("Гостевой доступ Twitch недоступен.".into())
    }
}
