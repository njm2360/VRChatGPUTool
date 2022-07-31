namespace VRCGPUTool.Form
{
    partial class SettingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DataProvideAllow = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.UsageLogDelete = new System.Windows.Forms.Button();
            this.PriceSettingRecreate = new System.Windows.Forms.Button();
            this.ConfigFileRecreate = new System.Windows.Forms.Button();
            this.RegisterStartup = new System.Windows.Forms.Button();
            this.DeleteStartup = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.DeleteStartup);
            this.groupBox1.Controls.Add(this.RegisterStartup);
            this.groupBox1.Controls.Add(this.DataProvideAllow);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(208, 121);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "一般";
            // 
            // DataProvideAllow
            // 
            this.DataProvideAllow.AutoSize = true;
            this.DataProvideAllow.Location = new System.Drawing.Point(28, 25);
            this.DataProvideAllow.Name = "DataProvideAllow";
            this.DataProvideAllow.Size = new System.Drawing.Size(102, 17);
            this.DataProvideAllow.TabIndex = 0;
            this.DataProvideAllow.Text = "使用データ提供";
            this.DataProvideAllow.UseVisualStyleBackColor = true;
            this.DataProvideAllow.CheckedChanged += new System.EventHandler(this.DataProvideAllow_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.UsageLogDelete);
            this.groupBox2.Controls.Add(this.PriceSettingRecreate);
            this.groupBox2.Controls.Add(this.ConfigFileRecreate);
            this.groupBox2.Location = new System.Drawing.Point(12, 139);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(208, 129);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "データ";
            // 
            // UsageLogDelete
            // 
            this.UsageLogDelete.Location = new System.Drawing.Point(28, 88);
            this.UsageLogDelete.Name = "UsageLogDelete";
            this.UsageLogDelete.Size = new System.Drawing.Size(144, 23);
            this.UsageLogDelete.TabIndex = 3;
            this.UsageLogDelete.Text = "電力使用履歴削除";
            this.UsageLogDelete.UseVisualStyleBackColor = true;
            this.UsageLogDelete.Click += new System.EventHandler(this.UsageLogDelete_Click);
            // 
            // PriceSettingRecreate
            // 
            this.PriceSettingRecreate.Location = new System.Drawing.Point(28, 59);
            this.PriceSettingRecreate.Name = "PriceSettingRecreate";
            this.PriceSettingRecreate.Size = new System.Drawing.Size(144, 23);
            this.PriceSettingRecreate.TabIndex = 2;
            this.PriceSettingRecreate.Text = "電気代設定削除";
            this.PriceSettingRecreate.UseVisualStyleBackColor = true;
            this.PriceSettingRecreate.Click += new System.EventHandler(this.PriceSettingRecreate_Click);
            // 
            // ConfigFileRecreate
            // 
            this.ConfigFileRecreate.Location = new System.Drawing.Point(28, 30);
            this.ConfigFileRecreate.Name = "ConfigFileRecreate";
            this.ConfigFileRecreate.Size = new System.Drawing.Size(144, 23);
            this.ConfigFileRecreate.TabIndex = 0;
            this.ConfigFileRecreate.Text = "設定ファイル削除";
            this.ConfigFileRecreate.UseVisualStyleBackColor = true;
            this.ConfigFileRecreate.Click += new System.EventHandler(this.ConfigFileRecreate_Click);
            // 
            // RegisterStartup
            // 
            this.RegisterStartup.Location = new System.Drawing.Point(28, 51);
            this.RegisterStartup.Name = "RegisterStartup";
            this.RegisterStartup.Size = new System.Drawing.Size(144, 23);
            this.RegisterStartup.TabIndex = 1;
            this.RegisterStartup.Text = "スタートアップ登録";
            this.RegisterStartup.UseVisualStyleBackColor = true;
            this.RegisterStartup.Click += new System.EventHandler(this.RegisterStartup_Click);
            // 
            // DeleteStartup
            // 
            this.DeleteStartup.Location = new System.Drawing.Point(28, 80);
            this.DeleteStartup.Name = "DeleteStartup";
            this.DeleteStartup.Size = new System.Drawing.Size(144, 23);
            this.DeleteStartup.TabIndex = 2;
            this.DeleteStartup.Text = "スタートアップ解除";
            this.DeleteStartup.UseVisualStyleBackColor = true;
            this.DeleteStartup.Click += new System.EventHandler(this.DeleteStartup_Click);
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(237, 279);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingForm";
            this.Text = "設定";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox DataProvideAllow;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button UsageLogDelete;
        private System.Windows.Forms.Button PriceSettingRecreate;
        private System.Windows.Forms.Button ConfigFileRecreate;
        private System.Windows.Forms.Button RegisterStartup;
        private System.Windows.Forms.Button DeleteStartup;
    }
}