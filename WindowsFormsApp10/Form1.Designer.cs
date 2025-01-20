namespace TwitchCategoryTracker
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtStreamerName;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.TextBox txtClientId;
        private System.Windows.Forms.TextBox txtClientSecret;
        private System.Windows.Forms.Button btnSaveCredentials;
        private System.Windows.Forms.Button btnEditCredentials;
        private System.Windows.Forms.Button btnStartTracking;
        private System.Windows.Forms.Button btnStopTracking;
        private System.Windows.Forms.ListView lstHistory;
        private System.Windows.Forms.TextBox txtInterval;
        private System.Windows.Forms.Button btnApplyInterval;
        private System.Windows.Forms.Label lblStreamerName;
        private System.Windows.Forms.Label lblClientId;
        private System.Windows.Forms.Label lblClientSecret;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.Button btnAddStreamer;
        private System.Windows.Forms.Button btnRemoveStreamer;
        private System.Windows.Forms.ListBox lstTrackedStreamers;
        private System.Windows.Forms.Label lblTrackedStreamers;
        private System.Windows.Forms.Button btnClearHistory;
        private System.Windows.Forms.CheckBox chkFilterUnchangedCategories;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnCheckAllCategories;
        private System.Windows.Forms.Label lblCountdown;
        private System.Windows.Forms.Button btnRemoveAllStreamers;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtStreamerName = new System.Windows.Forms.TextBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.txtClientId = new System.Windows.Forms.TextBox();
            this.txtClientSecret = new System.Windows.Forms.TextBox();
            this.btnSaveCredentials = new System.Windows.Forms.Button();
            this.btnEditCredentials = new System.Windows.Forms.Button();
            this.btnStartTracking = new System.Windows.Forms.Button();
            this.btnStopTracking = new System.Windows.Forms.Button();
            this.lstHistory = new System.Windows.Forms.ListView();
            this.txtInterval = new System.Windows.Forms.TextBox();
            this.btnApplyInterval = new System.Windows.Forms.Button();
            this.lblStreamerName = new System.Windows.Forms.Label();
            this.lblClientId = new System.Windows.Forms.Label();
            this.lblClientSecret = new System.Windows.Forms.Label();
            this.lblInterval = new System.Windows.Forms.Label();
            this.btnAddStreamer = new System.Windows.Forms.Button();
            this.btnRemoveStreamer = new System.Windows.Forms.Button();
            this.lstTrackedStreamers = new System.Windows.Forms.ListBox();
            this.lblTrackedStreamers = new System.Windows.Forms.Label();
            this.btnClearHistory = new System.Windows.Forms.Button();
            this.chkFilterUnchangedCategories = new System.Windows.Forms.CheckBox();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnCheckAllCategories = new System.Windows.Forms.Button();
            this.lblCountdown = new System.Windows.Forms.Label();
            this.btnRemoveAllStreamers = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtStreamerName
            // 
            this.txtStreamerName.BackColor = System.Drawing.Color.Silver;
            this.txtStreamerName.Location = new System.Drawing.Point(10, 147);
            this.txtStreamerName.Name = "txtStreamerName";
            this.txtStreamerName.Size = new System.Drawing.Size(173, 20);
            this.txtStreamerName.TabIndex = 0;
            this.txtStreamerName.TextChanged += new System.EventHandler(this.txtStreamerName_TextChanged);
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(903, 266);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(0, 13);
            this.lblCategory.TabIndex = 2;
            // 
            // txtClientId
            // 
            this.txtClientId.BackColor = System.Drawing.Color.Silver;
            this.txtClientId.Location = new System.Drawing.Point(13, 29);
            this.txtClientId.Name = "txtClientId";
            this.txtClientId.PasswordChar = '*';
            this.txtClientId.Size = new System.Drawing.Size(170, 20);
            this.txtClientId.TabIndex = 3;
            // 
            // txtClientSecret
            // 
            this.txtClientSecret.BackColor = System.Drawing.Color.Silver;
            this.txtClientSecret.Location = new System.Drawing.Point(13, 73);
            this.txtClientSecret.Name = "txtClientSecret";
            this.txtClientSecret.PasswordChar = '*';
            this.txtClientSecret.Size = new System.Drawing.Size(170, 20);
            this.txtClientSecret.TabIndex = 4;
            // 
            // btnSaveCredentials
            // 
            this.btnSaveCredentials.Location = new System.Drawing.Point(189, 29);
            this.btnSaveCredentials.Name = "btnSaveCredentials";
            this.btnSaveCredentials.Size = new System.Drawing.Size(75, 23);
            this.btnSaveCredentials.TabIndex = 5;
            this.btnSaveCredentials.Text = "Save";
            this.btnSaveCredentials.UseVisualStyleBackColor = true;
            this.btnSaveCredentials.Click += new System.EventHandler(this.btnSaveCredentials_Click);
            // 
            // btnEditCredentials
            // 
            this.btnEditCredentials.Enabled = false;
            this.btnEditCredentials.Location = new System.Drawing.Point(189, 52);
            this.btnEditCredentials.Name = "btnEditCredentials";
            this.btnEditCredentials.Size = new System.Drawing.Size(75, 23);
            this.btnEditCredentials.TabIndex = 6;
            this.btnEditCredentials.Text = "Edit";
            this.btnEditCredentials.UseVisualStyleBackColor = true;
            this.btnEditCredentials.Click += new System.EventHandler(this.btnEditCredentials_Click);
            // 
            // btnStartTracking
            // 
            this.btnStartTracking.Location = new System.Drawing.Point(270, 295);
            this.btnStartTracking.Name = "btnStartTracking";
            this.btnStartTracking.Size = new System.Drawing.Size(75, 23);
            this.btnStartTracking.TabIndex = 7;
            this.btnStartTracking.Text = "Start Tracking";
            this.btnStartTracking.UseVisualStyleBackColor = true;
            this.btnStartTracking.Click += new System.EventHandler(this.btnStartTracking_Click);
            // 
            // btnStopTracking
            // 
            this.btnStopTracking.Enabled = false;
            this.btnStopTracking.Location = new System.Drawing.Point(360, 295);
            this.btnStopTracking.Name = "btnStopTracking";
            this.btnStopTracking.Size = new System.Drawing.Size(75, 23);
            this.btnStopTracking.TabIndex = 8;
            this.btnStopTracking.Text = "Stop Tracking";
            this.btnStopTracking.UseVisualStyleBackColor = true;
            this.btnStopTracking.Click += new System.EventHandler(this.btnStopTracking_Click);
            // 
            // lstHistory
            // 
            this.lstHistory.FullRowSelect = true;
            this.lstHistory.GridLines = true;
            this.lstHistory.HideSelection = false;
            this.lstHistory.Location = new System.Drawing.Point(270, 83);
            this.lstHistory.Name = "lstHistory";
            this.lstHistory.Size = new System.Drawing.Size(300, 206);
            this.lstHistory.TabIndex = 9;
            this.lstHistory.UseCompatibleStateImageBehavior = false;
            this.lstHistory.View = System.Windows.Forms.View.Details;
            // 
            // txtInterval
            // 
            this.txtInterval.BackColor = System.Drawing.Color.Silver;
            this.txtInterval.Location = new System.Drawing.Point(273, 15);
            this.txtInterval.Name = "txtInterval";
            this.txtInterval.Size = new System.Drawing.Size(51, 20);
            this.txtInterval.TabIndex = 10;
            this.txtInterval.Text = "30";
            // 
            // btnApplyInterval
            // 
            this.btnApplyInterval.Location = new System.Drawing.Point(418, 13);
            this.btnApplyInterval.Name = "btnApplyInterval";
            this.btnApplyInterval.Size = new System.Drawing.Size(75, 23);
            this.btnApplyInterval.TabIndex = 11;
            this.btnApplyInterval.Text = "Apply";
            this.btnApplyInterval.UseVisualStyleBackColor = true;
            this.btnApplyInterval.Click += new System.EventHandler(this.btnApplyInterval_Click);
            // 
            // lblStreamerName
            // 
            this.lblStreamerName.AutoSize = true;
            this.lblStreamerName.Location = new System.Drawing.Point(12, 131);
            this.lblStreamerName.Name = "lblStreamerName";
            this.lblStreamerName.Size = new System.Drawing.Size(80, 13);
            this.lblStreamerName.TabIndex = 12;
            this.lblStreamerName.Text = "Streamer Name";
            // 
            // lblClientId
            // 
            this.lblClientId.AutoSize = true;
            this.lblClientId.Location = new System.Drawing.Point(15, 13);
            this.lblClientId.Name = "lblClientId";
            this.lblClientId.Size = new System.Drawing.Size(47, 13);
            this.lblClientId.TabIndex = 13;
            this.lblClientId.Text = "Client ID";
            // 
            // lblClientSecret
            // 
            this.lblClientSecret.AutoSize = true;
            this.lblClientSecret.Location = new System.Drawing.Point(15, 57);
            this.lblClientSecret.Name = "lblClientSecret";
            this.lblClientSecret.Size = new System.Drawing.Size(67, 13);
            this.lblClientSecret.TabIndex = 14;
            this.lblClientSecret.Text = "Client Secret";
            // 
            // lblInterval
            // 
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(321, 18);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(91, 13);
            this.lblInterval.TabIndex = 15;
            this.lblInterval.Text = "Interval (seconds)";
            // 
            // btnAddStreamer
            // 
            this.btnAddStreamer.Location = new System.Drawing.Point(189, 147);
            this.btnAddStreamer.Name = "btnAddStreamer";
            this.btnAddStreamer.Size = new System.Drawing.Size(75, 23);
            this.btnAddStreamer.TabIndex = 16;
            this.btnAddStreamer.Text = "Add";
            this.btnAddStreamer.UseVisualStyleBackColor = true;
            this.btnAddStreamer.Click += new System.EventHandler(this.btnAddStreamer_Click);
            // 
            // btnRemoveStreamer
            // 
            this.btnRemoveStreamer.Location = new System.Drawing.Point(93, 295);
            this.btnRemoveStreamer.Name = "btnRemoveStreamer";
            this.btnRemoveStreamer.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveStreamer.TabIndex = 17;
            this.btnRemoveStreamer.Text = "Remove";
            this.btnRemoveStreamer.UseVisualStyleBackColor = true;
            this.btnRemoveStreamer.Click += new System.EventHandler(this.btnRemoveStreamer_Click);
            // 
            // lstTrackedStreamers
            // 
            this.lstTrackedStreamers.FormattingEnabled = true;
            this.lstTrackedStreamers.Location = new System.Drawing.Point(10, 194);
            this.lstTrackedStreamers.Name = "lstTrackedStreamers";
            this.lstTrackedStreamers.Size = new System.Drawing.Size(254, 95);
            this.lstTrackedStreamers.TabIndex = 18;
            // 
            // lblTrackedStreamers
            // 
            this.lblTrackedStreamers.AutoSize = true;
            this.lblTrackedStreamers.Location = new System.Drawing.Point(10, 178);
            this.lblTrackedStreamers.Name = "lblTrackedStreamers";
            this.lblTrackedStreamers.Size = new System.Drawing.Size(97, 13);
            this.lblTrackedStreamers.TabIndex = 19;
            this.lblTrackedStreamers.Text = "Tracked Streamers";
            // 
            // btnClearHistory
            // 
            this.btnClearHistory.Location = new System.Drawing.Point(452, 295);
            this.btnClearHistory.Name = "btnClearHistory";
            this.btnClearHistory.Size = new System.Drawing.Size(103, 23);
            this.btnClearHistory.TabIndex = 20;
            this.btnClearHistory.Text = "Очистить журнал";
            this.btnClearHistory.UseVisualStyleBackColor = true;
            this.btnClearHistory.Click += new System.EventHandler(this.btnClearHistory_Click);
            // 
            // chkFilterUnchangedCategories
            // 
            this.chkFilterUnchangedCategories.AutoSize = true;
            this.chkFilterUnchangedCategories.Location = new System.Drawing.Point(273, 39);
            this.chkFilterUnchangedCategories.Name = "chkFilterUnchangedCategories";
            this.chkFilterUnchangedCategories.Size = new System.Drawing.Size(300, 17);
            this.chkFilterUnchangedCategories.TabIndex = 21;
            this.chkFilterUnchangedCategories.Text = "Не добавлять записи, если категория не изменилась";
            this.chkFilterUnchangedCategories.UseVisualStyleBackColor = true;
            // 
            // btnHelp
            // 
            this.btnHelp.Location = new System.Drawing.Point(189, 78);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(75, 23);
            this.btnHelp.TabIndex = 22;
            this.btnHelp.Text = "?";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnCheckAllCategories
            // 
            this.btnCheckAllCategories.Location = new System.Drawing.Point(12, 295);
            this.btnCheckAllCategories.Name = "btnCheckAllCategories";
            this.btnCheckAllCategories.Size = new System.Drawing.Size(75, 23);
            this.btnCheckAllCategories.TabIndex = 23;
            this.btnCheckAllCategories.Text = "Check All";
            this.btnCheckAllCategories.UseVisualStyleBackColor = true;
            this.btnCheckAllCategories.Click += new System.EventHandler(this.btnCheckAllCategories_Click);
            // 
            // lblCountdown
            // 
            this.lblCountdown.AutoSize = true;
            this.lblCountdown.Location = new System.Drawing.Point(270, 62);
            this.lblCountdown.Name = "lblCountdown";
            this.lblCountdown.Size = new System.Drawing.Size(152, 13);
            this.lblCountdown.TabIndex = 24;
            this.lblCountdown.Text = "Следующая проверка через:";
            // 
            // btnRemoveAllStreamers
            // 
            this.btnRemoveAllStreamers.Location = new System.Drawing.Point(174, 295);
            this.btnRemoveAllStreamers.Name = "btnRemoveAllStreamers";
            this.btnRemoveAllStreamers.Size = new System.Drawing.Size(90, 23);
            this.btnRemoveAllStreamers.TabIndex = 25;
            this.btnRemoveAllStreamers.Text = "Удалить всех стримеров";
            this.btnRemoveAllStreamers.UseVisualStyleBackColor = true;
            this.btnRemoveAllStreamers.Click += new System.EventHandler(this.btnRemoveAllStreamers_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(578, 328);
            this.Controls.Add(this.btnRemoveAllStreamers);
            this.Controls.Add(this.lblCountdown);
            this.Controls.Add(this.btnCheckAllCategories);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.chkFilterUnchangedCategories);
            this.Controls.Add(this.btnClearHistory);
            this.Controls.Add(this.lblTrackedStreamers);
            this.Controls.Add(this.lstTrackedStreamers);
            this.Controls.Add(this.btnRemoveStreamer);
            this.Controls.Add(this.btnAddStreamer);
            this.Controls.Add(this.lblInterval);
            this.Controls.Add(this.lblClientSecret);
            this.Controls.Add(this.lblClientId);
            this.Controls.Add(this.lblStreamerName);
            this.Controls.Add(this.btnApplyInterval);
            this.Controls.Add(this.txtInterval);
            this.Controls.Add(this.lstHistory);
            this.Controls.Add(this.btnStopTracking);
            this.Controls.Add(this.btnStartTracking);
            this.Controls.Add(this.btnEditCredentials);
            this.Controls.Add(this.btnSaveCredentials);
            this.Controls.Add(this.txtClientSecret);
            this.Controls.Add(this.txtClientId);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.txtStreamerName);
            this.Name = "Form1";
            this.Text = "Twitch Category Tracker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}