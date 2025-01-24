namespace TwitchCategoryTracker
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtStreamerName;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.Button btnStartTracking;
        private System.Windows.Forms.Button btnStopTracking;
        private System.Windows.Forms.ListView lstHistory;
        private System.Windows.Forms.Label lblStreamerName;
        private System.Windows.Forms.Button btnAddStreamer;
        private System.Windows.Forms.Button btnRemoveStreamer;
        private System.Windows.Forms.ListBox lstTrackedStreamers;
        private System.Windows.Forms.Label lblTrackedStreamers;
        private System.Windows.Forms.Button btnClearHistory;
        private System.Windows.Forms.Button btnCheckAllCategories;
        private System.Windows.Forms.Label lblCountdown;
        private System.Windows.Forms.Button btnRemoveAllStreamers;
        private System.Windows.Forms.Button btnSettings;

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
            this.btnStartTracking = new System.Windows.Forms.Button();
            this.btnStopTracking = new System.Windows.Forms.Button();
            this.lstHistory = new System.Windows.Forms.ListView();
            this.lblStreamerName = new System.Windows.Forms.Label();
            this.btnAddStreamer = new System.Windows.Forms.Button();
            this.btnRemoveStreamer = new System.Windows.Forms.Button();
            this.lstTrackedStreamers = new System.Windows.Forms.ListBox();
            this.lblTrackedStreamers = new System.Windows.Forms.Label();
            this.btnClearHistory = new System.Windows.Forms.Button();
            this.btnCheckAllCategories = new System.Windows.Forms.Button();
            this.lblCountdown = new System.Windows.Forms.Label();
            this.btnRemoveAllStreamers = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtStreamerName
            // 
            this.txtStreamerName.BackColor = System.Drawing.Color.Silver;
            this.txtStreamerName.Location = new System.Drawing.Point(98, 16);
            this.txtStreamerName.Name = "txtStreamerName";
            this.txtStreamerName.Size = new System.Drawing.Size(224, 20);
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
            // btnStartTracking
            // 
            this.btnStartTracking.Location = new System.Drawing.Point(15, 418);
            this.btnStartTracking.Name = "btnStartTracking";
            this.btnStartTracking.Size = new System.Drawing.Size(103, 23);
            this.btnStartTracking.TabIndex = 7;
            this.btnStartTracking.Text = "Start Tracking";
            this.btnStartTracking.UseVisualStyleBackColor = true;
            this.btnStartTracking.Click += new System.EventHandler(this.btnStartTracking_Click);
            // 
            // btnStopTracking
            // 
            this.btnStopTracking.Enabled = false;
            this.btnStopTracking.Location = new System.Drawing.Point(118, 418);
            this.btnStopTracking.Name = "btnStopTracking";
            this.btnStopTracking.Size = new System.Drawing.Size(103, 23);
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
            this.lstHistory.Location = new System.Drawing.Point(15, 206);
            this.lstHistory.Name = "lstHistory";
            this.lstHistory.Size = new System.Drawing.Size(309, 206);
            this.lstHistory.TabIndex = 9;
            this.lstHistory.UseCompatibleStateImageBehavior = false;
            this.lstHistory.View = System.Windows.Forms.View.Details;
            this.lstHistory.SelectedIndexChanged += new System.EventHandler(this.lstHistory_SelectedIndexChanged);
            // 
            // lblStreamerName
            // 
            this.lblStreamerName.AutoSize = true;
            this.lblStreamerName.Location = new System.Drawing.Point(12, 19);
            this.lblStreamerName.Name = "lblStreamerName";
            this.lblStreamerName.Size = new System.Drawing.Size(80, 13);
            this.lblStreamerName.TabIndex = 12;
            this.lblStreamerName.Text = "Streamer Name";
            // 
            // btnAddStreamer
            // 
            this.btnAddStreamer.Location = new System.Drawing.Point(118, 42);
            this.btnAddStreamer.Name = "btnAddStreamer";
            this.btnAddStreamer.Size = new System.Drawing.Size(103, 23);
            this.btnAddStreamer.TabIndex = 16;
            this.btnAddStreamer.Text = "Add";
            this.btnAddStreamer.UseVisualStyleBackColor = true;
            this.btnAddStreamer.Click += new System.EventHandler(this.btnAddStreamer_Click);
            // 
            // btnRemoveStreamer
            // 
            this.btnRemoveStreamer.Location = new System.Drawing.Point(118, 168);
            this.btnRemoveStreamer.Name = "btnRemoveStreamer";
            this.btnRemoveStreamer.Size = new System.Drawing.Size(103, 23);
            this.btnRemoveStreamer.TabIndex = 17;
            this.btnRemoveStreamer.Text = "Remove";
            this.btnRemoveStreamer.UseVisualStyleBackColor = true;
            this.btnRemoveStreamer.Click += new System.EventHandler(this.btnRemoveStreamer_Click);
            // 
            // lstTrackedStreamers
            // 
            this.lstTrackedStreamers.FormattingEnabled = true;
            this.lstTrackedStreamers.Location = new System.Drawing.Point(15, 71);
            this.lstTrackedStreamers.Name = "lstTrackedStreamers";
            this.lstTrackedStreamers.Size = new System.Drawing.Size(309, 95);
            this.lstTrackedStreamers.TabIndex = 18;
            // 
            // lblTrackedStreamers
            // 
            this.lblTrackedStreamers.AutoSize = true;
            this.lblTrackedStreamers.BackColor = System.Drawing.SystemColors.Window;
            this.lblTrackedStreamers.ForeColor = System.Drawing.Color.DimGray;
            this.lblTrackedStreamers.Location = new System.Drawing.Point(224, 152);
            this.lblTrackedStreamers.Name = "lblTrackedStreamers";
            this.lblTrackedStreamers.Size = new System.Drawing.Size(97, 13);
            this.lblTrackedStreamers.TabIndex = 19;
            this.lblTrackedStreamers.Text = "Tracked Streamers";
            this.lblTrackedStreamers.Click += new System.EventHandler(this.lblTrackedStreamers_Click);
            // 
            // btnClearHistory
            // 
            this.btnClearHistory.Location = new System.Drawing.Point(221, 418);
            this.btnClearHistory.Name = "btnClearHistory";
            this.btnClearHistory.Size = new System.Drawing.Size(103, 23);
            this.btnClearHistory.TabIndex = 20;
            this.btnClearHistory.Text = "Clear History";
            this.btnClearHistory.UseVisualStyleBackColor = true;
            this.btnClearHistory.Click += new System.EventHandler(this.btnClearHistory_Click);
            // 
            // btnCheckAllCategories
            // 
            this.btnCheckAllCategories.Location = new System.Drawing.Point(15, 168);
            this.btnCheckAllCategories.Name = "btnCheckAllCategories";
            this.btnCheckAllCategories.Size = new System.Drawing.Size(103, 23);
            this.btnCheckAllCategories.TabIndex = 23;
            this.btnCheckAllCategories.Text = "Check All";
            this.btnCheckAllCategories.UseVisualStyleBackColor = true;
            this.btnCheckAllCategories.Click += new System.EventHandler(this.btnCheckAllCategories_Click);
            // 
            // lblCountdown
            // 
            this.lblCountdown.AutoSize = true;
            this.lblCountdown.Location = new System.Drawing.Point(18, 444);
            this.lblCountdown.Name = "lblCountdown";
            this.lblCountdown.Size = new System.Drawing.Size(152, 13);
            this.lblCountdown.TabIndex = 24;
            this.lblCountdown.Text = "Next check in:";
            // 
            // btnRemoveAllStreamers
            // 
            this.btnRemoveAllStreamers.Location = new System.Drawing.Point(221, 168);
            this.btnRemoveAllStreamers.Name = "btnRemoveAllStreamers";
            this.btnRemoveAllStreamers.Size = new System.Drawing.Size(103, 23);
            this.btnRemoveAllStreamers.TabIndex = 25;
            this.btnRemoveAllStreamers.Text = "Remove All Streamers";
            this.btnRemoveAllStreamers.UseVisualStyleBackColor = true;
            this.btnRemoveAllStreamers.Click += new System.EventHandler(this.btnRemoveAllStreamers_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(221, 42);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(103, 23);
            this.btnSettings.TabIndex = 26;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(335, 463);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnRemoveAllStreamers);
            this.Controls.Add(this.lblCountdown);
            this.Controls.Add(this.btnCheckAllCategories);
            this.Controls.Add(this.btnClearHistory);
            this.Controls.Add(this.lblTrackedStreamers);
            this.Controls.Add(this.lstTrackedStreamers);
            this.Controls.Add(this.btnRemoveStreamer);
            this.Controls.Add(this.btnAddStreamer);
            this.Controls.Add(this.lblStreamerName);
            this.Controls.Add(this.lstHistory);
            this.Controls.Add(this.btnStopTracking);
            this.Controls.Add(this.btnStartTracking);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.txtStreamerName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Twitch Category Tracker";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}