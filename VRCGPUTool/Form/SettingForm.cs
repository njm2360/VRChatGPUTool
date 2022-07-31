using System;
using System.IO;
using System.Windows.Forms;

namespace VRCGPUTool.Form
{
    public partial class SettingForm : System.Windows.Forms.Form
    {
        MainForm fm;

        public SettingForm(MainForm　fm)
        {
            InitializeComponent();
            this.fm = fm;
            DataProvideAllow.Checked = fm.allowDataProvide;
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
            string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), @"VRCGPUTool.lnk");
            string targetPath = Application.ExecutablePath;

            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.WshShortcut shortcut = shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = Application.StartupPath;
            shortcut.WindowStyle = 1;
            shortcut.IconLocation = Application.ExecutablePath + ",0";

            shortcut.Save();

            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
        }

        private void DeleteStartup_Click(object sender, EventArgs e)
        {
            File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), @"VRCGPUTool.lnk"));
        }
    }
}
