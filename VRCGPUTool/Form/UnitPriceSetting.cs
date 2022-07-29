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

        const int margin = 30;

        DateTimePicker[] dtPicker = new DateTimePicker[PowerProfile.maxPf];
        TextBox[] textBox = new TextBox[PowerProfile.maxPf];
        Label[] label1 = new Label[PowerProfile.maxPf];
        Label[] label2 = new Label[PowerProfile.maxPf];

        private int sbCount = 0;

        private void UnitPriceSetting_Load(object sender, EventArgs e)
        {
            SettingMenuDraw();
        }

        private void ConfigButton_Click(object sender, EventArgs e)
        {
            int res = InputValueCheck();
            if (res != -1)
            {
                SettingRefresh(res);
            }
            powerProfile.SaveProfileFile();
            Redraw(res);
        }

        private int InputValueCheck()
        {
            int plancount = PowerProfile.maxPf;
            for (int i = 0; i < PowerProfile.maxPf; i++)
            {
                if (textBox[i].Text == "")
                {
                    plancount = i;
                    break;
                }
                else
                {
                    int res;
                    if (!(int.TryParse(textBox[i].Text, out res)))
                    {
                        MessageBox.Show("整数を入力してください");
                        return -1;
                    }
                    if (i >= 1)
                    {
                        if (dtPicker[i].Value.Hour <= dtPicker[i - 1].Value.Hour)
                        {
                            MessageBox.Show("入力順番が間違っています");
                            return -1;
                        }
                    }
                }
            }
            return plancount;
        }

        private void SettingRefresh(int plancount)
        {
            powerProfile.pfData.ProfileCount = plancount;
            for (int i = 0; i < PowerProfile.maxPf; i++)
            {
                if (i < plancount)
                {
                    powerProfile.pfData.SplitTime[i] = dtPicker[i].Value.Hour;
                    powerProfile.pfData.Unit[i] = Convert.ToInt32(textBox[i].Text);
                }
                else
                {
                    powerProfile.pfData.SplitTime[i] = 0;
                    powerProfile.pfData.Unit[i] = 0;
                }
            }
        }

        private void SettingMenuDraw()
        {
            GroupBox groupBox1 = new GroupBox();
            groupBox1.Location = new Point(481, 12);
            groupBox1.Name = "SettingGroupBox";
            groupBox1.Size = new Size(156, 270);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "電気代設定(時間別)";
            groupBox1.SuspendLayout();

            for (int i = 0; i < PowerProfile.maxPf; i++)
            {
                dtPicker[i] = new DateTimePicker();
                dtPicker[i].CustomFormat = "H";
                dtPicker[i].Value = new DateTime(2020, 1, 1, 0, 0, 0, 0);
                dtPicker[i].Format = DateTimePickerFormat.Custom;
                dtPicker[i].Location = new Point(10, 25 + i * margin);
                dtPicker[i].Name = "Time";
                dtPicker[i].ShowUpDown = true;
                dtPicker[i].Size = new Size(40, 20);
                if (powerProfile.pfData.ProfileCount > i)
                {
                    dtPicker[i].Value = new DateTime(2020, 1, 1, powerProfile.pfData.SplitTime[i], 0, 0);
                }
                dtPicker[i].ValueChanged += new EventHandler(UnitPriceSetting_ValueChanged);
                groupBox1.Controls.Add(dtPicker[i]);

                textBox[i] = new TextBox();
                textBox[i].Location = new Point(85, 25 + i * margin);
                textBox[i].Name = "Pr";
                textBox[i].Size = new Size(30, 20);
                if (powerProfile.pfData.ProfileCount > i)
                {
                    textBox[i].Text = powerProfile.pfData.Unit[i].ToString();
                }
                groupBox1.Controls.Add(textBox[i]);

                label1[i] = new Label();
                label1[i].Location = new Point(120, 27 + i * margin);
                label1[i].Size = new Size(30, 20);
                label1[i].Text = "円";
                groupBox1.Controls.Add(label1[i]);

                label2[i] = new Label();
                label2[i].Location = new Point(50, 27 + i * margin);
                label2[i].Size = new Size(40, 20);
                label2[i].Text = "時～";
                groupBox1.Controls.Add(label2[i]);
            }

            dtPicker[0].Enabled = false;

            groupBox1.ResumeLayout(false);
            Controls.Add(groupBox1);
        }

        private void UnitPriceSetting_ValueChanged(object sender, EventArgs e)
        {
            int res = InputValueCheck();
            if (res != -1)
            {
                SettingRefresh(res);
            }
            else
            {
                return;
            }
            powerProfile.SaveProfileFile();
            Redraw(res);
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

        private void Redraw(int count)
        {
            Graphics g = CreateGraphics();
            Pen p = new Pen(Color.Black);

            sbCount = 0;

            for (int i = 0; i < count; i++)
            {
                if (i + 1 == count)
                {
                    string res = string.Format("{0}-End", dtPicker[i].Value);
                    Console.WriteLine(res);
                    int minOfDay = dtPicker[i].Value.Hour * 60 + dtPicker[i].Value.Minute;
                    TimeSpan duration = new DateTime(2020, 1, 2, 0, 0, 0) - dtPicker[i].Value;
                    int startAngle = (630 + (int)(360.0 * minOfDay / 1440.0)) % 360;
                    int sweepAngle = (int)(360.0 * duration.TotalMinutes / 1440.0);
                    res = string.Format("{0}~{1}deg", startAngle, sweepAngle);
                    Console.WriteLine(res);
                    g.FillPie(NewSB(), 50, 50, 350, 350, startAngle, sweepAngle);
                }
                else
                {
                    string res = string.Format("{0}-{1}", dtPicker[i].Value, dtPicker[i + 1].Value);
                    Console.WriteLine(res);
                    int minOfDay = dtPicker[i].Value.Hour * 60 + dtPicker[i].Value.Minute;
                    TimeSpan duration = dtPicker[i + 1].Value - dtPicker[i].Value;
                    int startAngle = (630 + (int)(360.0 * minOfDay / 1440.0)) % 360;
                    int sweepAngle = (int)(360.0 * duration.TotalMinutes / 1440.0);
                    res = string.Format("{0}~{1}deg", startAngle, sweepAngle);
                    Console.WriteLine(res);
                    g.FillPie(NewSB(), 50, 50, 350, 350, startAngle, sweepAngle);
                }
            }
        }

        private void Redraw_Form(object sender, PaintEventArgs e)
        {
            Redraw(powerProfile.pfData.ProfileCount);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}