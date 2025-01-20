namespace TwitchCategoryTracker
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtClientId;
        private System.Windows.Forms.TextBox txtClientSecret;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblClientId;
        private System.Windows.Forms.Label lblClientSecret;

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
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblClientId = new System.Windows.Forms.Label();
            this.lblClientSecret = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtClientId
            // 
            this.txtClientId.Location = new System.Drawing.Point(12, 25);
            this.txtClientId.Name = "txtClientId";
            this.txtClientId.Size = new System.Drawing.Size(200, 20);
            this.txtClientId.TabIndex = 0;
            // 
            // txtClientSecret
            // 
            this.txtClientSecret.Location = new System.Drawing.Point(12, 65);
            this.txtClientSecret.Name = "txtClientSecret";
            this.txtClientSecret.Size = new System.Drawing.Size(200, 20);
            this.txtClientSecret.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(12, 100);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(137, 100);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblClientId
            // 
            this.lblClientId.AutoSize = true;
            this.lblClientId.Location = new System.Drawing.Point(12, 9);
            this.lblClientId.Name = "lblClientId";
            this.lblClientId.Size = new System.Drawing.Size(47, 13);
            this.lblClientId.TabIndex = 4;
            this.lblClientId.Text = "Client ID";
            // 
            // lblClientSecret
            // 
            this.lblClientSecret.AutoSize = true;
            this.lblClientSecret.Location = new System.Drawing.Point(12, 49);
            this.lblClientSecret.Name = "lblClientSecret";
            this.lblClientSecret.Size = new System.Drawing.Size(67, 13);
            this.lblClientSecret.TabIndex = 5;
            this.lblClientSecret.Text = "Client Secret";
            // 
            // SettingsForm
            // 
            this.ClientSize = new System.Drawing.Size(224, 135);
            this.Controls.Add(this.lblClientSecret);
            this.Controls.Add(this.lblClientId);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtClientSecret);
            this.Controls.Add(this.txtClientId);
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}