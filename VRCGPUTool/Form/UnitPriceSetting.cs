using System;
using System.Drawing;
using System.Windows.Forms;
using VRCGPUTool.Util;

namespace VRCGPUTool.Form
{
    public partial class UnitPriceSetting : System.Windows.Forms.Form
    {
        public UnitPriceSetting(PowerProfile powerProfile)
        {
            InitializeComponent();
            this.powerProfile = powerProfile;
        }

        readonly PowerProfile powerProfile;

        private int sbCount = 0;

        private void UnitPriceSetting_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < PowerProfile.maxPf; i++)
            {
                if (powerProfile.pfData.ProfileCount > i)
                {
                    int hour = powerProfile.pfData.SplitTime[i];
                    int pr = powerProfile.pfData.Unit[i];

                    listBox1.Items.Add($"{hour,02}時～{pr,03}円");
                }
            }
        }

        private void ConfigButton_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count != 0)
            {
                int firstProf = Convert.ToInt32(listBox1.Items[0].ToString().Substring(0, 2).Trim());
                if(firstProf != 0)
                {
                    MessageBox.Show(
                        "0時の単価設定が存在しません",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }
            }
            else
            {
                MessageBox.Show(
                    "最低１つの単価設定が必要です",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            int plancount = listBox1.Items.Count;
            powerProfile.pfData.ProfileCount = plancount;
            for (int i = 0; i < PowerProfile.maxPf; i++)
            {
                if (i < plancount)
                {
                    int splitHour = Convert.ToInt32(listBox1.Items[i].ToString().Substring(0, 2));
                    int pr = Convert.ToInt32(listBox1.Items[i].ToString().Substring(4, 3)); //要調整
                    powerProfile.pfData.SplitTime[i] = splitHour;
                    powerProfile.pfData.Unit[i] = pr;
                }
                else
                {
                    powerProfile.pfData.SplitTime[i] = 0;
                    powerProfile.pfData.Unit[i] = 0;
                }
            }

            powerProfile.SaveProfileFile();
        }

        private SolidBrush NewSB()
        {
            sbCount++;
            switch (sbCount)
            {
                case 1: return new SolidBrush(Color.FromArgb(60, 179, 113));
                case 2: return new SolidBrush(Color.FromArgb(30, 144, 255));
                case 3: return new SolidBrush(Color.FromArgb(178, 34, 34));
                case 4: return new SolidBrush(Color.FromArgb(255, 127, 80));
                case 5: return new SolidBrush(Color.FromArgb(0, 139, 139));
                //8まで作らないとダメ（現状）
                default: return new SolidBrush(Color.FromArgb(0, 0, 0));
            }
        }

        private void Redraw()
        {
            Graphics g = CreateGraphics();

            sbCount = 0;

            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                int dt1 = Convert.ToInt32(listBox1.Items[i].ToString().Substring(0, 2).Trim());

                if (i + 1 == listBox1.Items.Count)
                {
                    int minOfDay = dt1 * 60;
                    TimeSpan duration = new TimeSpan(0, 24 - dt1, 0, 0);
                    int startAngle = (630 + (int)(360.0 * minOfDay / 1440.0)) % 360;
                    int sweepAngle = (int)(360.0 * duration.TotalMinutes / 1440.0);
                    g.FillPie(NewSB(), 50, 50, 350, 350, startAngle, sweepAngle);
                }
                else
                {
                    int dt2 = Convert.ToInt32(listBox1.Items[i + 1].ToString().Substring(0, 2).Trim());
                    int minOfDay = dt1 * 60;
                    TimeSpan duration = new TimeSpan(0, dt2 - dt1, 0, 0);
                    int startAngle = (630 + (int)(360.0 * minOfDay / 1440.0)) % 360;
                    int sweepAngle = (int)(360.0 * duration.TotalMinutes / 1440.0);
                    g.FillPie(NewSB(), 50, 50, 350, 350, startAngle, sweepAngle);
                }
            }
        }

        private void Redraw_Form(object sender, PaintEventArgs e)
        {
            Redraw();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ProfileAddButton_Click(object sender, EventArgs e)
        {
            if (PowerProfile.maxPf > listBox1.Items.Count)
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    int hour = Convert.ToInt32(listBox1.Items[i].ToString().Substring(0, 2).Trim());
                    if (hour == Convert.ToInt32(HourSplitInput.Value))
                    {
                        MessageBox.Show(
                            "同じ時刻で複数登録できません\n" +
                            "単価を変更したい場合は削除してから追加してください"
                            , "エラー",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }
                }
                listBox1.Items.Add($"{HourSplitInput.Value,02}時～{UnitPriceInput.Value,03}円");
                Redraw();
            }
            else
            {
                MessageBox.Show("これ以上追加できません", "エラー");
            }
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listBox1.SelectedIndex != -1)
                {
                    var res = MessageBox.Show(
                        "選択中の単価設定を削除してよろしいですか",
                        "確認",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );
                    if (res == DialogResult.Yes)
                    {
                        listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                        Redraw();
                    }
                }
                else
                {
                    MessageBox.Show(
                        "削除対象が選択されていません",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }
    }
}