using System;
using System.Windows.Forms;

namespace TwitchCategoryTracker
{
    public partial class SettingsForm : Form
    {
        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }

        public SettingsForm(string clientId, string clientSecret)
        {
            InitializeComponent();
            txtClientId.Text = clientId;
            txtClientSecret.Text = clientSecret;
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

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}