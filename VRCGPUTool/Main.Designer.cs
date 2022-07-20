namespace VRCGPUTool
{
    partial class Main
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
            this.label4 = new System.Windows.Forms.Label();
            this.GPUMemoryClockValue = new System.Windows.Forms.Label();
            this.GPUCoreClockValue = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.menu = new System.Windows.Forms.MenuStrip();
            this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.終了XToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ヘルプHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boothpagelink = new System.Windows.Forms.ToolStripMenuItem();
            this.githubpagelink = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHowToUse = new System.Windows.Forms.ToolStripMenuItem();
            this.howtouse = new System.Windows.Forms.Button();
            this.bugreport = new System.Windows.Forms.Button();
            this.functionsuggestion = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.PowerLimitValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GPUusageThreshold)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CoreClockSetting)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.menu.SuspendLayout();
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
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(11, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(225, 33);
            this.label2.TabIndex = 2;
            this.label2.Text = "電力制限値設定";
            // 
            // PowerLimitValue
            // 
            this.PowerLimitValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PowerLimitValue.Location = new System.Drawing.Point(34, 54);
            this.PowerLimitValue.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.PowerLimitValue.Name = "PowerLimitValue";
            this.PowerLimitValue.Size = new System.Drawing.Size(137, 47);
            this.PowerLimitValue.TabIndex = 3;
            this.PowerLimitValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PowerLimitValue.ValueChanged += new System.EventHandler(this.PowerLimitSettingChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(177, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 39);
            this.label3.TabIndex = 4;
            this.label3.Text = "W";
            // 
            // ForceLimit
            // 
            this.ForceLimit.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForceLimit.Location = new System.Drawing.Point(13, 448);
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
            this.ForceUnlimit.Location = new System.Drawing.Point(13, 500);
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
            this.GpuIndex.Location = new System.Drawing.Point(357, 500);
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
            this.LoadDefaultLimit.Location = new System.Drawing.Point(91, 105);
            this.LoadDefaultLimit.Name = "LoadDefaultLimit";
            this.LoadDefaultLimit.Size = new System.Drawing.Size(59, 25);
            this.LoadDefaultLimit.TabIndex = 10;
            this.LoadDefaultLimit.Text = "デフォルト";
            this.LoadDefaultLimit.UseVisualStyleBackColor = true;
            this.LoadDefaultLimit.Click += new System.EventHandler(this.LoadDefaultLimit_Click);
            // 
            // AutoDetect
            // 
            this.AutoDetect.AutoSize = true;
            this.AutoDetect.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AutoDetect.Location = new System.Drawing.Point(13, 15);
            this.AutoDetect.Name = "AutoDetect";
            this.AutoDetect.Size = new System.Drawing.Size(109, 28);
            this.AutoDetect.TabIndex = 11;
            this.AutoDetect.Text = "自動検出";
            this.AutoDetect.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(27, 46);
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
            this.LimitStatusText.Location = new System.Drawing.Point(224, 469);
            this.LimitStatusText.Name = "LimitStatusText";
            this.LimitStatusText.Size = new System.Drawing.Size(127, 41);
            this.LimitStatusText.TabIndex = 16;
            this.LimitStatusText.Text = "制限中";
            this.LimitStatusText.Visible = false;
            // 
            // GPUusageThreshold
            // 
            this.GPUusageThreshold.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GPUusageThreshold.Location = new System.Drawing.Point(209, 46);
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
            this.label6.Location = new System.Drawing.Point(252, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(18, 15);
            this.label6.TabIndex = 18;
            this.label6.Text = "%";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(206, 27);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 15);
            this.label7.TabIndex = 19;
            this.label7.Text = "しきい値";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.CoreClockSetting);
            this.groupBox1.Controls.Add(this.CoreLimitEnable);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.GPUusageThreshold);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.AutoDetect);
            this.groupBox1.Location = new System.Drawing.Point(355, 322);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(284, 157);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ベータ機能";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(99, 119);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(33, 15);
            this.label9.TabIndex = 29;
            this.label9.Text = "MHz";
            // 
            // CoreClockSetting
            // 
            this.CoreClockSetting.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CoreClockSetting.Location = new System.Drawing.Point(34, 117);
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
            this.CoreLimitEnable.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CoreLimitEnable.Location = new System.Drawing.Point(13, 92);
            this.CoreLimitEnable.Name = "CoreLimitEnable";
            this.CoreLimitEnable.Size = new System.Drawing.Size(153, 28);
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
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.EndTime);
            this.groupBox2.Controls.Add(this.BeginTime);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 27);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(520, 147);
            this.groupBox2.TabIndex = 24;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "時刻設定";
            // 
            // LoadMinimumLimit
            // 
            this.LoadMinimumLimit.Location = new System.Drawing.Point(34, 105);
            this.LoadMinimumLimit.Name = "LoadMinimumLimit";
            this.LoadMinimumLimit.Size = new System.Drawing.Size(51, 25);
            this.LoadMinimumLimit.TabIndex = 25;
            this.LoadMinimumLimit.Text = "最小値";
            this.LoadMinimumLimit.UseVisualStyleBackColor = true;
            this.LoadMinimumLimit.Click += new System.EventHandler(this.LoadMinimumLimit_Click);
            // 
            // LoadMaximumLimit
            // 
            this.LoadMaximumLimit.Location = new System.Drawing.Point(156, 105);
            this.LoadMaximumLimit.Name = "LoadMaximumLimit";
            this.LoadMaximumLimit.Size = new System.Drawing.Size(53, 25);
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
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.GPUMemoryClockValue);
            this.groupBox3.Controls.Add(this.GPUCoreClockValue);
            this.groupBox3.Controls.Add(this.GPUCorePLValue);
            this.groupBox3.Controls.Add(this.GPUTotalPower);
            this.groupBox3.Controls.Add(this.GPUCoreTemp);
            this.groupBox3.Location = new System.Drawing.Point(13, 180);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(336, 247);
            this.groupBox3.TabIndex = 29;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "GPUステータス";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 198);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(251, 28);
            this.label4.TabIndex = 30;
            this.label4.Text = "※GPU全体電力はコアの電力制限値より大きくなる\r\n場合があります（コア以外の消費電力を含むため）";
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
            this.groupBox4.Controls.Add(this.LoadMaximumLimit);
            this.groupBox4.Controls.Add(this.LoadMinimumLimit);
            this.groupBox4.Controls.Add(this.LoadDefaultLimit);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.PowerLimitValue);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Location = new System.Drawing.Point(355, 180);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(284, 136);
            this.groupBox4.TabIndex = 30;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "コア電力制限設定";
            // 
            // menu
            // 
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuExit,
            this.ヘルプHToolStripMenuItem});
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Name = "menu";
            this.menu.Size = new System.Drawing.Size(661, 24);
            this.menu.TabIndex = 31;
            this.menu.Text = "menuStrip1";
            // 
            // menuExit
            // 
            this.menuExit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.終了XToolStripMenuItem});
            this.menuExit.Name = "menuExit";
            this.menuExit.Size = new System.Drawing.Size(70, 20);
            this.menuExit.Text = "ファイル(&F)";
            // 
            // 終了XToolStripMenuItem
            // 
            this.終了XToolStripMenuItem.Name = "終了XToolStripMenuItem";
            this.終了XToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.終了XToolStripMenuItem.Text = "終了(&X)";
            // 
            // ヘルプHToolStripMenuItem
            // 
            this.ヘルプHToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.boothpagelink,
            this.githubpagelink,
            this.menuHowToUse});
            this.ヘルプHToolStripMenuItem.Name = "ヘルプHToolStripMenuItem";
            this.ヘルプHToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.ヘルプHToolStripMenuItem.Text = "ヘルプ(&H)";
            // 
            // boothpagelink
            // 
            this.boothpagelink.Name = "boothpagelink";
            this.boothpagelink.Size = new System.Drawing.Size(199, 22);
            this.boothpagelink.Text = "BOOTHページ（リンク）";
            // 
            // githubpagelink
            // 
            this.githubpagelink.Name = "githubpagelink";
            this.githubpagelink.Size = new System.Drawing.Size(199, 22);
            this.githubpagelink.Text = "GitHubページ（リンク）";
            // 
            // menuHowToUse
            // 
            this.menuHowToUse.Name = "menuHowToUse";
            this.menuHowToUse.Size = new System.Drawing.Size(199, 22);
            this.menuHowToUse.Text = "使い方";
            this.menuHowToUse.Click += new System.EventHandler(this.ShowHowToUse);
            // 
            // howtouse
            // 
            this.howtouse.Location = new System.Drawing.Point(540, 54);
            this.howtouse.Name = "howtouse";
            this.howtouse.Size = new System.Drawing.Size(99, 25);
            this.howtouse.TabIndex = 27;
            this.howtouse.Text = "使い方";
            this.howtouse.UseVisualStyleBackColor = true;
            this.howtouse.Click += new System.EventHandler(this.ShowHowToUse);
            // 
            // bugreport
            // 
            this.bugreport.Location = new System.Drawing.Point(540, 87);
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
            this.functionsuggestion.Location = new System.Drawing.Point(540, 118);
            this.functionsuggestion.Name = "functionsuggestion";
            this.functionsuggestion.Size = new System.Drawing.Size(99, 25);
            this.functionsuggestion.TabIndex = 33;
            this.functionsuggestion.Tag = "1";
            this.functionsuggestion.Text = "機能要望等";
            this.functionsuggestion.UseVisualStyleBackColor = true;
            this.functionsuggestion.Click += new System.EventHandler(this.Reporter);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(527, 432);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(93, 28);
            this.label10.TabIndex = 30;
            this.label10.Text = "※下げすぎ注意\r\n　上級者向けです";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(661, 554);
            this.Controls.Add(this.label10);
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
            this.Controls.Add(this.menu);
            this.Font = new System.Drawing.Font("Gadugi", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MainMenuStrip = this.menu;
            this.MaximizeBox = false;
            this.Name = "Main";
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
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker BeginTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown PowerLimitValue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button ForceLimit;
        private System.Windows.Forms.Button ForceUnlimit;
        private System.Windows.Forms.ComboBox GpuIndex;
        private System.Windows.Forms.Timer GPUreadTimer;
        private System.Windows.Forms.Button LoadDefaultLimit;
        private System.Windows.Forms.CheckBox AutoDetect;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label GPUCoreTemp;
        private System.Windows.Forms.Label LimitStatusText;
        private System.Windows.Forms.NumericUpDown GPUusageThreshold;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DateTimePicker EndTime;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button LoadMinimumLimit;
        private System.Windows.Forms.Button LoadMaximumLimit;
        private System.Windows.Forms.Label GPUTotalPower;
        private System.Windows.Forms.Label GPUCorePLValue;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label GPUMemoryClockValue;
        private System.Windows.Forms.Label GPUCoreClockValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown CoreClockSetting;
        private System.Windows.Forms.CheckBox CoreLimitEnable;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.MenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem menuExit;
        private System.Windows.Forms.ToolStripMenuItem 終了XToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ヘルプHToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boothpagelink;
        private System.Windows.Forms.ToolStripMenuItem githubpagelink;
        private System.Windows.Forms.ToolStripMenuItem menuHowToUse;
        private System.Windows.Forms.Button howtouse;
        private System.Windows.Forms.Button bugreport;
        private System.Windows.Forms.Button functionsuggestion;
        private System.Windows.Forms.Label label10;
    }
}

