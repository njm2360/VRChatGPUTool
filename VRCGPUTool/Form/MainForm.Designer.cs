using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace VRCGPUTool.Form
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

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
            this.components = new Container();

            this.BeginTime = new DateTimePicker();
            this.EndTime = new DateTimePicker();
            this.BeginTimeLabel = new Label();
            this.EndTimeLabel = new Label();
            this.TimeSetGroupBox = new GroupBox();

            this.howtouse = new Button();
            this.bugreport = new Button();
            this.PowerLogShow = new Button();
            this.SettingButton = new Button();

            this.GPUCoreTemp = new Label();
            this.GPUTotalPower = new Label();
            this.GPUCorePLValue = new Label();
            this.GPUCoreClockValue = new Label();
            this.GPUMemoryClockValue = new Label();
            this.GPUStatusGroup = new GroupBox();

            this.PLimitLabel = new Label();
            this.PowerLimitValue = new NumericUpDown();
            this.PLwattLabel1 = new Label();
            this.LoadMinimumLimit = new Button();
            this.LoadDefaultLimit = new Button();
            this.LoadMaximumLimit = new Button();
            this.ResetBehavior = new Label();
            this.ResetGPUDefaultPL = new RadioButton();
            this.SetGPUPLSpecific = new RadioButton();
            this.SpecificPLValue = new NumericUpDown();
            this.PLwattLabel2 = new Label();
            this.PLimitGroup = new GroupBox();

            this.AutoDetect = new CheckBox();
            this.AutoDetectDesc = new Label();
            this.ThresholdLabel = new Label();
            this.GPUusageThreshold = new NumericUpDown();
            this.PercentLabel = new Label();
            this.CoreLimitEnable = new CheckBox();
            this.CoreClockSetting = new NumericUpDown();
            this.FreqLabel = new Label();
            this.CoreLimitDesc = new Label();
            this.BetaGroup = new GroupBox();

            this.ForceLimit = new Button();
            this.ForceUnlimit = new Button();
            this.LimitStatusText = new Label();
            this.GpuIndex = new ComboBox();

            this.GPUreadTimer = new Timer(this.components);

            this.TimeSetGroupBox.SuspendLayout();
            this.GPUStatusGroup.SuspendLayout();
            this.PLimitGroup.SuspendLayout();
            this.BetaGroup.SuspendLayout();
            this.SuspendLayout();

            //Time Setting Group

            this.BeginTime.CustomFormat = "H:mm";
            this.BeginTime.Font = new Font("HGP創英角ｺﾞｼｯｸUB", 48F, FontStyle.Bold);
            this.BeginTime.Format = DateTimePickerFormat.Custom;
            this.BeginTime.Location = new Point(20, 60);
            this.BeginTime.ShowUpDown = true;
            this.BeginTime.Size = new Size(220, 70);
            this.BeginTime.TabIndex = 0;
            this.BeginTime.ValueChanged += new EventHandler(this.SettingTimeChange);

            this.EndTime.CustomFormat = "H:mm";
            this.EndTime.Font = new Font("HGP創英角ｺﾞｼｯｸUB", 48F, FontStyle.Bold);
            this.EndTime.Format = DateTimePickerFormat.Custom;
            this.EndTime.Location = new Point(280, 60);
            this.EndTime.ShowUpDown = true;
            this.EndTime.Size = new Size(220, 70);
            this.EndTime.TabIndex = 1;
            this.EndTime.ValueChanged += new EventHandler(this.SettingTimeChange);

            this.BeginTimeLabel.Font = new Font("Microsoft Sans Serif", 20F, FontStyle.Bold);
            this.BeginTimeLabel.Location = new Point(10, 20);
            this.BeginTimeLabel.Size = new Size(240, 30);
            this.BeginTimeLabel.Text = "電力制限開始時間";

            this.EndTimeLabel.Font = new Font("Microsoft Sans Serif", 20F, FontStyle.Bold);
            this.EndTimeLabel.Location = new Point(270, 20);
            this.EndTimeLabel.Size = new Size(240, 30);
            this.EndTimeLabel.Text = "電力制限終了時間";

            this.TimeSetGroupBox.Controls.Add(this.BeginTime);
            this.TimeSetGroupBox.Controls.Add(this.EndTime);
            this.TimeSetGroupBox.Controls.Add(this.BeginTimeLabel);
            this.TimeSetGroupBox.Controls.Add(this.EndTimeLabel);
            this.TimeSetGroupBox.Location = new Point(12, 12);
            this.TimeSetGroupBox.Size = new Size(520, 150);
            this.TimeSetGroupBox.Text = "時刻設定";

            //Setting Group

            this.howtouse.Location = new Point(540, 20);
            this.howtouse.Size = new Size(100, 25);
            this.howtouse.TabIndex = 2;
            this.howtouse.Text = "使い方";
            this.howtouse.Click += new EventHandler(this.ShowHowToUse);

            this.bugreport.Location = new Point(540, 60);
            this.bugreport.Size = new Size(100, 25);
            this.bugreport.TabIndex = 3;
            this.bugreport.Text = "フィードバック";
            this.bugreport.Click += new EventHandler(this.Reporter);

            this.PowerLogShow.Location = new Point(540, 100);
            this.PowerLogShow.Size = new Size(100, 25);
            this.PowerLogShow.TabIndex = 4;
            this.PowerLogShow.Text = "電力使用履歴";
            this.PowerLogShow.Click += new EventHandler(this.PowerLogShow_Click);

            this.SettingButton.Location = new Point(542, 140);
            this.SettingButton.Size = new Size(100, 25);
            this.SettingButton.TabIndex = 5;
            this.SettingButton.Text = "設定";
            this.SettingButton.Click += new EventHandler(this.SettingButton_Click);

            //Status Group

            this.GPUCoreTemp.Font = new Font("Nirmala UI", 18F, FontStyle.Bold);
            this.GPUCoreTemp.Location = new Point(10, 25);
            this.GPUCoreTemp.Size = new Size(300, 35);
            this.GPUCoreTemp.Text = "GPUコア温度: 0℃";

            this.GPUTotalPower.Font = new Font("Nirmala UI", 18F, FontStyle.Bold);
            this.GPUTotalPower.Location = new Point(10, 60);
            this.GPUTotalPower.Name = "GPUTotalPower";
            this.GPUTotalPower.Size = new Size(300, 35);
            this.GPUTotalPower.Text = "GPU全体電力: 0W";

            this.GPUCorePLValue.Font = new Font("Nirmala UI", 18F, FontStyle.Bold);
            this.GPUCorePLValue.Location = new Point(10, 95);
            this.GPUCorePLValue.Size = new Size(300, 35);
            this.GPUCorePLValue.Text = "GPUコア電力制限: 0W";

            this.GPUCoreClockValue.Font = new Font("Nirmala UI", 18F, FontStyle.Bold);
            this.GPUCoreClockValue.Location = new Point(10, 130);
            this.GPUCoreClockValue.Size = new Size(300, 35);
            this.GPUCoreClockValue.Text = "GPUコアクロック: 0MHz";

            this.GPUMemoryClockValue.Font = new Font("Nirmala UI", 18F, FontStyle.Bold);
            this.GPUMemoryClockValue.Location = new Point(10, 165);
            this.GPUMemoryClockValue.Size = new Size(300, 35);
            this.GPUMemoryClockValue.Text = "GPUメモリクロック: 0MHz";

            this.GPUStatusGroup.Controls.Add(this.GPUMemoryClockValue);
            this.GPUStatusGroup.Controls.Add(this.GPUCoreClockValue);
            this.GPUStatusGroup.Controls.Add(this.GPUCorePLValue);
            this.GPUStatusGroup.Controls.Add(this.GPUTotalPower);
            this.GPUStatusGroup.Controls.Add(this.GPUCoreTemp);
            this.GPUStatusGroup.Location = new Point(12, 165);
            this.GPUStatusGroup.Size = new Size(330, 200);
            this.GPUStatusGroup.TabStop = false;
            this.GPUStatusGroup.Text = "GPUステータス";

            //PL Group

            this.PLimitLabel.Font = new Font("Microsoft Sans Serif", 18F, FontStyle.Bold);
            this.PLimitLabel.Location = new Point(10, 20);
            this.PLimitLabel.Size = new Size(190, 30);
            this.PLimitLabel.Text = "電力制限値設定";

            this.PowerLimitValue.Font = new Font("Microsoft Sans Serif", 18F, FontStyle.Bold);
            this.PowerLimitValue.Location = new Point(35, 55);
            this.PowerLimitValue.Maximum = 1000;
            this.PowerLimitValue.Size = new Size(90, 35);
            this.PowerLimitValue.TabIndex = 6;
            this.PowerLimitValue.TextAlign = HorizontalAlignment.Right;
            this.PowerLimitValue.ValueChanged += new EventHandler(this.PowerLimitSettingChanged);

            this.PLwattLabel1.Font = new Font("Microsoft Sans Serif", 18F, FontStyle.Bold);
            this.PLwattLabel1.Location = new Point(130, 55);
            this.PLwattLabel1.Size = new Size(35, 30);
            this.PLwattLabel1.Text = "W";

            this.LoadMinimumLimit.Location = new Point(200, 20);
            this.LoadMinimumLimit.Size = new Size(70, 25);
            this.LoadMinimumLimit.TabIndex = 7;
            this.LoadMinimumLimit.Text = "最小値";
            this.LoadMinimumLimit.Click += new EventHandler(this.LoadMinimumLimit_Click);

            this.LoadDefaultLimit.Location = new Point(200, 50);
            this.LoadDefaultLimit.Size = new Size(70, 25);
            this.LoadDefaultLimit.TabIndex = 8;
            this.LoadDefaultLimit.Text = "デフォルト";
            this.LoadDefaultLimit.Click += new EventHandler(this.LoadDefaultLimit_Click);

            this.LoadMaximumLimit.Location = new Point(200, 80);
            this.LoadMaximumLimit.Size = new Size(70, 25);
            this.LoadMaximumLimit.TabIndex = 9;
            this.LoadMaximumLimit.Text = "最大値";
            this.LoadMaximumLimit.Click += new EventHandler(this.LoadMaximumLimit_Click);

            this.ResetBehavior.Location = new Point(15, 100);
            this.ResetBehavior.Size = new Size(80, 15);
            this.ResetBehavior.Text = "解除時の挙動";

            this.ResetGPUDefaultPL.Checked = true;
            this.ResetGPUDefaultPL.Location = new Point(30, 125);
            this.ResetGPUDefaultPL.Size = new Size(140, 20);
            this.ResetGPUDefaultPL.TabIndex = 11;
            this.ResetGPUDefaultPL.Text = "GPUのデフォルトに戻す";

            this.SetGPUPLSpecific.Location = new Point(30, 150);
            this.SetGPUPLSpecific.Size = new Size(140, 20);
            this.SetGPUPLSpecific.TabIndex = 10;
            this.SetGPUPLSpecific.Text = "指定した値にセットする";
            this.SetGPUPLSpecific.CheckedChanged += new EventHandler(this.SetGPUPLSpecific_CheckedChanged);

            this.SpecificPLValue.Enabled = false;
            this.SpecificPLValue.Font = new Font("Microsoft Sans Serif", 9F);
            this.SpecificPLValue.Location = new Point(175, 150);
            this.SpecificPLValue.Maximum = 1000;
            this.SpecificPLValue.Size = new Size(55, 20);
            this.SpecificPLValue.TabIndex = 12;
            this.SpecificPLValue.TextAlign = HorizontalAlignment.Right;
            this.SpecificPLValue.ValueChanged += new EventHandler(this.SpecificPLValue_ValueChanged);

            this.PLwattLabel2.Font = new Font("Microsoft Sans Serif", 9F);
            this.PLwattLabel2.Location = new Point(235, 150);
            this.PLwattLabel2.Size = new Size(20, 15);
            this.PLwattLabel2.Text = "W";

            this.PLimitGroup.Controls.Add(this.PLimitLabel);
            this.PLimitGroup.Controls.Add(this.SpecificPLValue);
            this.PLimitGroup.Controls.Add(this.PLwattLabel1);
            this.PLimitGroup.Controls.Add(this.LoadMinimumLimit);
            this.PLimitGroup.Controls.Add(this.LoadDefaultLimit);
            this.PLimitGroup.Controls.Add(this.LoadMaximumLimit);
            this.PLimitGroup.Controls.Add(this.ResetBehavior);
            this.PLimitGroup.Controls.Add(this.ResetGPUDefaultPL);
            this.PLimitGroup.Controls.Add(this.SetGPUPLSpecific);
            this.PLimitGroup.Controls.Add(this.PowerLimitValue);
            this.PLimitGroup.Controls.Add(this.PLwattLabel2);
            this.PLimitGroup.Location = new Point(355, 165);
            this.PLimitGroup.Size = new Size(280, 180);
            this.PLimitGroup.Text = "コア電力制限設定";

            //Beta Function Group

            this.AutoDetect.Font = new Font("Gadugi", 9F);
            this.AutoDetect.Location = new Point(10, 15);
            this.AutoDetect.Size = new Size(75, 20);
            this.AutoDetect.TabIndex = 13;
            this.AutoDetect.Text = "自動検出";

            this.AutoDetectDesc.Location = new Point(15, 40);
            this.AutoDetectDesc.Size = new Size(170, 30);
            this.AutoDetectDesc.Text = "※自動で寝落ちを検出するモード\r\n GPU使用率に余裕がある人のみ";

            this.ThresholdLabel.Font = new Font("Gadugi", 9F);
            this.ThresholdLabel.Location = new Point(200, 20);
            this.ThresholdLabel.Size = new Size(50, 15);
            this.ThresholdLabel.Text = "しきい値";

            this.GPUusageThreshold.Font = new Font("Microsoft Sans Serif", 9F);
            this.GPUusageThreshold.Location = new Point(205, 40);
            this.GPUusageThreshold.Size = new Size(40, 20);
            this.GPUusageThreshold.TabIndex = 14;
            this.GPUusageThreshold.TextAlign = HorizontalAlignment.Right;
            this.GPUusageThreshold.Value = 20;

            this.PercentLabel.Font = new Font("Microsoft Sans Serif", 9F);
            this.PercentLabel.Location = new Point(250, 40);
            this.PercentLabel.Size = new Size(20, 15);
            this.PercentLabel.Text = "%";

            this.CoreLimitEnable.Font = new Font("Gadugi", 9F);
            this.CoreLimitEnable.Location = new Point(10, 70);
            this.CoreLimitEnable.Size = new Size(100, 20);
            this.CoreLimitEnable.TabIndex = 15;
            this.CoreLimitEnable.Text = "コアクロック制限";

            this.CoreClockSetting.Font = new Font("Microsoft Sans Serif", 9F);
            this.CoreClockSetting.Location = new Point(35, 90);
            this.CoreClockSetting.Maximum = 2000;
            this.CoreClockSetting.Minimum = 210;
            this.CoreClockSetting.Size = new Size(60, 20);
            this.CoreClockSetting.TabIndex = 16;
            this.CoreClockSetting.TextAlign = HorizontalAlignment.Right;
            this.CoreClockSetting.Value = 210;

            this.FreqLabel.Font = new Font("Microsoft Sans Serif", 9F);
            this.FreqLabel.Location = new Point(100, 90);
            this.FreqLabel.Size = new Size(40, 15);
            this.FreqLabel.Text = "MHz";

            this.CoreLimitDesc.Location = new Point(140, 80);
            this.CoreLimitDesc.Size = new Size(100, 30);
            this.CoreLimitDesc.Text = "※下げすぎ注意\r\n 上級者向けです";

            this.BetaGroup.Controls.Add(this.CoreLimitDesc);
            this.BetaGroup.Controls.Add(this.FreqLabel);
            this.BetaGroup.Controls.Add(this.CoreClockSetting);
            this.BetaGroup.Controls.Add(this.CoreLimitEnable);
            this.BetaGroup.Controls.Add(this.ThresholdLabel);
            this.BetaGroup.Controls.Add(this.PercentLabel);
            this.BetaGroup.Controls.Add(this.GPUusageThreshold);
            this.BetaGroup.Controls.Add(this.AutoDetectDesc);
            this.BetaGroup.Controls.Add(this.AutoDetect);
            this.BetaGroup.Location = new Point(355, 347);
            this.BetaGroup.Size = new Size(284, 119);
            this.BetaGroup.Text = "ベータ機能";

            //Othet Control

            this.ForceLimit.Font = new Font("Microsoft Sans Serif", 20F, FontStyle.Bold);
            this.ForceLimit.Location = new Point(10, 380);
            this.ForceLimit.Size = new Size(200, 45);
            this.ForceLimit.TabIndex = 17;
            this.ForceLimit.Text = "強制電力制限";
            this.ForceLimit.Click += new EventHandler(this.ForceLimit_Click);

            this.ForceUnlimit.Font = new Font("Microsoft Sans Serif", 20F, FontStyle.Bold);
            this.ForceUnlimit.Location = new Point(10, 430);
            this.ForceUnlimit.Size = new Size(200, 45);
            this.ForceUnlimit.TabIndex = 18;
            this.ForceUnlimit.Text = "全制限解除";
            this.ForceUnlimit.Click += new EventHandler(this.ForceUnlimit_Click);

            this.GpuIndex.DropDownStyle = ComboBoxStyle.DropDownList;
            this.GpuIndex.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            this.GpuIndex.FormattingEnabled = true;
            this.GpuIndex.Location = new Point(360, 470);
            this.GpuIndex.Size = new Size(280, 30);
            this.GpuIndex.TabIndex = 19;
            this.GpuIndex.SelectedIndexChanged += new EventHandler(this.SelectGPUChanged);

            this.LimitStatusText.BackColor = SystemColors.ControlLight;
            this.LimitStatusText.BorderStyle = BorderStyle.Fixed3D;
            this.LimitStatusText.Font = new Font("Microsoft Sans Serif", 26F, FontStyle.Bold);
            this.LimitStatusText.ForeColor = Color.Red;
            this.LimitStatusText.Location = new Point(222, 409);
            this.LimitStatusText.Size = new Size(127, 41);
            this.LimitStatusText.Text = "制限中";
            this.LimitStatusText.Visible = false;

            this.GPUreadTimer.Interval = 1000;
            this.GPUreadTimer.Tick += new EventHandler(this.GPUreadTimer_Tick);

            //Form

            this.AutoScaleDimensions = new SizeF(6F, 14F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(660, 500);
            this.Controls.Add(this.TimeSetGroupBox);
            this.Controls.Add(this.howtouse);
            this.Controls.Add(this.bugreport);
            this.Controls.Add(this.PowerLogShow);
            this.Controls.Add(this.SettingButton);
            this.Controls.Add(this.GPUStatusGroup);
            this.Controls.Add(this.PLimitGroup);
            this.Controls.Add(this.BetaGroup);
            this.Controls.Add(this.ForceLimit);
            this.Controls.Add(this.ForceUnlimit);
            this.Controls.Add(this.LimitStatusText);
            this.Controls.Add(this.GpuIndex);
            this.Font = new Font("Gadugi", 8F);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "VRChat向け GPU電力制限ツール Ver ";
            this.FormClosing += new FormClosingEventHandler(this.AppClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.BetaGroup.ResumeLayout(false);
            this.BetaGroup.PerformLayout();
            this.TimeSetGroupBox.ResumeLayout(false);
            this.TimeSetGroupBox.PerformLayout();
            this.GPUStatusGroup.ResumeLayout(false);
            this.GPUStatusGroup.PerformLayout();
            this.PLimitGroup.ResumeLayout(false);
            this.PLimitGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Label BeginTimeLabel;
        private Label PLimitLabel;
        private Label PLwattLabel1;
        private Button ForceLimit;
        private Button ForceUnlimit;
        private Timer GPUreadTimer;
        private Button LoadDefaultLimit;
        private CheckBox AutoDetect;
        private Label AutoDetectDesc;
        private Label LimitStatusText;
        private Label PercentLabel;
        private Label ThresholdLabel;
        private GroupBox BetaGroup;
        private Label EndTimeLabel;
        private GroupBox TimeSetGroupBox;
        private Button LoadMinimumLimit;
        private Button LoadMaximumLimit;
        private GroupBox GPUStatusGroup;
        private NumericUpDown CoreClockSetting;
        private CheckBox CoreLimitEnable;
        private GroupBox PLimitGroup;
        private Label FreqLabel;
        private Button howtouse;
        private Button bugreport;
        private Label CoreLimitDesc;
        internal ComboBox GpuIndex;
        internal Label GPUCoreTemp;
        internal Label GPUTotalPower;
        internal Label GPUCorePLValue;
        internal Label GPUMemoryClockValue;
        internal Label GPUCoreClockValue;
        private Label PLwattLabel2;
        private Label ResetBehavior;
        internal NumericUpDown PowerLimitValue;
        internal DateTimePicker BeginTime;
        internal DateTimePicker EndTime;
        internal NumericUpDown SpecificPLValue;
        internal RadioButton SetGPUPLSpecific;
        internal RadioButton ResetGPUDefaultPL;
        private Button PowerLogShow;
        internal NumericUpDown GPUusageThreshold;
        private Button SettingButton;
    }
}

