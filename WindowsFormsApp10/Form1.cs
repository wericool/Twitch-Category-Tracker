using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;

namespace TwitchCategoryTracker
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient client = new HttpClient();
        private static string accessToken;
        private List<string> trackedStreamers = new List<string>();
        private string ClientId = string.Empty;
        private string ClientSecret = string.Empty;
        private bool isTracking = false;
        private int checkInterval = 30;
        private System.Windows.Forms.Timer trackingTimer;
        private Dictionary<string, string> streamerCategories = new Dictionary<string, string>();
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private bool notificationsEnabled = true; // Флаг для отключения уведомлений

        public Form1()
        {
            InitializeComponent();
            trackingTimer = new System.Windows.Forms.Timer();
            trackingTimer.Tick += TrackingTimer_Tick;
            InitializeToolTips();
            InitializeTrayIcon();
            _ = LoadSettings(); // Асинхронная загрузка настроек
        }

        private void InitializeToolTips()
        {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(txtStreamerName, "Введите имя стримера, например, 'shroud'.");
            toolTip.SetToolTip(txtClientId, "Введите ваш Client ID от Twitch API.");
            toolTip.SetToolTip(txtClientSecret, "Введите ваш Client Secret от Twitch API.");
            toolTip.SetToolTip(txtInterval, "Введите интервал проверки в секундах (минимум 30).");
            toolTip.SetToolTip(btnCheckCategory, "Проверить текущую категорию стримера.");
            toolTip.SetToolTip(btnSaveCredentials, "Сохранить Client ID и Client Secret.");
            toolTip.SetToolTip(btnEditCredentials, "Редактировать Client ID и Client Secret.");
            toolTip.SetToolTip(btnStartTracking, "Начать отслеживание категории стримеров.");
            toolTip.SetToolTip(btnStopTracking, "Остановить отслеживание.");
            toolTip.SetToolTip(btnApplyInterval, "Применить новый интервал проверки.");
            toolTip.SetToolTip(btnAddStreamer, "Добавить стримера в список отслеживания.");
            toolTip.SetToolTip(btnRemoveStreamer, "Удалить выбранного стримера из списка.");
            toolTip.SetToolTip(btnClearHistory, "Очистить журнал.");
            toolTip.SetToolTip(chkFilterUnchangedCategories, "Не добавлять записи, если категория не изменилась.");
        }

        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application; // Замените на свою иконку
            trayIcon.Text = "Twitch Category Tracker";
            trayIcon.Visible = false;

            // Создаем контекстное меню для иконки в трее
            trayMenu = new ContextMenuStrip();

            // Добавляем пункты меню
            trayMenu.Items.Add("Развернуть", null, OnRestore);
            trayMenu.Items.Add("Тестовое уведомление", null, OnTestNotification);

            // Добавляем разделитель
            trayMenu.Items.Add(new ToolStripSeparator());

            // Создаем выпадающий пункт меню "Уведомления"
            ToolStripMenuItem notificationsMenuItem = new ToolStripMenuItem("Уведомления");

            // Добавляем подпункты
            ToolStripMenuItem enableNotificationsItem = new ToolStripMenuItem("Включить");
            ToolStripMenuItem disableNotificationsItem = new ToolStripMenuItem("Отключить");

            enableNotificationsItem.Click += (s, e) => { notificationsEnabled = true; UpdateTrayMenu(); SaveSettings(); };
            disableNotificationsItem.Click += (s, e) => { notificationsEnabled = false; UpdateTrayMenu(); SaveSettings(); };

            notificationsMenuItem.DropDownItems.Add(enableNotificationsItem);
            notificationsMenuItem.DropDownItems.Add(disableNotificationsItem);

            trayMenu.Items.Add(notificationsMenuItem);

            // Обновляем видимость пунктов
            UpdateTrayMenu();

            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Выход", null, OnExit);

            trayIcon.ContextMenuStrip = trayMenu;

            // Обработка двойного клика по иконке в трее
            trayIcon.DoubleClick += OnRestore;
        }

        private void UpdateTrayMenu()
        {
            // Обновляем галочки в подпунктах меню
            foreach (ToolStripMenuItem item in ((ToolStripMenuItem)trayMenu.Items[3]).DropDownItems)
            {
                if (item.Text == "Включить")
                {
                    item.Checked = notificationsEnabled;
                }
                else if (item.Text == "Отключить")
                {
                    item.Checked = !notificationsEnabled;
                }
            }
        }

        private void OnRestore(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            trayIcon.Visible = false; // Скрываем иконку в трее
        }

        private void OnTestNotification(object sender, EventArgs e)
        {
            if (notificationsEnabled) // Проверяем, включены ли уведомления
            {
                // Запускаем тестовое уведомление через 5 секунд
                Task.Delay(5000).ContinueWith(_ =>
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        ShowToastNotification("Тестовое уведомление", "Это тестовое уведомление для проверки функциональности.");
                    }));
                });
            }
            else
            {
                MessageBox.Show("Уведомления отключены.");
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            trayIcon.Visible = false; // Скрываем иконку в трее перед выходом
            Application.Exit();
        }

        protected override void OnResize(EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                trayIcon.Visible = true; // Показываем иконку в трее при сворачивании
            }
            base.OnResize(e);
        }

        private async void TrackingTimer_Tick(object sender, EventArgs e)
        {
            foreach (var streamer in trackedStreamers)
            {
                string category = await GetStreamerCategoryAsync(streamer);
                await CheckCategoryChangeAsync(streamer, category);
            }
            UpdateTrayIconToolTip();
        }

        private async Task CheckCategoryChangeAsync(string streamerName, string newCategory)
        {
            if (streamerCategories.ContainsKey(streamerName))
            {
                string oldCategory = streamerCategories[streamerName];
                if (oldCategory != newCategory)
                {
                    // Категория изменилась
                    if (notificationsEnabled) // Проверяем, включены ли уведомления
                    {
                        ShowToastNotification("Категория изменена", $"{streamerName} сменил категорию с '{oldCategory}' на '{newCategory}'.");
                    }

                    // Добавляем запись в журнал, если категория изменилась
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    lstHistory.Items.Add($"{time} - {streamerName}: {newCategory}");

                    streamerCategories[streamerName] = newCategory;
                }
                else if (!chkFilterUnchangedCategories.Checked)
                {
                    // Категория не изменилась, но фильтр отключен — добавляем запись
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    lstHistory.Items.Add($"{time} - {streamerName}: {newCategory} (без изменений)");
                }
            }
            else
            {
                // Первая запись о стримере
                streamerCategories[streamerName] = newCategory;

                // Добавляем запись в журнал
                string time = DateTime.Now.ToString("HH:mm:ss");
                lstHistory.Items.Add($"{time} - {streamerName}: {newCategory}");
            }
        }

        private void ShowToastNotification(string title, string message)
        {
            if (!notificationsEnabled)
            {
                return; // Если уведомления отключены, ничего не делаем
            }

            // Получаем путь к звуку Notif.wav
            string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notif.wav");

            // Проверяем, существует ли файл
            if (!File.Exists(soundPath))
            {
                LogError($"Sound file not found: {soundPath}");
                return; // Если файл не найден, выходим
            }

            // Создаем уведомление
            var toast = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);

            // Добавляем звук, если уведомления включены
            if (notificationsEnabled)
            {
                toast.AddAudio(new Uri(soundPath), silent: false); // Используем звук из папки с программой
            }
            else
            {
                toast.AddAudio(new Uri("ms-winsoundevent:Notification.Default"), silent: true); // Без звука
            }

            // Показываем уведомление
            toast.Show();
        }

        private void UpdateTrayIconToolTip()
        {
            StringBuilder tooltipText = new StringBuilder("Текущие категории:\n");
            foreach (var streamer in trackedStreamers)
            {
                if (streamerCategories.ContainsKey(streamer))
                {
                    tooltipText.AppendLine($"{streamer}: {streamerCategories[streamer]}");
                }
            }

            // Ограничиваем длину текста до 63 символов
            string finalText = tooltipText.ToString();
            if (finalText.Length > 63)
            {
                finalText = finalText.Substring(0, 60) + "..."; // Обрезаем и добавляем многоточие
            }

            trayIcon.Text = finalText;
        }

        private async void btnCheckCategory_Click(object sender, EventArgs e)
        {
            string streamerName = txtStreamerName.Text.Trim();
            if (string.IsNullOrEmpty(streamerName))
            {
                MessageBox.Show("Please enter a streamer name.");
                return;
            }

            await GetAccessTokenAsync();
            string category = await GetStreamerCategoryAsync(streamerName);
            if (category != null)
            {
                lblCategory.Text = $"{streamerName}: {category}";
            }
            else
            {
                lblCategory.Text = $"{streamerName} is offline or not found.";
            }
        }

        private async Task GetAccessTokenAsync()
        {
            if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(ClientSecret))
            {
                MessageBox.Show("Please enter Client ID and Client Secret.");
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "https://id.twitch.tv/oauth2/token");
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            try
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseData);

                if (json["access_token"] != null)
                {
                    accessToken = json["access_token"].ToString();
                }
                else
                {
                    MessageBox.Show("Failed to get access token. Check your Client ID and Client Secret.");
                    accessToken = null;
                }
            }
            catch (HttpRequestException ex)
            {
                LogError($"HTTP Error: {ex.Message}");
                accessToken = null;
            }
            catch (Exception ex)
            {
                LogError($"Error: {ex.Message}");
                accessToken = null;
            }
        }

        private async Task<string> GetStreamerCategoryAsync(string streamerName)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/helix/streams?user_login={streamerName}");
            request.Headers.Add("Client-ID", ClientId);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            try
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseData);

                var streams = json["data"] as JArray;
                if (streams != null && streams.Count > 0)
                {
                    var gameName = streams[0]["game_name"]?.ToString();
                    return gameName ?? "No category";
                }

                return "Offline";
            }
            catch (HttpRequestException ex)
            {
                LogError($"HTTP Error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                LogError($"Error: {ex.Message}");
                return null;
            }
        }

        private void btnSaveCredentials_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtClientId.Text.Trim()) || string.IsNullOrEmpty(txtClientSecret.Text.Trim()))
            {
                MessageBox.Show("Please enter both Client ID and Client Secret.");
                return;
            }

            ClientId = txtClientId.Text.Trim();
            ClientSecret = txtClientSecret.Text.Trim();

            btnSaveCredentials.Enabled = false;
            btnEditCredentials.Enabled = true;

            txtClientId.Enabled = false;
            txtClientSecret.Enabled = false;

            SaveSettings();
            MessageBox.Show("Credentials saved successfully.");
        }

        private void btnEditCredentials_Click(object sender, EventArgs e)
        {
            txtClientId.Enabled = true;
            txtClientSecret.Enabled = true;

            btnSaveCredentials.Enabled = true;
            btnEditCredentials.Enabled = false;
        }

        private async void btnStartTracking_Click(object sender, EventArgs e)
        {
            if (trackedStreamers.Count == 0)
            {
                MessageBox.Show("Please add at least one streamer to track.");
                return;
            }

            if (isTracking)
            {
                MessageBox.Show("Already tracking streamers.");
                return;
            }

            isTracking = true;
            btnStartTracking.Enabled = false;
            btnStopTracking.Enabled = true;

            StartTrackingInterval();
        }

        private void StartTrackingInterval()
        {
            trackingTimer.Interval = checkInterval * 1000;
            trackingTimer.Start();
        }

        private void btnStopTracking_Click(object sender, EventArgs e)
        {
            if (isTracking)
            {
                isTracking = false;
                btnStartTracking.Enabled = true;
                btnStopTracking.Enabled = false;
                trackingTimer.Stop();
                MessageBox.Show("Tracking stopped.");
            }
            else
            {
                MessageBox.Show("Tracking is not active.");
            }
        }

        private void btnApplyInterval_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtInterval.Text, out int newInterval))
            {
                if (newInterval >= 30 && newInterval <= 3600)
                {
                    checkInterval = newInterval;
                    MessageBox.Show($"Interval set to {checkInterval} seconds.");
                    SaveSettings();
                }
                else
                {
                    MessageBox.Show("Interval must be between 30 and 3600 seconds.");
                }
            }
            else
            {
                MessageBox.Show("Invalid interval.");
            }
        }

        private void btnClearHistory_Click(object sender, EventArgs e)
        {
            lstHistory.Items.Clear(); // Очищаем журнал
        }

        private void LogError(string errorMessage)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            lstHistory.Items.Add($"{time} - Error: {errorMessage}");
        }

        private void SaveSettings()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("settings.txt"))
                {
                    writer.WriteLine(ClientId);
                    writer.WriteLine(ClientSecret);
                    writer.WriteLine(checkInterval);
                    writer.WriteLine(string.Join(",", trackedStreamers));
                    writer.WriteLine(notificationsEnabled); // Сохраняем состояние уведомлений
                    writer.WriteLine(chkFilterUnchangedCategories.Checked); // Сохраняем состояние CheckBox
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to save settings: {ex.Message}");
            }
        }

        private async Task LoadSettings()
        {
            try
            {
                if (File.Exists("settings.txt"))
                {
                    using (StreamReader reader = new StreamReader("settings.txt"))
                    {
                        ClientId = reader.ReadLine();
                        ClientSecret = reader.ReadLine();
                        if (int.TryParse(reader.ReadLine(), out int interval))
                        {
                            checkInterval = interval;
                        }
                        string streamers = reader.ReadLine();
                        if (!string.IsNullOrEmpty(streamers))
                        {
                            trackedStreamers.AddRange(streamers.Split(','));
                            lstTrackedStreamers.Items.AddRange(trackedStreamers.ToArray());
                        }
                        if (bool.TryParse(reader.ReadLine(), out bool notifications))
                        {
                            notificationsEnabled = notifications;
                        }
                        if (bool.TryParse(reader.ReadLine(), out bool filterUnchangedCategories))
                        {
                            chkFilterUnchangedCategories.Checked = filterUnchangedCategories;
                        }
                    }

                    txtClientId.Text = ClientId;
                    txtClientSecret.Text = ClientSecret;
                    txtInterval.Text = checkInterval.ToString();

                    // Обновляем меню в трее
                    UpdateTrayMenu();

                    // Проверка категорий для загруженных стримеров
                    await CheckLoadedStreamersCategoriesAsync();
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to load settings: {ex.Message}");
            }
        }

        private async Task CheckLoadedStreamersCategoriesAsync()
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                await GetAccessTokenAsync();
            }

            foreach (var streamer in trackedStreamers)
            {
                try
                {
                    string category = await GetStreamerCategoryAsync(streamer);
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    lstHistory.Items.Add($"{time} - {streamer}: {category}");

                    // Обновляем данные о категориях
                    streamerCategories[streamer] = category;
                }
                catch (Exception ex)
                {
                    LogError($"Failed to get category for {streamer}: {ex.Message}");
                }
            }

            // Обновляем текст иконки в трее
            UpdateTrayIconToolTip();
        }

        private void btnAddStreamer_Click(object sender, EventArgs e)
        {
            string streamerName = txtStreamerName.Text.Trim();
            if (string.IsNullOrEmpty(streamerName))
            {
                MessageBox.Show("Please enter a streamer name.");
                return;
            }

            if (!trackedStreamers.Contains(streamerName))
            {
                trackedStreamers.Add(streamerName);
                lstTrackedStreamers.Items.Add(streamerName);
                SaveSettings();
            }
            else
            {
                MessageBox.Show("Streamer is already in the list.");
            }
        }

        private void btnRemoveStreamer_Click(object sender, EventArgs e)
        {
            if (lstTrackedStreamers.SelectedItem != null)
            {
                string selectedStreamer = lstTrackedStreamers.SelectedItem.ToString();
                trackedStreamers.Remove(selectedStreamer);
                lstTrackedStreamers.Items.Remove(selectedStreamer);
                SaveSettings();
            }
            else
            {
                MessageBox.Show("Please select a streamer to remove.");
            }
        }
    }
}

