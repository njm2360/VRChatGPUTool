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
                "設定ファイルを再生成してよろしいですか\n" +
                "再生成すると保存している情報が失われます",
                "確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if(res == DialogResult.Yes)
            {
                File.Delete("config.json");
                Application.Restart();
            }
        }

        private void PriceSettingRecreate_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(
                "電気代設定ファイルを再生成してよろしいですか\n" +
                "再生成すると保存している情報が失われます",
                "確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (res == DialogResult.Yes)
            {
                File.Delete("profile.json");
                Application.Restart();
            }
        }

        private void UsageLogDelete_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(
                "電力使用履歴ファイルを削除してよろしいですか\n" +
                "削除すると保存している情報が失われます",
                "確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (res == DialogResult.Yes)
            {
                Directory.Delete("powerlog");
                Application.Restart();
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
    }
}
