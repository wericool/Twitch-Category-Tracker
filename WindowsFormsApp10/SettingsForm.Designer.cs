namespace TwitchCategoryTracker
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtClientId;
        private System.Windows.Forms.TextBox txtClientSecret;
        private System.Windows.Forms.Label lblClientId;
        private System.Windows.Forms.Label lblClientSecret;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtInterval;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.CheckBox chkFilterUnchangedCategories;
        private System.Windows.Forms.CheckBox chkSaveLogToFile;
        private System.Windows.Forms.CheckBox chkFilterOfflineStreamers;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnEnglish;
        private System.Windows.Forms.Button btnRussian;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.RadioButton rbNotificationsOff;
        private System.Windows.Forms.RadioButton rbNotificationsNoSound;
        private System.Windows.Forms.RadioButton rbNotificationsWithSound;
        private System.Windows.Forms.RadioButton rbNotificationsSoundOnly;
        private System.Windows.Forms.Label lblAbout; // Новый Label
        private System.Windows.Forms.LinkLabel linkLabel; // Новый LinkLabel

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
            this.txtClientId = new System.Windows.Forms.TextBox();
            this.txtClientSecret = new System.Windows.Forms.TextBox();
            this.lblClientId = new System.Windows.Forms.Label();
            this.lblClientSecret = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtInterval = new System.Windows.Forms.TextBox();
            this.lblInterval = new System.Windows.Forms.Label();
            this.chkFilterUnchangedCategories = new System.Windows.Forms.CheckBox();
            this.chkSaveLogToFile = new System.Windows.Forms.CheckBox();
            this.chkFilterOfflineStreamers = new System.Windows.Forms.CheckBox();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnEnglish = new System.Windows.Forms.Button();
            this.btnRussian = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.rbNotificationsOff = new System.Windows.Forms.RadioButton();
            this.rbNotificationsNoSound = new System.Windows.Forms.RadioButton();
            this.rbNotificationsWithSound = new System.Windows.Forms.RadioButton();
            this.rbNotificationsSoundOnly = new System.Windows.Forms.RadioButton();
            this.lblAbout = new System.Windows.Forms.Label();
            this.linkLabel = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // txtClientId
            // 
            this.txtClientId.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.txtClientId.Location = new System.Drawing.Point(3, 5);
            this.txtClientId.Name = "txtClientId";
            this.txtClientId.Size = new System.Drawing.Size(183, 20);
            this.txtClientId.TabIndex = 0;
            // 
            // txtClientSecret
            // 
            this.txtClientSecret.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.txtClientSecret.Location = new System.Drawing.Point(3, 33);
            this.txtClientSecret.Name = "txtClientSecret";
            this.txtClientSecret.Size = new System.Drawing.Size(183, 20);
            this.txtClientSecret.TabIndex = 1;
            // 
            // lblClientId
            // 
            this.lblClientId.AutoSize = true;
            this.lblClientId.Location = new System.Drawing.Point(192, 12);
            this.lblClientId.Name = "lblClientId";
            this.lblClientId.Size = new System.Drawing.Size(47, 13);
            this.lblClientId.TabIndex = 2;
            this.lblClientId.Text = "Client ID";
            this.lblClientId.Click += new System.EventHandler(this.lblClientId_Click);
            // 
            // lblClientSecret
            // 
            this.lblClientSecret.AutoSize = true;
            this.lblClientSecret.Location = new System.Drawing.Point(192, 40);
            this.lblClientSecret.Name = "lblClientSecret";
            this.lblClientSecret.Size = new System.Drawing.Size(67, 13);
            this.lblClientSecret.TabIndex = 3;
            this.lblClientSecret.Text = "Client Secret";
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.SystemColors.Control;
            this.btnSave.Location = new System.Drawing.Point(205, 245);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(286, 245);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // txtInterval
            // 
            this.txtInterval.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.txtInterval.Location = new System.Drawing.Point(3, 61);
            this.txtInterval.Name = "txtInterval";
            this.txtInterval.Size = new System.Drawing.Size(183, 20);
            this.txtInterval.TabIndex = 6;
            // 
            // lblInterval
            // 
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(192, 68);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(91, 13);
            this.lblInterval.TabIndex = 7;
            this.lblInterval.Text = "Interval (seconds)";
            // 
            // chkFilterUnchangedCategories
            // 
            this.chkFilterUnchangedCategories.AutoSize = true;
            this.chkFilterUnchangedCategories.Location = new System.Drawing.Point(3, 116);
            this.chkFilterUnchangedCategories.Name = "chkFilterUnchangedCategories";
            this.chkFilterUnchangedCategories.Size = new System.Drawing.Size(300, 17);
            this.chkFilterUnchangedCategories.TabIndex = 8;
            this.chkFilterUnchangedCategories.Text = "Не добавлять записи, если категория не изменилась";
            this.chkFilterUnchangedCategories.UseVisualStyleBackColor = true;
            // 
            // chkSaveLogToFile
            // 
            this.chkSaveLogToFile.AutoSize = true;
            this.chkSaveLogToFile.Location = new System.Drawing.Point(3, 96);
            this.chkSaveLogToFile.Name = "chkSaveLogToFile";
            this.chkSaveLogToFile.Size = new System.Drawing.Size(119, 17);
            this.chkSaveLogToFile.TabIndex = 9;
            this.chkSaveLogToFile.Text = "Сохранять журнал";
            this.chkSaveLogToFile.UseVisualStyleBackColor = true;
            // 
            // chkFilterOfflineStreamers
            // 
            this.chkFilterOfflineStreamers.AutoSize = true;
            this.chkFilterOfflineStreamers.Location = new System.Drawing.Point(3, 136);
            this.chkFilterOfflineStreamers.Name = "chkFilterOfflineStreamers";
            this.chkFilterOfflineStreamers.Size = new System.Drawing.Size(252, 17);
            this.chkFilterOfflineStreamers.TabIndex = 10;
            this.chkFilterOfflineStreamers.Text = "Не добавлять оффлайн стримеров в журнал";
            this.chkFilterOfflineStreamers.UseVisualStyleBackColor = true;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(286, 38);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 11;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnEnglish
            // 
            this.btnEnglish.Location = new System.Drawing.Point(205, 211);
            this.btnEnglish.Name = "btnEnglish";
            this.btnEnglish.Size = new System.Drawing.Size(75, 23);
            this.btnEnglish.TabIndex = 12;
            this.btnEnglish.Text = "English";
            this.btnEnglish.UseVisualStyleBackColor = true;
            this.btnEnglish.Click += new System.EventHandler(this.btnEnglish_Click);
            // 
            // btnRussian
            // 
            this.btnRussian.Location = new System.Drawing.Point(286, 211);
            this.btnRussian.Name = "btnRussian";
            this.btnRussian.Size = new System.Drawing.Size(75, 23);
            this.btnRussian.TabIndex = 13;
            this.btnRussian.Text = "Русский";
            this.btnRussian.UseVisualStyleBackColor = true;
            this.btnRussian.Click += new System.EventHandler(this.btnRussian_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Location = new System.Drawing.Point(286, 7);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(75, 23);
            this.btnHelp.TabIndex = 14;
            this.btnHelp.Text = "Help";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // rbNotificationsOff
            // 
            this.rbNotificationsOff.AutoSize = true;
            this.rbNotificationsOff.Location = new System.Drawing.Point(3, 184);
            this.rbNotificationsOff.Name = "rbNotificationsOff";
            this.rbNotificationsOff.Size = new System.Drawing.Size(103, 17);
            this.rbNotificationsOff.TabIndex = 15;
            this.rbNotificationsOff.TabStop = true;
            this.rbNotificationsOff.Text = "Notifications: Off";
            this.rbNotificationsOff.UseVisualStyleBackColor = true;
            this.rbNotificationsOff.CheckedChanged += new System.EventHandler(this.rbNotificationsOff_CheckedChanged);
            // 
            // rbNotificationsNoSound
            // 
            this.rbNotificationsNoSound.AutoSize = true;
            this.rbNotificationsNoSound.Location = new System.Drawing.Point(174, 184);
            this.rbNotificationsNoSound.Name = "rbNotificationsNoSound";
            this.rbNotificationsNoSound.Size = new System.Drawing.Size(160, 17);
            this.rbNotificationsNoSound.TabIndex = 16;
            this.rbNotificationsNoSound.TabStop = true;
            this.rbNotificationsNoSound.Text = "Notifications: On (No Sound)";
            this.rbNotificationsNoSound.UseVisualStyleBackColor = true;
            // 
            // rbNotificationsWithSound
            // 
            this.rbNotificationsWithSound.AutoSize = true;
            this.rbNotificationsWithSound.Location = new System.Drawing.Point(174, 161);
            this.rbNotificationsWithSound.Name = "rbNotificationsWithSound";
            this.rbNotificationsWithSound.Size = new System.Drawing.Size(168, 17);
            this.rbNotificationsWithSound.TabIndex = 17;
            this.rbNotificationsWithSound.TabStop = true;
            this.rbNotificationsWithSound.Text = "Notifications: On (With Sound)";
            this.rbNotificationsWithSound.UseVisualStyleBackColor = true;
            // 
            // rbNotificationsSoundOnly
            // 
            this.rbNotificationsSoundOnly.AutoSize = true;
            this.rbNotificationsSoundOnly.Location = new System.Drawing.Point(3, 161);
            this.rbNotificationsSoundOnly.Name = "rbNotificationsSoundOnly";
            this.rbNotificationsSoundOnly.Size = new System.Drawing.Size(144, 17);
            this.rbNotificationsSoundOnly.TabIndex = 18;
            this.rbNotificationsSoundOnly.TabStop = true;
            this.rbNotificationsSoundOnly.Text = "Notifications: Sound Only";
            this.rbNotificationsSoundOnly.UseVisualStyleBackColor = true;
            // 
            // lblAbout
            // 
            this.lblAbout.AutoSize = true;
            this.lblAbout.Location = new System.Drawing.Point(0, 211);
            this.lblAbout.Name = "lblAbout";
            this.lblAbout.Size = new System.Drawing.Size(182, 39);
            this.lblAbout.TabIndex = 19;
            this.lblAbout.Text = "Twitch Category Tracker\nDeveloped by ericool and DeepSeek\n2025 v1.0";
            // 
            // linkLabel
            // 
            this.linkLabel.AutoSize = true;
            this.linkLabel.Location = new System.Drawing.Point(0, 250);
            this.linkLabel.Name = "linkLabel";
            this.linkLabel.Size = new System.Drawing.Size(125, 13);
            this.linkLabel.TabIndex = 20;
            this.linkLabel.TabStop = true;
            this.linkLabel.Text = "https://taplink.cc/ericool";
            this.linkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            // 
            // SettingsForm
            // 
            this.ClientSize = new System.Drawing.Size(363, 271);
            this.Controls.Add(this.linkLabel);
            this.Controls.Add(this.lblAbout);
            this.Controls.Add(this.rbNotificationsSoundOnly);
            this.Controls.Add(this.rbNotificationsWithSound);
            this.Controls.Add(this.rbNotificationsNoSound);
            this.Controls.Add(this.rbNotificationsOff);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnRussian);
            this.Controls.Add(this.btnEnglish);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.chkFilterOfflineStreamers);
            this.Controls.Add(this.chkSaveLogToFile);
            this.Controls.Add(this.chkFilterUnchangedCategories);
            this.Controls.Add(this.lblInterval);
            this.Controls.Add(this.txtInterval);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblClientSecret);
            this.Controls.Add(this.lblClientId);
            this.Controls.Add(this.txtClientSecret);
            this.Controls.Add(this.txtClientId);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}