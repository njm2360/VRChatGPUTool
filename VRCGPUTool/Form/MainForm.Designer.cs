namespace VRCGPUTool.Form
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.BeginTime = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.PowerLimitValue = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.ForceLimit = new System.Windows.Forms.Button();
            this.ForceUnlimit = new System.Windows.Forms.Button();
            this.GpuIndex = new System.Windows.Forms.ComboBox();
            this.GPUreadTimer = new System.Windows.Forms.Timer(this.components);
            this.LoadDefaultLimit = new System.Windows.Forms.Button();
            this.AutoDetect = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.GPUCoreTemp = new System.Windows.Forms.Label();
            this.LimitStatusText = new System.Windows.Forms.Label();
            this.GPUusageThreshold = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.CoreClockSetting = new System.Windows.Forms.NumericUpDown();
            this.CoreLimitEnable = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.EndTime = new System.Windows.Forms.DateTimePicker();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.LoadMinimumLimit = new System.Windows.Forms.Button();
            this.LoadMaximumLimit = new System.Windows.Forms.Button();
            this.GPUTotalPower = new System.Windows.Forms.Label();
            this.GPUCorePLValue = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.GPUMemoryClockValue = new System.Windows.Forms.Label();
            this.GPUCoreClockValue = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.SpecificPLValue = new System.Windows.Forms.NumericUpDown();
            this.SetGPUPLSpecific = new System.Windows.Forms.RadioButton();
            this.ResetGPUDefaultPL = new System.Windows.Forms.RadioButton();
            this.label11 = new System.Windows.Forms.Label();
            this.howtouse = new System.Windows.Forms.Button();
            this.bugreport = new System.Windows.Forms.Button();
            this.functionsuggestion = new System.Windows.Forms.Button();
            this.PowerLogShow = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PowerLimitValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GPUusageThreshold)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CoreClockSetting)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SpecificPLValue)).BeginInit();
            this.SuspendLayout();
            // 
            // BeginTime
            // 
            this.BeginTime.CustomFormat = "H:mm";
            this.BeginTime.Font = new System.Drawing.Font("HGP創英角ｺﾞｼｯｸUB", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.BeginTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.BeginTime.Location = new System.Drawing.Point(17, 60);
            this.BeginTime.Name = "BeginTime";
            this.BeginTime.ShowUpDown = true;
            this.BeginTime.Size = new System.Drawing.Size(218, 71);
            this.BeginTime.TabIndex = 0;
            this.BeginTime.ValueChanged += new System.EventHandler(this.SettingTimeChange);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(11, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(238, 31);
            this.label1.TabIndex = 1;
            this.label1.Text = "電力制限開始時間";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(11, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(188, 29);
            this.label2.TabIndex = 2;
            this.label2.Text = "電力制限値設定";
            // 
            // PowerLimitValue
            // 
            this.PowerLimitValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PowerLimitValue.Location = new System.Drawing.Point(34, 56);
            this.PowerLimitValue.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.PowerLimitValue.Name = "PowerLimitValue";
            this.PowerLimitValue.Size = new System.Drawing.Size(92, 35);
            this.PowerLimitValue.TabIndex = 3;
            this.PowerLimitValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PowerLimitValue.ValueChanged += new System.EventHandler(this.PowerLimitSettingChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(135, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 29);
            this.label3.TabIndex = 4;
            this.label3.Text = "W";
            // 
            // ForceLimit
            // 
            this.ForceLimit.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForceLimit.Location = new System.Drawing.Point(12, 382);
            this.ForceLimit.Name = "ForceLimit";
            this.ForceLimit.Size = new System.Drawing.Size(205, 44);
            this.ForceLimit.TabIndex = 7;
            this.ForceLimit.Text = "強制電力制限";
            this.ForceLimit.UseVisualStyleBackColor = true;
            this.ForceLimit.Click += new System.EventHandler(this.ForceLimit_Click);
            // 
            // ForceUnlimit
            // 
            this.ForceUnlimit.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForceUnlimit.Location = new System.Drawing.Point(12, 434);
            this.ForceUnlimit.Name = "ForceUnlimit";
            this.ForceUnlimit.Size = new System.Drawing.Size(205, 44);
            this.ForceUnlimit.TabIndex = 8;
            this.ForceUnlimit.Text = "全制限解除";
            this.ForceUnlimit.UseVisualStyleBackColor = true;
            this.ForceUnlimit.Click += new System.EventHandler(this.ForceUnlimit_Click);
            // 
            // GpuIndex
            // 
            this.GpuIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.GpuIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GpuIndex.FormattingEnabled = true;
            this.GpuIndex.Location = new System.Drawing.Point(357, 472);
            this.GpuIndex.Name = "GpuIndex";
            this.GpuIndex.Size = new System.Drawing.Size(284, 28);
            this.GpuIndex.TabIndex = 9;
            this.GpuIndex.SelectedIndexChanged += new System.EventHandler(this.SelectGPUChanged);
            // 
            // GPUreadTimer
            // 
            this.GPUreadTimer.Interval = 1000;
            this.GPUreadTimer.Tick += new System.EventHandler(this.GPUreadTimer_Tick);
            // 
            // LoadDefaultLimit
            // 
            this.LoadDefaultLimit.Location = new System.Drawing.Point(197, 50);
            this.LoadDefaultLimit.Name = "LoadDefaultLimit";
            this.LoadDefaultLimit.Size = new System.Drawing.Size(68, 25);
            this.LoadDefaultLimit.TabIndex = 10;
            this.LoadDefaultLimit.Text = "デフォルト";
            this.LoadDefaultLimit.UseVisualStyleBackColor = true;
            this.LoadDefaultLimit.Click += new System.EventHandler(this.LoadDefaultLimit_Click);
            // 
            // AutoDetect
            // 
            this.AutoDetect.AutoSize = true;
            this.AutoDetect.Font = new System.Drawing.Font("Gadugi", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AutoDetect.Location = new System.Drawing.Point(13, 15);
            this.AutoDetect.Name = "AutoDetect";
            this.AutoDetect.Size = new System.Drawing.Size(74, 20);
            this.AutoDetect.TabIndex = 11;
            this.AutoDetect.Text = "自動検出";
            this.AutoDetect.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 38);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(168, 28);
            this.label5.TabIndex = 12;
            this.label5.Text = "※自動で寝落ちを検出するモード\r\n　GPU使用率に余裕がある人のみ";
            // 
            // GPUCoreTemp
            // 
            this.GPUCoreTemp.AutoSize = true;
            this.GPUCoreTemp.Font = new System.Drawing.Font("Nirmala UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GPUCoreTemp.Location = new System.Drawing.Point(11, 27);
            this.GPUCoreTemp.Name = "GPUCoreTemp";
            this.GPUCoreTemp.Size = new System.Drawing.Size(203, 32);
            this.GPUCoreTemp.TabIndex = 13;
            this.GPUCoreTemp.Text = "GPUコア温度: 0℃";
            // 
            // LimitStatusText
            // 
            this.LimitStatusText.AutoSize = true;
            this.LimitStatusText.BackColor = System.Drawing.SystemColors.ControlLight;
            this.LimitStatusText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LimitStatusText.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LimitStatusText.ForeColor = System.Drawing.Color.Red;
            this.LimitStatusText.Location = new System.Drawing.Point(222, 409);
            this.LimitStatusText.Name = "LimitStatusText";
            this.LimitStatusText.Size = new System.Drawing.Size(127, 41);
            this.LimitStatusText.TabIndex = 16;
            this.LimitStatusText.Text = "制限中";
            this.LimitStatusText.Visible = false;
            // 
            // GPUusageThreshold
            // 
            this.GPUusageThreshold.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GPUusageThreshold.Location = new System.Drawing.Point(205, 42);
            this.GPUusageThreshold.Name = "GPUusageThreshold";
            this.GPUusageThreshold.Size = new System.Drawing.Size(41, 21);
            this.GPUusageThreshold.TabIndex = 17;
            this.GPUusageThreshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.GPUusageThreshold.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(252, 44);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(18, 15);
            this.label6.TabIndex = 18;
            this.label6.Text = "%";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Gadugi", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(203, 19);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 16);
            this.label7.TabIndex = 19;
            this.label7.Text = "しきい値";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.CoreClockSetting);
            this.groupBox1.Controls.Add(this.CoreLimitEnable);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.GPUusageThreshold);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.AutoDetect);
            this.groupBox1.Location = new System.Drawing.Point(355, 347);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(284, 119);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ベータ機能";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(138, 79);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(93, 28);
            this.label10.TabIndex = 30;
            this.label10.Text = "※下げすぎ注意\r\n　上級者向けです";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(99, 92);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(33, 15);
            this.label9.TabIndex = 29;
            this.label9.Text = "MHz";
            // 
            // CoreClockSetting
            // 
            this.CoreClockSetting.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CoreClockSetting.Location = new System.Drawing.Point(34, 90);
            this.CoreClockSetting.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.CoreClockSetting.Minimum = new decimal(new int[] {
            210,
            0,
            0,
            0});
            this.CoreClockSetting.Name = "CoreClockSetting";
            this.CoreClockSetting.Size = new System.Drawing.Size(59, 21);
            this.CoreClockSetting.TabIndex = 27;
            this.CoreClockSetting.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.CoreClockSetting.Value = new decimal(new int[] {
            210,
            0,
            0,
            0});
            // 
            // CoreLimitEnable
            // 
            this.CoreLimitEnable.AutoSize = true;
            this.CoreLimitEnable.Font = new System.Drawing.Font("Gadugi", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CoreLimitEnable.Location = new System.Drawing.Point(13, 69);
            this.CoreLimitEnable.Name = "CoreLimitEnable";
            this.CoreLimitEnable.Size = new System.Drawing.Size(99, 20);
            this.CoreLimitEnable.TabIndex = 20;
            this.CoreLimitEnable.Text = "コアクロック制限";
            this.CoreLimitEnable.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(276, 18);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(238, 31);
            this.label8.TabIndex = 22;
            this.label8.Text = "電力制限終了時間";
            // 
            // EndTime
            // 
            this.EndTime.CustomFormat = "H:mm";
            this.EndTime.Font = new System.Drawing.Font("HGP創英角ｺﾞｼｯｸUB", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.EndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.EndTime.Location = new System.Drawing.Point(282, 60);
            this.EndTime.Name = "EndTime";
            this.EndTime.ShowUpDown = true;
            this.EndTime.Size = new System.Drawing.Size(218, 71);
            this.EndTime.TabIndex = 21;
            this.EndTime.ValueChanged += new System.EventHandler(this.SettingTimeChange);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.EndTime);
            this.groupBox2.Controls.Add(this.BeginTime);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(520, 147);
            this.groupBox2.TabIndex = 24;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "時刻設定";
            // 
            // LoadMinimumLimit
            // 
            this.LoadMinimumLimit.Location = new System.Drawing.Point(197, 21);
            this.LoadMinimumLimit.Name = "LoadMinimumLimit";
            this.LoadMinimumLimit.Size = new System.Drawing.Size(68, 25);
            this.LoadMinimumLimit.TabIndex = 25;
            this.LoadMinimumLimit.Text = "最小値";
            this.LoadMinimumLimit.UseVisualStyleBackColor = true;
            this.LoadMinimumLimit.Click += new System.EventHandler(this.LoadMinimumLimit_Click);
            // 
            // LoadMaximumLimit
            // 
            this.LoadMaximumLimit.Location = new System.Drawing.Point(197, 78);
            this.LoadMaximumLimit.Name = "LoadMaximumLimit";
            this.LoadMaximumLimit.Size = new System.Drawing.Size(68, 25);
            this.LoadMaximumLimit.TabIndex = 26;
            this.LoadMaximumLimit.Text = "最大値";
            this.LoadMaximumLimit.UseVisualStyleBackColor = true;
            this.LoadMaximumLimit.Click += new System.EventHandler(this.LoadMaximumLimit_Click);
            // 
            // GPUTotalPower
            // 
            this.GPUTotalPower.AutoSize = true;
            this.GPUTotalPower.Font = new System.Drawing.Font("Nirmala UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GPUTotalPower.Location = new System.Drawing.Point(11, 59);
            this.GPUTotalPower.Name = "GPUTotalPower";
            this.GPUTotalPower.Size = new System.Drawing.Size(215, 32);
            this.GPUTotalPower.TabIndex = 27;
            this.GPUTotalPower.Text = "GPU全体電力: 0W";
            // 
            // GPUCorePLValue
            // 
            this.GPUCorePLValue.AutoSize = true;
            this.GPUCorePLValue.Font = new System.Drawing.Font("Nirmala UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GPUCorePLValue.Location = new System.Drawing.Point(11, 93);
            this.GPUCorePLValue.Name = "GPUCorePLValue";
            this.GPUCorePLValue.Size = new System.Drawing.Size(252, 32);
            this.GPUCorePLValue.TabIndex = 28;
            this.GPUCorePLValue.Text = "GPUコア電力制限: 0W";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.GPUMemoryClockValue);
            this.groupBox3.Controls.Add(this.GPUCoreClockValue);
            this.groupBox3.Controls.Add(this.GPUCorePLValue);
            this.groupBox3.Controls.Add(this.GPUTotalPower);
            this.groupBox3.Controls.Add(this.GPUCoreTemp);
            this.groupBox3.Location = new System.Drawing.Point(13, 165);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(336, 201);
            this.groupBox3.TabIndex = 29;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "GPUステータス";
            // 
            // GPUMemoryClockValue
            // 
            this.GPUMemoryClockValue.AutoSize = true;
            this.GPUMemoryClockValue.Font = new System.Drawing.Font("Nirmala UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GPUMemoryClockValue.Location = new System.Drawing.Point(13, 157);
            this.GPUMemoryClockValue.Name = "GPUMemoryClockValue";
            this.GPUMemoryClockValue.Size = new System.Drawing.Size(265, 32);
            this.GPUMemoryClockValue.TabIndex = 30;
            this.GPUMemoryClockValue.Text = "GPUメモリクロック: 0MHz";
            // 
            // GPUCoreClockValue
            // 
            this.GPUCoreClockValue.AutoSize = true;
            this.GPUCoreClockValue.Font = new System.Drawing.Font("Nirmala UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GPUCoreClockValue.Location = new System.Drawing.Point(11, 125);
            this.GPUCoreClockValue.Name = "GPUCoreClockValue";
            this.GPUCoreClockValue.Size = new System.Drawing.Size(250, 32);
            this.GPUCoreClockValue.TabIndex = 29;
            this.GPUCoreClockValue.Text = "GPUコアクロック: 0MHz";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label12);
            this.groupBox4.Controls.Add(this.SpecificPLValue);
            this.groupBox4.Controls.Add(this.SetGPUPLSpecific);
            this.groupBox4.Controls.Add(this.ResetGPUDefaultPL);
            this.groupBox4.Controls.Add(this.label11);
            this.groupBox4.Controls.Add(this.LoadMaximumLimit);
            this.groupBox4.Controls.Add(this.LoadMinimumLimit);
            this.groupBox4.Controls.Add(this.LoadDefaultLimit);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.PowerLimitValue);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Location = new System.Drawing.Point(355, 165);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(284, 176);
            this.groupBox4.TabIndex = 30;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "コア電力制限設定";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(232, 150);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(18, 15);
            this.label12.TabIndex = 35;
            this.label12.Text = "W";
            // 
            // SpecificPLValue
            // 
            this.SpecificPLValue.Enabled = false;
            this.SpecificPLValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SpecificPLValue.Location = new System.Drawing.Point(173, 148);
            this.SpecificPLValue.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.SpecificPLValue.Name = "SpecificPLValue";
            this.SpecificPLValue.Size = new System.Drawing.Size(53, 21);
            this.SpecificPLValue.TabIndex = 34;
            this.SpecificPLValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.SpecificPLValue.ValueChanged += new System.EventHandler(this.SpecificPLValue_ValueChanged);
            // 
            // SetGPUPLSpecific
            // 
            this.SetGPUPLSpecific.AutoSize = true;
            this.SetGPUPLSpecific.Location = new System.Drawing.Point(32, 149);
            this.SetGPUPLSpecific.Name = "SetGPUPLSpecific";
            this.SetGPUPLSpecific.Size = new System.Drawing.Size(132, 18);
            this.SetGPUPLSpecific.TabIndex = 33;
            this.SetGPUPLSpecific.Text = "指定した値にセットする";
            this.SetGPUPLSpecific.UseVisualStyleBackColor = true;
            this.SetGPUPLSpecific.CheckedChanged += new System.EventHandler(this.SetGPUPLSpecific_CheckedChanged);
            // 
            // ResetGPUDefaultPL
            // 
            this.ResetGPUDefaultPL.AutoSize = true;
            this.ResetGPUDefaultPL.Checked = true;
            this.ResetGPUDefaultPL.Location = new System.Drawing.Point(32, 125);
            this.ResetGPUDefaultPL.Name = "ResetGPUDefaultPL";
            this.ResetGPUDefaultPL.Size = new System.Drawing.Size(132, 18);
            this.ResetGPUDefaultPL.TabIndex = 32;
            this.ResetGPUDefaultPL.TabStop = true;
            this.ResetGPUDefaultPL.Text = "GPUのデフォルトに戻す";
            this.ResetGPUDefaultPL.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(16, 107);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(77, 14);
            this.label11.TabIndex = 31;
            this.label11.Text = "解除時の挙動";
            // 
            // howtouse
            // 
            this.howtouse.Location = new System.Drawing.Point(540, 39);
            this.howtouse.Name = "howtouse";
            this.howtouse.Size = new System.Drawing.Size(99, 25);
            this.howtouse.TabIndex = 27;
            this.howtouse.Text = "使い方";
            this.howtouse.UseVisualStyleBackColor = true;
            this.howtouse.Click += new System.EventHandler(this.ShowHowToUse);
            // 
            // bugreport
            // 
            this.bugreport.Location = new System.Drawing.Point(540, 72);
            this.bugreport.Name = "bugreport";
            this.bugreport.Size = new System.Drawing.Size(99, 25);
            this.bugreport.TabIndex = 32;
            this.bugreport.Tag = "0";
            this.bugreport.Text = "バグ報告";
            this.bugreport.UseVisualStyleBackColor = true;
            this.bugreport.Click += new System.EventHandler(this.Reporter);
            // 
            // functionsuggestion
            // 
            this.functionsuggestion.Location = new System.Drawing.Point(540, 103);
            this.functionsuggestion.Name = "functionsuggestion";
            this.functionsuggestion.Size = new System.Drawing.Size(99, 25);
            this.functionsuggestion.TabIndex = 33;
            this.functionsuggestion.Tag = "1";
            this.functionsuggestion.Text = "機能要望等";
            this.functionsuggestion.UseVisualStyleBackColor = true;
            this.functionsuggestion.Click += new System.EventHandler(this.Reporter);
            // 
            // PowerLogShow
            // 
            this.PowerLogShow.Location = new System.Drawing.Point(542, 134);
            this.PowerLogShow.Name = "PowerLogShow";
            this.PowerLogShow.Size = new System.Drawing.Size(99, 25);
            this.PowerLogShow.TabIndex = 34;
            this.PowerLogShow.Tag = "1";
            this.PowerLogShow.Text = "電力使用履歴";
            this.PowerLogShow.UseVisualStyleBackColor = true;
            this.PowerLogShow.Click += new System.EventHandler(this.PowerLogShow_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(661, 507);
            this.Controls.Add(this.PowerLogShow);
            this.Controls.Add(this.functionsuggestion);
            this.Controls.Add(this.bugreport);
            this.Controls.Add(this.howtouse);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.LimitStatusText);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.GpuIndex);
            this.Controls.Add(this.ForceUnlimit);
            this.Controls.Add(this.ForceLimit);
            this.Font = new System.Drawing.Font("Gadugi", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VRChat向け　GPU電力制限ツール Ver ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AppClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PowerLimitValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GPUusageThreshold)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CoreClockSetting)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SpecificPLValue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button ForceLimit;
        private System.Windows.Forms.Button ForceUnlimit;
        private System.Windows.Forms.Timer GPUreadTimer;
        private System.Windows.Forms.Button LoadDefaultLimit;
        private System.Windows.Forms.CheckBox AutoDetect;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label LimitStatusText;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button LoadMinimumLimit;
        private System.Windows.Forms.Button LoadMaximumLimit;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.NumericUpDown CoreClockSetting;
        private System.Windows.Forms.CheckBox CoreLimitEnable;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button howtouse;
        private System.Windows.Forms.Button bugreport;
        private System.Windows.Forms.Button functionsuggestion;
        private System.Windows.Forms.Label label10;
        internal System.Windows.Forms.ComboBox GpuIndex;
        internal System.Windows.Forms.Label GPUCoreTemp;
        internal System.Windows.Forms.Label GPUTotalPower;
        internal System.Windows.Forms.Label GPUCorePLValue;
        internal System.Windows.Forms.Label GPUMemoryClockValue;
        internal System.Windows.Forms.Label GPUCoreClockValue;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        internal System.Windows.Forms.NumericUpDown PowerLimitValue;
        internal System.Windows.Forms.DateTimePicker BeginTime;
        internal System.Windows.Forms.DateTimePicker EndTime;
        internal System.Windows.Forms.NumericUpDown SpecificPLValue;
        internal System.Windows.Forms.RadioButton SetGPUPLSpecific;
        internal System.Windows.Forms.RadioButton ResetGPUDefaultPL;
        private System.Windows.Forms.Button PowerLogShow;
        internal System.Windows.Forms.NumericUpDown GPUusageThreshold;
    }
}

