using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace VRCGPUTool.Form
{
    partial class SettingForm
    {
        private IContainer components = null;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(SettingForm));


            this.DataProvideAllow = new CheckBox();
            this.RegisterStartup = new Button();
            this.DeleteStartup = new Button();
            this.GeneralGroup = new GroupBox();
            this.ConfigFileRecreate = new Button();
            this.PriceSettingRecreate = new Button();
            this.UsageLogDelete = new Button();
            this.DataGroup = new GroupBox();
            this.GeneralGroup.SuspendLayout();
            this.DataGroup.SuspendLayout();
            this.SuspendLayout();


            this.DataProvideAllow.Location = new Point(25, 20);
            this.DataProvideAllow.Size = new Size(150, 20);
            this.DataProvideAllow.TabIndex = 0;
            this.DataProvideAllow.Text = "使用データ提供";
            this.DataProvideAllow.CheckedChanged += new EventHandler(this.DataProvideAllow_CheckedChanged);

            this.RegisterStartup.Location = new Point(25, 45);
            this.RegisterStartup.Size = new Size(150, 20);
            this.RegisterStartup.TabIndex = 1;
            this.RegisterStartup.Text = "スタートアップ登録";
            this.RegisterStartup.Click += new EventHandler(this.RegisterStartup_Click);

            this.DeleteStartup.Location = new Point(25, 70);
            this.DeleteStartup.Size = new Size(150, 20);
            this.DeleteStartup.TabIndex = 2;
            this.DeleteStartup.Text = "スタートアップ解除";
            this.DeleteStartup.Click += new EventHandler(this.DeleteStartup_Click);


            this.GeneralGroup.Controls.Add(this.DeleteStartup);
            this.GeneralGroup.Controls.Add(this.RegisterStartup);
            this.GeneralGroup.Controls.Add(this.DataProvideAllow);
            this.GeneralGroup.Location = new Point(10, 10);
            this.GeneralGroup.Size = new Size(200, 110);
            this.GeneralGroup.Text = "一般";


            this.ConfigFileRecreate.Location = new Point(25, 20);
            this.ConfigFileRecreate.Size = new Size(150, 20);
            this.ConfigFileRecreate.TabIndex = 3;
            this.ConfigFileRecreate.Text = "設定ファイル削除";
            this.ConfigFileRecreate.Click += new EventHandler(this.ConfigFileRecreate_Click);

            this.PriceSettingRecreate.Location = new Point(25, 45);
            this.PriceSettingRecreate.Size = new Size(150, 20);
            this.PriceSettingRecreate.TabIndex = 4;
            this.PriceSettingRecreate.Text = "電気代設定削除";
            this.PriceSettingRecreate.Click += new EventHandler(this.PriceSettingRecreate_Click);

            this.UsageLogDelete.Location = new Point(25, 70);
            this.UsageLogDelete.Size = new Size(150, 20);
            this.UsageLogDelete.TabIndex = 5;
            this.UsageLogDelete.Text = "電力使用履歴削除";
            this.UsageLogDelete.Click += new EventHandler(this.UsageLogDelete_Click);

            this.DataGroup.Controls.Add(this.UsageLogDelete);
            this.DataGroup.Controls.Add(this.PriceSettingRecreate);
            this.DataGroup.Controls.Add(this.ConfigFileRecreate);
            this.DataGroup.Location = new Point(10, 130);
            this.DataGroup.Size = new Size(200, 110);
            this.DataGroup.Text = "データ";

            //Setting Form

            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(220, 250);
            this.Controls.Add(this.DataGroup);
            this.Controls.Add(this.GeneralGroup);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Icon = ((Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "設定";
            this.GeneralGroup.ResumeLayout(false);
            this.GeneralGroup.PerformLayout();
            this.DataGroup.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private GroupBox GeneralGroup;
        private CheckBox DataProvideAllow;
        private GroupBox DataGroup;
        private Button UsageLogDelete;
        private Button PriceSettingRecreate;
        private Button ConfigFileRecreate;
        private Button RegisterStartup;
        private Button DeleteStartup;
    }
}