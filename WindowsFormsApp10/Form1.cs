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
        private int notificationMode = 2;
        private int remainingTime;
        private bool FilterUnchangedCategories { get; set; } = false;
        private bool SaveLogToFile { get; set; } = false;
        public string CurrentLanguage { get; set; } = "EN";

        public Form1()
        {
            InitializeComponent();
            trackingTimer = new System.Windows.Forms.Timer();
            trackingTimer.Tick += TrackingTimer_Tick;
            countdownTimer = new System.Windows.Forms.Timer();
            countdownTimer.Interval = 1000;
            countdownTimer.Tick += CountdownTimer_Tick;
            InitializeToolTips();
            InitializeTrayIcon();
            _ = LoadSettings();
            UpdateButtonsState(false);

            lstHistory.Columns.Add("Time", 60);
            lstHistory.Columns.Add("Streamer", 100);
            lstHistory.Columns.Add("Category", 120);

            UpdateLanguage();
        }

        private void InitializeToolTips()
        {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(txtStreamerName, "Enter the streamer's name, e.g., 'shroud'.");
            toolTip.SetToolTip(btnStartTracking, "Start tracking streamers' categories.");
            toolTip.SetToolTip(btnStopTracking, "Stop tracking.");
            toolTip.SetToolTip(btnAddStreamer, "Add a streamer to the tracking list.");
            toolTip.SetToolTip(btnRemoveStreamer, "Remove the selected streamer from the list.");
            toolTip.SetToolTip(btnClearHistory, "Clear the history.");
            toolTip.SetToolTip(btnCheckAllCategories, "Check all streamers' categories.");
            toolTip.SetToolTip(btnRemoveAllStreamers, "Remove all streamers from the list.");
        }

        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Text = "Twitch Category Tracker";
            trayIcon.Visible = false;

            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Test Notification", null, OnTestNotification);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Notifications: Off", null, OnToggleNotifications);
            trayMenu.Items.Add("Notifications: On (No Sound)", null, OnToggleNotifications);
            trayMenu.Items.Add("Notifications: On (With Sound)", null, OnToggleNotifications);
            trayMenu.Items.Add("Notifications: Sound Only", null, OnToggleNotifications);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Settings", null, OnSettingsFromTray);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, OnExit);

            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.DoubleClick += OnRestore;
        }

        private void OnToggleNotifications(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                if (item.Text == "Notifications: Off")
                {
                    notificationMode = 0;
                }
                else if (item.Text == "Notifications: On (No Sound)")
                {
                    notificationMode = 1;
                }
                else if (item.Text == "Notifications: On (With Sound)")
                {
                    notificationMode = 2;
                }
                else if (item.Text == "Notifications: Sound Only")
                {
                    notificationMode = 3;
                }
                UpdateTrayMenu();
                SaveSettings();
            }
        }

        private void OnTestNotification(object sender, EventArgs e)
        {
            if (notificationMode > 0)
            {
                Task.Delay(5000).ContinueWith(_ =>
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        ShowToastNotification("Test Notification", "This is a test notification to check functionality.");
                    }));
                });
            }
            else
            {
                MessageBox.Show("Notifications are disabled.");
            }
        }

        private void OnSettingsFromTray(object sender, EventArgs e)
        {
            btnSettings_Click(sender, e);
        }

        private void UpdateTrayMenu()
        {
            foreach (ToolStripItem item in trayMenu.Items)
            {
                if (item is ToolStripMenuItem menuItem && menuItem.Text.StartsWith("Notifications:"))
                {
                    menuItem.Checked = false;
                }
            }

            switch (notificationMode)
            {
                case 0:
                    ((ToolStripMenuItem)trayMenu.Items[2]).Checked = true;
                    break;
                case 1:
                    ((ToolStripMenuItem)trayMenu.Items[3]).Checked = true;
                    break;
                case 2:
                    ((ToolStripMenuItem)trayMenu.Items[4]).Checked = true;
                    break;
                case 3:
                    ((ToolStripMenuItem)trayMenu.Items[5]).Checked = true;
                    break;
            }
        }

        private void OnRestore(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            trayIcon.Visible = false;
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
            remainingTime = checkInterval;
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
            lblCountdown.Text = CurrentLanguage == "EN"
                ? $"Next check in: {remainingTime} sec."
                : $"Следующая проверка через: {remainingTime} сек.";
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
                    if (notificationMode > 0)
                    {
                        ShowToastNotification(
                            CurrentLanguage == "EN" ? "Category Changed" : "Категория изменена",
                            CurrentLanguage == "EN"
                                ? $"{streamerName} changed category from '{oldCategory}' to '{newCategory}'."
                                : $"{streamerName} сменил категорию с '{oldCategory}' на '{newCategory}'."
                        );
                    }

                    string time = DateTime.Now.ToString("HH:mm:ss");
                    AddToHistory(time, streamerName, newCategory);

                    streamerCategories[streamerName] = newCategory;
                }
                else if (!FilterUnchangedCategories)
                {
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    AddToHistory(time, streamerName, newCategory);
                }
            }
            else
            {
                streamerCategories[streamerName] = newCategory;

                string time = DateTime.Now.ToString("HH:mm:ss");
                AddToHistory(time, streamerName, newCategory);
            }

            if (newCategory == "Offline" && streamerCategories[streamerName] != "Offline")
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                AddToHistory(time, streamerName, "Offline");
                streamerCategories[streamerName] = "Offline";
            }
        }

        private void AddToHistory(string time, string streamerName, string category)
        {
            var item = new ListViewItem(new[] { time, streamerName, category });
            lstHistory.Items.Add(item);
            lstHistory.EnsureVisible(lstHistory.Items.Count - 1);

            if (SaveLogToFile)
            {
                string logEntry = $"{time} - {streamerName}: {category}";
                File.AppendAllText("history.log", logEntry + Environment.NewLine);
            }
        }

        private void ShowToastNotification(string title, string message)
        {
            if (notificationMode == 0)
            {
                return;
            }

            string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notif.wav");

            if (!File.Exists(soundPath))
            {
                LogError($"Sound file not found: {soundPath}");
                return;
            }

            if (notificationMode == 3)
            {
                try
                {
                    using (var player = new System.Media.SoundPlayer(soundPath))
                    {
                        player.Play();
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to play sound: {ex.Message}");
                }
                return;
            }

            var toast = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);

            if (notificationMode == 2)
            {
                toast.AddAudio(new Uri(soundPath), silent: false);
            }
            else if (notificationMode == 1)
            {
                toast.AddAudio(new Uri("ms-winsoundevent:Notification.Default"), silent: true);
            }

            toast.Show();
        }

        private void UpdateTrayIconToolTip()
        {
            StringBuilder tooltipText = new StringBuilder(CurrentLanguage == "EN" ? "Current categories:\n" : "Текущие категории:\n");
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
                MessageBox.Show(CurrentLanguage == "EN"
                    ? "Please enter Client ID and Client Secret."
                    : "Пожалуйста, введите Client ID и Client Secret.");
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
                    MessageBox.Show(CurrentLanguage == "EN"
                        ? "Failed to get access token. Check your Client ID and Client Secret."
                        : "Не удалось получить токен доступа. Проверьте Client ID и Client Secret.");
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
            using (var settingsForm = new SettingsForm(ClientId, ClientSecret, checkInterval, FilterUnchangedCategories, SaveLogToFile, CurrentLanguage, notificationMode))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    ClientId = settingsForm.ClientId;
                    ClientSecret = settingsForm.ClientSecret;
                    checkInterval = settingsForm.CheckInterval;
                    FilterUnchangedCategories = settingsForm.FilterUnchangedCategories;
                    SaveLogToFile = settingsForm.SaveLogToFile;
                    CurrentLanguage = settingsForm.CurrentLanguage;
                    notificationMode = settingsForm.NotificationMode;

                    SaveSettings();
                    UpdateLanguage();
                    UpdateTrayMenu();
                }
            }
        }

        private async void btnStartTracking_Click(object sender, EventArgs e)
        {
            if (trackedStreamers.Count == 0)
            {
                MessageBox.Show(CurrentLanguage == "EN"
                    ? "Please add at least one streamer to track."
                    : "Пожалуйста, добавьте хотя бы одного стримера для отслеживания.");
                return;
            }

            if (isTracking)
            {
                return;
            }

            isTracking = true;
            btnStartTracking.Enabled = false;
            btnStopTracking.Enabled = true;

            remainingTime = checkInterval;
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
                lblCountdown.Text = CurrentLanguage == "EN"
                    ? "Tracking stopped."
                    : "Отслеживание остановлено.";
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
            var iniFile = new IniFile("Settings.cfg");

            iniFile.SetValue("General", "ClientId", ClientId);
            iniFile.SetValue("General", "ClientSecret", ClientSecret);
            iniFile.SetValue("General", "CheckInterval", checkInterval.ToString());
            iniFile.SetValue("General", "FilterUnchangedCategories", FilterUnchangedCategories.ToString());
            iniFile.SetValue("General", "SaveLogToFile", SaveLogToFile.ToString());
            iniFile.SetValue("General", "CurrentLanguage", CurrentLanguage);
            iniFile.SetValue("General", "NotificationMode", notificationMode.ToString());

            int i = 1;
            foreach (var streamer in trackedStreamers)
            {
                iniFile.SetValue("TrackedStreamers", $"Streamer{i}", streamer);
                i++;
            }

            iniFile.Save();
        }

        private async Task LoadSettings()
        {
            var iniFile = new IniFile("Settings.cfg");

            ClientId = iniFile.GetValue("General", "ClientId", string.Empty);
            ClientSecret = iniFile.GetValue("General", "ClientSecret", string.Empty);
            if (int.TryParse(iniFile.GetValue("General", "CheckInterval"), out int interval))
            {
                checkInterval = interval;
            }
            FilterUnchangedCategories = bool.Parse(iniFile.GetValue("General", "FilterUnchangedCategories", "false"));
            SaveLogToFile = bool.Parse(iniFile.GetValue("General", "SaveLogToFile", "false"));
            CurrentLanguage = iniFile.GetValue("General", "CurrentLanguage", "EN");
            if (int.TryParse(iniFile.GetValue("General", "NotificationMode"), out int mode))
            {
                notificationMode = mode;
            }

            trackedStreamers.Clear();
            foreach (var key in iniFile.GetKeys("TrackedStreamers"))
            {
                trackedStreamers.Add(iniFile.GetValue("TrackedStreamers", key));
            }

            UpdateTrayMenu();
            UpdateStreamersList();
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
                MessageBox.Show(CurrentLanguage == "EN"
                    ? "Please enter a streamer name or URL."
                    : "Пожалуйста, введите имя стримера или URL.");
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
                MessageBox.Show(CurrentLanguage == "EN"
                    ? "Streamer is already in the list."
                    : "Стример уже в списке.");
            }
        }

        private void btnRemoveStreamer_Click(object sender, EventArgs e)
        {
            if (lstTrackedStreamers.SelectedItem != null)
            {
                string selectedItem = lstTrackedStreamers.SelectedItem.ToString();
                string streamerName = selectedItem
                    .Replace("[LIVE]", "")
                    .Replace("[OFFLINE]", "")
                    .Trim();

                if (trackedStreamers.Contains(streamerName))
                {
                    trackedStreamers.Remove(streamerName);

                    if (streamerCategories.ContainsKey(streamerName))
                    {
                        streamerCategories.Remove(streamerName);
                    }

                    UpdateStreamersList();
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

                    streamerCategories[streamer] = categories[streamer];
                }
            }

            UpdateStreamersList();
        }

        private void UpdateButtonsState(bool isTrackingActive = false)
        {
            btnStartTracking.Enabled = !isTrackingActive;
            btnStopTracking.Enabled = isTrackingActive;
            btnAddStreamer.Enabled = !isTrackingActive;
            btnRemoveStreamer.Enabled = !isTrackingActive;
            btnCheckAllCategories.Enabled = !isTrackingActive;
            btnClearHistory.Enabled = true;
            btnRemoveAllStreamers.Enabled = !isTrackingActive;
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

        private void UpdateLanguage()
        {
            lblStreamerName.Text = CurrentLanguage == "EN" ? "Streamer Name" : "Имя стримера";
            btnAddStreamer.Text = CurrentLanguage == "EN" ? "Add" : "Добавить";
            btnRemoveStreamer.Text = CurrentLanguage == "EN" ? "Remove" : "Удалить";
            btnStartTracking.Text = CurrentLanguage == "EN" ? "Start Tracking" : "Начать отслеживание";
            btnStopTracking.Text = CurrentLanguage == "EN" ? "Stop Tracking" : "Остановить отслеживание";
            btnCheckAllCategories.Text = CurrentLanguage == "EN" ? "Check All" : "Проверить всех";
            btnClearHistory.Text = CurrentLanguage == "EN" ? "Clear History" : "Очистить журнал";
            btnRemoveAllStreamers.Text = CurrentLanguage == "EN" ? "Remove All Streamers" : "Удалить всех стримеров";
            btnSettings.Text = CurrentLanguage == "EN" ? "Settings" : "Настройки";
            lblTrackedStreamers.Text = CurrentLanguage == "EN" ? "Tracked Streamers" : "Отслеживаемые стримеры";
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