﻿using System;
using System.IO;
using System.Windows.Forms;
using VRCGPUTool.Util;

namespace VRCGPUTool.Form
{
    public partial class SettingForm : System.Windows.Forms.Form
    {
        MainForm fm;
        StartupTask startupTask;

        public SettingForm(MainForm　fm)
        {
            InitializeComponent();
            this.fm = fm;
            DataProvideAllow.Checked = fm.allowDataProvide;
            startupTask = new StartupTask();
        }

        private void ConfigFileRecreate_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(
                "設定ファイルを削除してよろしいですか\n" +
                "削除すると保存している情報が失われます\n" +
                "※削除するとアプリが終了します",
                "確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if(res == DialogResult.Yes)
            {
                File.Delete("config.json");
                Environment.Exit(0);
            }
        }

        private void PriceSettingRecreate_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(
                "電気代設定ファイルを削除してよろしいですか\n" +
                "削除すると保存している情報が失われます\n" +
                "※削除するとアプリが終了します",
                "確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (res == DialogResult.Yes)
            {
                File.Delete("profile.json");
                Environment.Exit(0);
            }
        }

        private void UsageLogDelete_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(
                "電力使用履歴ファイルを削除してよろしいですか\n" +
                "削除すると保存している情報が失われます\n" +
                "※削除するとアプリが終了します",
                "確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (res == DialogResult.Yes)
            {
                DirectoryInfo di = new DirectoryInfo("powerlog");
                di.Delete(true);
                Directory.CreateDirectory("powerlog");
                Environment.Exit(0);
            }
        }

        private void DataProvideAllow_CheckedChanged(object sender, EventArgs e)
        {
            if(DataProvideAllow.Checked == true)
            {
                fm.allowDataProvide = true;
            }
            else
            {
                fm.allowDataProvide= false;
            }
        }

        private void RegisterStartup_Click(object sender, EventArgs e)
        {
            startupTask.registerTask();
        }

        private void DeleteStartup_Click(object sender, EventArgs e)
        {
            startupTask.removeTask();
        }
    }
}
