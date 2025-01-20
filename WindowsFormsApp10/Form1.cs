using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private System.Windows.Forms.Timer countdownTimer;
        private Dictionary<string, string> streamerCategories = new Dictionary<string, string>();
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private bool notificationsEnabled = true;
        private int remainingTime; // Оставшееся время до следующей проверки
        private bool FilterUnchangedCategories { get; set; } = false; // Значение по умолчанию

        public Form1()
        {
            InitializeComponent();
            trackingTimer = new System.Windows.Forms.Timer();
            trackingTimer.Tick += TrackingTimer_Tick;
            countdownTimer = new System.Windows.Forms.Timer();
            countdownTimer.Interval = 1000; // 1 секунда
            countdownTimer.Tick += CountdownTimer_Tick;
            InitializeToolTips();
            InitializeTrayIcon();
            _ = LoadSettings();
            UpdateButtonsState(false);

            // Инициализация колонок в lstHistory
            lstHistory.Columns.Add("Time", 60);
            lstHistory.Columns.Add("Streamer", 100);
            lstHistory.Columns.Add("Category", 120);
        }

        private void InitializeToolTips()
        {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(txtStreamerName, "Введите имя стримера, например, 'shroud'.");
            toolTip.SetToolTip(btnStartTracking, "Начать отслеживание категории стримеров.");
            toolTip.SetToolTip(btnStopTracking, "Остановить отслеживание.");
            toolTip.SetToolTip(btnAddStreamer, "Добавить стримера в список отслеживания.");
            toolTip.SetToolTip(btnRemoveStreamer, "Удалить выбранного стримера из списка.");
            toolTip.SetToolTip(btnClearHistory, "Очистить журнал.");
            toolTip.SetToolTip(btnCheckAllCategories, "Проверить категории всех стримеров.");
            toolTip.SetToolTip(btnRemoveAllStreamers, "Удалить всех стримеров из списка.");
        }

        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Text = "Twitch Category Tracker";
            trayIcon.Visible = false;

            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Развернуть", null, OnRestore);
            trayMenu.Items.Add("Тестовое уведомление", null, OnTestNotification);

            trayMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem notificationsMenuItem = new ToolStripMenuItem("Уведомления");
            ToolStripMenuItem enableNotificationsItem = new ToolStripMenuItem("Включить");
            ToolStripMenuItem disableNotificationsItem = new ToolStripMenuItem("Отключить");

            enableNotificationsItem.Click += (s, e) => { notificationsEnabled = true; UpdateTrayMenu(); SaveSettings(); };
            disableNotificationsItem.Click += (s, e) => { notificationsEnabled = false; UpdateTrayMenu(); SaveSettings(); };

            notificationsMenuItem.DropDownItems.Add(enableNotificationsItem);
            notificationsMenuItem.DropDownItems.Add(disableNotificationsItem);

            trayMenu.Items.Add(notificationsMenuItem);
            UpdateTrayMenu();

            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Выход", null, OnExit);

            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.DoubleClick += OnRestore;
        }

        private void UpdateTrayMenu()
        {
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
            trayIcon.Visible = false;
        }

        private void OnTestNotification(object sender, EventArgs e)
        {
            if (notificationsEnabled)
            {
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
            trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void OnResize(EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                trayIcon.Visible = true;
            }
            base.OnResize(e);
        }

        private async void TrackingTimer_Tick(object sender, EventArgs e)
        {
            await CheckAllStreamersCategoriesAsync();
            remainingTime = checkInterval; // Сбрасываем таймер
            UpdateCountdownLabel();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (remainingTime > 0)
            {
                remainingTime--;
                UpdateCountdownLabel();
            }
        }

        private void UpdateCountdownLabel()
        {
            lblCountdown.Text = $"Следующая проверка через: {remainingTime} сек.";
        }

        private async Task CheckAllStreamersCategoriesAsync()
        {
            var categories = await GetStreamersCategoriesAsync(trackedStreamers);
            if (categories != null)
            {
                foreach (var streamer in trackedStreamers)
                {
                    await CheckCategoryChangeAsync(streamer, categories[streamer]);
                }
            }
            UpdateTrayIconToolTip();
            UpdateStreamersList();
        }

        private async Task CheckCategoryChangeAsync(string streamerName, string newCategory)
        {
            if (streamerCategories.ContainsKey(streamerName))
            {
                string oldCategory = streamerCategories[streamerName];
                if (oldCategory != newCategory)
                {
                    // Категория изменилась
                    if (notificationsEnabled)
                    {
                        ShowToastNotification("Категория изменена", $"{streamerName} сменил категорию с '{oldCategory}' на '{newCategory}'.");
                    }

                    // Добавляем запись в журнал
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    AddToHistory(time, streamerName, newCategory);

                    streamerCategories[streamerName] = newCategory;
                }
                else if (!FilterUnchangedCategories) // Используем значение из настроек
                {
                    // Категория не изменилась, но фильтр отключен
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    AddToHistory(time, streamerName, newCategory);
                }
            }
            else
            {
                // Первая запись о стримере
                streamerCategories[streamerName] = newCategory;

                // Добавляем запись в журнал
                string time = DateTime.Now.ToString("HH:mm:ss");
                AddToHistory(time, streamerName, newCategory);
            }

            // Добавляем запись в журнал, если стример стал оффлайн
            if (newCategory == "Offline" && streamerCategories[streamerName] != "Offline")
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                AddToHistory(time, streamerName, "Offline");
                streamerCategories[streamerName] = "Offline";
            }
        }

        private void AddToHistory(string time, string streamerName, string category)
        {
            // Проверяем, что ListViewItem создается и добавляется в lstHistory
            var item = new ListViewItem(new[] { time, streamerName, category });
            lstHistory.Items.Add(item);

            // Прокручиваем журнал вниз, чтобы показать последнюю запись
            lstHistory.EnsureVisible(lstHistory.Items.Count - 1);
        }

        private void ShowToastNotification(string title, string message)
        {
            if (!notificationsEnabled)
            {
                return;
            }

            string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notif.wav");

            if (!File.Exists(soundPath))
            {
                LogError($"Sound file not found: {soundPath}");
                return;
            }

            var toast = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);

            if (notificationsEnabled)
            {
                toast.AddAudio(new Uri(soundPath), silent: false);
            }
            else
            {
                toast.AddAudio(new Uri("ms-winsoundevent:Notification.Default"), silent: true);
            }

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

            string finalText = tooltipText.ToString();
            if (finalText.Length > 63)
            {
                finalText = finalText.Substring(0, 60) + "...";
            }

            trayIcon.Text = finalText;
        }

        private string ExtractStreamerName(string input)
        {
            if (input.StartsWith("https://www.twitch.tv/"))
            {
                return input.Substring("https://www.twitch.tv/".Length).Split('/')[0];
            }
            return input;
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

        private async Task<Dictionary<string, string>> GetStreamersCategoriesAsync(IEnumerable<string> streamerNames)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                await GetAccessTokenAsync();
            }

            var categories = new Dictionary<string, string>();
            var streamerList = streamerNames.ToList();
            int batchSize = 100;

            for (int i = 0; i < streamerList.Count; i += batchSize)
            {
                var batch = streamerList.Skip(i).Take(batchSize);
                var batchQuery = string.Join("&user_login=", batch);
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/helix/streams?user_login={batchQuery}");
                request.Headers.Add("Client-ID", ClientId);
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                try
                {
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var responseData = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(responseData);

                    var streams = json["data"] as JArray;
                    if (streams != null)
                    {
                        foreach (var stream in streams)
                        {
                            var streamerName = stream["user_login"]?.ToString();
                            var gameName = stream["game_name"]?.ToString();
                            if (streamerName != null)
                            {
                                categories[streamerName] = gameName ?? "No category";
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    LogError($"HTTP Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    LogError($"Error: {ex.Message}");
                }
            }

            foreach (var streamer in streamerNames)
            {
                if (!categories.ContainsKey(streamer))
                {
                    categories[streamer] = "Offline";
                }
            }

            return categories;
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            // Открываем окно настроек
            using (var settingsForm = new SettingsForm(ClientId, ClientSecret, checkInterval, FilterUnchangedCategories))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Сохраняем новые значения
                    ClientId = settingsForm.ClientId;
                    ClientSecret = settingsForm.ClientSecret;
                    checkInterval = settingsForm.CheckInterval;
                    FilterUnchangedCategories = settingsForm.FilterUnchangedCategories; // Обновляем значение

                    // Сохраняем настройки в файл
                    SaveSettings();
                    MessageBox.Show("Settings saved successfully.");
                }
            }
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

            remainingTime = checkInterval; // Устанавливаем начальное значение таймера
            UpdateCountdownLabel();
            countdownTimer.Start();

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
                countdownTimer.Stop();
                lblCountdown.Text = "Отслеживание остановлено.";
                MessageBox.Show("Tracking stopped.");
            }
            else
            {
                MessageBox.Show("Tracking is not active.");
            }
        }

        private void btnClearHistory_Click(object sender, EventArgs e)
        {
            lstHistory.Items.Clear();
        }

        private void btnRemoveAllStreamers_Click(object sender, EventArgs e)
        {
            trackedStreamers.Clear();
            streamerCategories.Clear();
            lstTrackedStreamers.Items.Clear();
            SaveSettings();
        }

        private void LogError(string errorMessage)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            AddToHistory(time, "Error", errorMessage);
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
                    writer.WriteLine(notificationsEnabled);
                    writer.WriteLine(FilterUnchangedCategories); // Сохраняем значение
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
                            FilterUnchangedCategories = filterUnchangedCategories; // Загружаем значение
                        }
                    }

                    UpdateTrayMenu();

                    await CheckLoadedStreamersCategoriesAsync();
                }
                else
                {
                    UpdateButtonsState(false); // Обновляем состояние кнопок без проверки на сохранение настроек
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to load settings: {ex.Message}");
                UpdateButtonsState(false); // Обновляем состояние кнопок без проверки на сохранение настроек
            }
        }

        private async Task CheckLoadedStreamersCategoriesAsync()
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                await GetAccessTokenAsync();
            }

            var categories = await GetStreamersCategoriesAsync(trackedStreamers);
            if (categories != null)
            {
                foreach (var streamer in trackedStreamers)
                {
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    AddToHistory(time, streamer, categories[streamer]);

                    streamerCategories[streamer] = categories[streamer];
                }
            }

            UpdateTrayIconToolTip();
            UpdateStreamersList();
        }

        private void btnAddStreamer_Click(object sender, EventArgs e)
        {
            string streamerName = ExtractStreamerName(txtStreamerName.Text.Trim());
            if (string.IsNullOrEmpty(streamerName))
            {
                MessageBox.Show("Please enter a streamer name or URL.");
                return;
            }

            if (!trackedStreamers.Contains(streamerName))
            {
                trackedStreamers.Add(streamerName);
                lstTrackedStreamers.Items.Add(streamerName);
                SaveSettings();
                UpdateStreamersList();
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
                // Получаем выбранный элемент
                string selectedItem = lstTrackedStreamers.SelectedItem.ToString();

                // Убираем статус (например, "[LIVE] " или "[OFFLINE] ")
                string streamerName = selectedItem
                    .Replace("[LIVE]", "")
                    .Replace("[OFFLINE]", "")
                    .Trim();

                // Удаляем стримера из списка отслеживаемых
                if (trackedStreamers.Contains(streamerName))
                {
                    trackedStreamers.Remove(streamerName);

                    // Удаляем стримера из словаря категорий
                    if (streamerCategories.ContainsKey(streamerName))
                    {
                        streamerCategories.Remove(streamerName);
                    }

                    // Обновляем отображаемый список
                    UpdateStreamersList();

                    // Сохраняем настройки
                    SaveSettings();
                }
            }
        }

        private async void btnCheckAllCategories_Click(object sender, EventArgs e)
        {
            var categories = await GetStreamersCategoriesAsync(trackedStreamers);
            if (categories != null)
            {
                foreach (var streamer in trackedStreamers)
                {
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    AddToHistory(time, streamer, categories[streamer]);
                }
            }
        }

        private void UpdateButtonsState(bool isTrackingActive = false)
        {
            btnStartTracking.Enabled = !isTrackingActive; // Кнопка "Start Tracking" активна, если отслеживание не запущено
            btnStopTracking.Enabled = isTrackingActive;   // Кнопка "Stop Tracking" активна, если отслеживание запущено
            btnAddStreamer.Enabled = !isTrackingActive;   // Кнопка "Add Streamer" активна, если отслеживание не запущено
            btnRemoveStreamer.Enabled = !isTrackingActive; // Кнопка "Remove Streamer" активна, если отслеживание не запущено
            btnCheckAllCategories.Enabled = !isTrackingActive; // Кнопка "Check All" активна, если отслеживание не запущено
            btnClearHistory.Enabled = true; // Кнопка "Clear History" всегда активна
            btnRemoveAllStreamers.Enabled = !isTrackingActive; // Кнопка "Remove All Streamers" активна, если отслеживание не запущено
        }

        private void UpdateStreamersList()
        {
            lstTrackedStreamers.Items.Clear();
            foreach (var streamer in trackedStreamers)
            {
                if (streamerCategories.ContainsKey(streamer))
                {
                    string status = streamerCategories[streamer] != "Offline" ? "[LIVE]" : "[OFFLINE]";
                    lstTrackedStreamers.Items.Add($"{status} {streamer}");
                }
                else
                {
                    lstTrackedStreamers.Items.Add($"[OFFLINE] {streamer}");
                }
            }
        }

        private void txtStreamerName_TextChanged(object sender, EventArgs e)
        {
        }

        private void lblTrackedStreamers_Click(object sender, EventArgs e)
        {

        }

        private void lstHistory_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}