using System;
using System.Drawing;
using System.Windows.Forms;

namespace VRCGPUTool.Form
{
    public partial class UnitPriceSetting : System.Windows.Forms.Form
    {
        CheckBox[] checkBox = new CheckBox[5];
        DateTimePicker[] dtPicker = new DateTimePicker[5];
        TextBox[] textBox = new TextBox[5];

        private int sbCount = 0;

        public UnitPriceSetting()
        {
            InitializeComponent();
        }

        private void UnitPriceSetting_Load(object sender, EventArgs e)
        {
            SettingMenuDraw();
        }

        private void SettingMenuDraw()
        {
            const int margin = 30;

            GroupBox groupBox1 = new GroupBox();
            groupBox1.Location = new System.Drawing.Point(481, 12);
            groupBox1.Name = "SettingGroupBox";
            groupBox1.Size = new System.Drawing.Size(156, 191);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "List";
            groupBox1.SuspendLayout();

            for (int i = 0; i < checkBox.Length; i++)
            {
                checkBox[i] = new CheckBox();
                checkBox[i].AutoSize = true;
                checkBox[i].Location = new System.Drawing.Point(15, 47 + i * margin);
                checkBox[i].Name = "SplitTime";
                checkBox[i].Size = new System.Drawing.Size(15, 15);
                checkBox[i].UseVisualStyleBackColor = true;
                groupBox1.Controls.Add(checkBox[i]);

                dtPicker[i] = new DateTimePicker();
                dtPicker[i].CustomFormat = "H:mm";
                dtPicker[i].Value = new DateTime(2020, 1, 1, 0, 0, 0, 0);
                dtPicker[i].Format = System.Windows.Forms.DateTimePickerFormat.Custom;
                dtPicker[i].Location = new System.Drawing.Point(40, 45 + i * margin);
                dtPicker[i].Name = "Time";
                dtPicker[i].ShowUpDown = true;
                dtPicker[i].Size = new System.Drawing.Size(50, 20);
                groupBox1.Controls.Add(dtPicker[i]);

                textBox[i] = new TextBox();
                textBox[i].Location = new System.Drawing.Point(95, 45 + i * margin);
                textBox[i].Name = "Pr";
                textBox[i].Size = new System.Drawing.Size(40, 20);
                groupBox1.Controls.Add(textBox[i]);
            }

            checkBox[0].Checked = true;
            checkBox[0].Enabled = false;
            dtPicker[0].Enabled = false;

            groupBox1.ResumeLayout(false);
            Controls.Add(groupBox1);
        }

        private SolidBrush NewSB()
        {
            sbCount++;
            switch (sbCount)
            {
                case 1: return new SolidBrush(Color.Red);
                case 2: return new SolidBrush(Color.Blue);
                case 3: return new SolidBrush(Color.Yellow);
                case 4: return new SolidBrush(Color.Green);
                case 5: return new SolidBrush(Color.Magenta);
                default: return new SolidBrush(Color.Black);
            }
        }

        private void ConfigButton_Click(object sender, EventArgs e)
        {
            sbCount = 0;

            Graphics g = this.CreateGraphics();
            Pen p = new Pen(Color.Black);

            int plans = checkBox.Length;

            for (int i = 0; i < checkBox.Length; i++)
            {
                if (!checkBox[i].Checked)
                {
                    plans = i;
                    break;
                }
                else
                {
                    if (textBox[i].Text == "")
                    {
                        //MessageBox.Show("入力されていません");
                    }
                }
            }
            for (int j = 0; j < plans; j++)
            {
                if (j == 0)
                {
                    if (plans == 1)
                    {
                        string res = string.Format("Start-End");
                        g.FillPie(NewSB(), 30, 30, 330, 330, 0, 360);
                        Console.WriteLine(res);
                    }
                    else
                    {
                        string res = string.Format("Start-{0}", dtPicker[j + 1].Value);
                        int minOfDay = dtPicker[j + 1].Value.Hour * 60 + dtPicker[j + 1].Value.Minute;
                        int angle = (int)(360 * minOfDay / 1440.0 );
                        g.FillPie(NewSB(), 30, 30, 330, 330, 0, angle);
                        Console.WriteLine(res);
                    }
                }
                else if (j == (plans - 1))
                {
                    string res = string.Format("{0}-End", dtPicker[j].Value);
                    int minOfDay = dtPicker[j].Value.Hour * 60 + dtPicker[j].Value.Minute;
                    int angle = (int)(360 * minOfDay / 1440.0);
                    sbCount = 0;
                    g.FillPie(NewSB(), 30, 30, 330, 330, angle, 360-angle);
                    Console.WriteLine(res);
                }
                else
                {
                    string res = string.Format("{0}-{1}", dtPicker[j].Value, dtPicker[j + 1].Value);
                    int minOfDay1 = dtPicker[j].Value.Hour * 60 + dtPicker[j].Value.Minute;
                    int minOfDay2 = dtPicker[j + 1].Value.Hour * 60 + dtPicker[j + 1].Value.Minute;
                    int angle1 = (int)(360 * minOfDay1 / 1440.0);
                    int angle2 = (int)(360 * minOfDay2 / 1440.0);
                    g.FillPie(NewSB(), 30, 30, 330, 330, angle1, angle2-angle1);
                    Console.WriteLine(res);
                }
            }

        }
    }
}
