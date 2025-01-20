using System;
using System.Windows.Forms;

namespace TwitchCategoryTracker
{
    public partial class SettingsForm : Form
    {
        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }
        public int CheckInterval { get; private set; }
        public bool FilterUnchangedCategories { get; private set; }

        public SettingsForm(string currentClientId, string currentClientSecret, int currentInterval, bool currentFilterUnchangedCategories)
        {
            InitializeComponent();
            txtClientId.Text = currentClientId;
            txtClientSecret.Text = currentClientSecret;
            txtInterval.Text = currentInterval.ToString();
            chkFilterUnchangedCategories.Checked = currentFilterUnchangedCategories;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ClientId = txtClientId.Text.Trim();
            ClientSecret = txtClientSecret.Text.Trim();

            if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(ClientSecret))
            {
                MessageBox.Show("Please enter both Client ID and Client Secret.");
                return;
            }

            if (!int.TryParse(txtInterval.Text, out int interval) || interval < 30 || interval > 3600)
            {
                MessageBox.Show("Interval must be a number between 30 and 3600 seconds.");
                return;
            }

            CheckInterval = interval;
            FilterUnchangedCategories = chkFilterUnchangedCategories.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void lblClientId_Click(object sender, EventArgs e)
        {

        }
    }
}