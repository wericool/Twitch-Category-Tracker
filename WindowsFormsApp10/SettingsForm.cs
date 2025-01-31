using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Forms;

namespace TwitchCategoryTracker
{
    public partial class SettingsForm : Form
    {
        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }
        public int CheckInterval { get; private set; }
        public bool FilterUnchangedCategories { get; private set; }
        public bool SaveLogToFile { get; private set; }
        public bool FilterOfflineStreamers { get; private set; }
        public string CurrentLanguage { get; private set; }
        public int NotificationMode { get; private set; }

        public SettingsForm(string currentClientId, string currentClientSecret, int currentInterval, bool currentFilterUnchangedCategories, bool currentSaveLogToFile, bool currentFilterOfflineStreamers, string currentLanguage, int currentNotificationMode)
        {
            InitializeComponent();

            // Жестко задаем текст кнопок выбора языка
            btnEnglish.Text = "English";
            btnRussian.Text = "Русский";

            txtClientId.Text = currentClientId;
            txtClientSecret.Text = currentClientSecret;
            txtInterval.Text = currentInterval.ToString();
            chkFilterUnchangedCategories.Checked = currentFilterUnchangedCategories;
            chkSaveLogToFile.Checked = currentSaveLogToFile;
            chkFilterOfflineStreamers.Checked = currentFilterOfflineStreamers;
            CurrentLanguage = currentLanguage;
            NotificationMode = currentNotificationMode;
            UpdateLanguage();
            UpdateNotificationMode();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ClientId = txtClientId.Text.Trim();
            ClientSecret = txtClientSecret.Text.Trim();

            if (!int.TryParse(txtInterval.Text, out int interval) || interval < 30 || interval > 3600)
            {
                MessageBox.Show(CurrentLanguage == "EN"
                    ? "Interval must be a number between 30 and 3600 seconds."
                    : "Интервал должен быть числом от 30 до 3600 секунд.");
                return;
            }

            CheckInterval = interval;
            FilterUnchangedCategories = chkFilterUnchangedCategories.Checked;
            SaveLogToFile = chkSaveLogToFile.Checked;
            FilterOfflineStreamers = chkFilterOfflineStreamers.Checked;

            if (rbNotificationsOff.Checked)
            {
                NotificationMode = 0;
            }
            else if (rbNotificationsNoSound.Checked)
            {
                NotificationMode = 1;
            }
            else if (rbNotificationsWithSound.Checked)
            {
                NotificationMode = 2;
            }
            else if (rbNotificationsSoundOnly.Checked)
            {
                NotificationMode = 3;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtClientId.Text) || string.IsNullOrEmpty(txtClientSecret.Text))
            {
                MessageBox.Show(CurrentLanguage == "EN"
                    ? "Please enter both Client ID and Client Secret."
                    : "Пожалуйста, введите Client ID и Client Secret.");
                return;
            }

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://id.twitch.tv/oauth2/token");
                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", txtClientId.Text),
                    new KeyValuePair<string, string>("client_secret", txtClientSecret.Text),
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                try
                {
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    MessageBox.Show(CurrentLanguage == "EN"
                        ? "Client ID and Client Secret are valid."
                        : "Client ID и Client Secret действительны.", "Test Successful");
                }
                catch (HttpRequestException)
                {
                    MessageBox.Show(CurrentLanguage == "EN"
                        ? "Invalid Client ID or Client Secret."
                        : "Неверный Client ID или Client Secret.", "Test Failed");
                }
            }
        }

        private void btnEnglish_Click(object sender, EventArgs e)
        {
            CurrentLanguage = "EN";
            UpdateLanguage();
        }

        private void btnRussian_Click(object sender, EventArgs e)
        {
            CurrentLanguage = "RU";
            UpdateLanguage();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://dev.twitch.tv/docs/authentication/register-app");
        }

        private void UpdateLanguage()
        {
            lblClientId.Text = CurrentLanguage == "EN" ? "Client ID" : "Client ID";
            lblClientSecret.Text = CurrentLanguage == "EN" ? "Client Secret" : "Client Secret";
            lblInterval.Text = CurrentLanguage == "EN" ? "Interval (seconds)" : "Интервал (секунды)";
            chkFilterUnchangedCategories.Text = CurrentLanguage == "EN"
                ? "Do not add entries if category is unchanged"
                : "Не добавлять записи, если категория не изменилась";
            chkSaveLogToFile.Text = CurrentLanguage == "EN"
                ? "Save log to file"
                : "Сохранять журнал";
            chkFilterOfflineStreamers.Text = CurrentLanguage == "EN"
                ? "Do not add offline streamers to the log"
                : "Не добавлять оффлайн стримеров в журнал";
            btnSave.Text = CurrentLanguage == "EN" ? "Save" : "Сохранить";
            btnCancel.Text = CurrentLanguage == "EN" ? "Cancel" : "Отмена";
            btnTest.Text = CurrentLanguage == "EN" ? "Test" : "Тест";
            btnHelp.Text = CurrentLanguage == "EN" ? "Help" : "Помощь";
            rbNotificationsOff.Text = CurrentLanguage == "EN" ? "Notifications: Off" : "Уведомления: Выкл.";
            rbNotificationsNoSound.Text = CurrentLanguage == "EN" ? "Notifications: On (No Sound)" : "Уведомления: Вкл. (Без звука)";
            rbNotificationsWithSound.Text = CurrentLanguage == "EN" ? "Notifications: On (With Sound)" : "Уведомления: Вкл. (Со звуком)";
            rbNotificationsSoundOnly.Text = CurrentLanguage == "EN" ? "Notifications: Sound Only" : "Уведомления: Только звук";
            lblAbout.Text = CurrentLanguage == "EN"
                ? "Twitch Category Tracker\nDeveloped by ericool and DeepSeek\n2025 v1.0"
                : "Twitch Category Tracker\nРазработано ericool и DeepSeek\n2025 v1.0";
        }

        private void UpdateNotificationMode()
        {
            switch (NotificationMode)
            {
                case 0:
                    rbNotificationsOff.Checked = true;
                    break;
                case 1:
                    rbNotificationsNoSound.Checked = true;
                    break;
                case 2:
                    rbNotificationsWithSound.Checked = true;
                    break;
                case 3:
                    rbNotificationsSoundOnly.Checked = true;
                    break;
            }
        }

        private void lblClientId_Click(object sender, EventArgs e)
        {
            // Пустой обработчик, если не используется
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Открываем ссылку в браузере
            System.Diagnostics.Process.Start("https://taplink.cc/ericool");
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {

        }

        private void rbNotificationsOff_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}