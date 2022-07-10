namespace VRCGPUTool
{
    partial class Form1
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
            this.label4 = new System.Windows.Forms.Label();
            this.StatusLimit = new System.Windows.Forms.Label();
            this.ForceLimit = new System.Windows.Forms.Button();
            this.ForceUnlimit = new System.Windows.Forms.Button();
            this.GpuIndex = new System.Windows.Forms.ComboBox();
            this.GPUreadTimer = new System.Windows.Forms.Timer(this.components);
            this.LoadDefaultLimit = new System.Windows.Forms.Button();
            this.AutoDetect = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.GPUTemp = new System.Windows.Forms.Label();
            this.LimitStatusText = new System.Windows.Forms.Label();
            this.GPUusageThreshold = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.EndTime = new System.Windows.Forms.DateTimePicker();
            ((System.ComponentModel.ISupportInitialize)(this.PowerLimitValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GPUusageThreshold)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BeginTime
            // 
            this.BeginTime.CustomFormat = "H:mm";
            this.BeginTime.Font = new System.Drawing.Font("HGP創英角ｺﾞｼｯｸUB", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.BeginTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.BeginTime.Location = new System.Drawing.Point(63, 62);
            this.BeginTime.Name = "BeginTime";
            this.BeginTime.ShowUpDown = true;
            this.BeginTime.Size = new System.Drawing.Size(218, 71);
            this.BeginTime.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(7, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(305, 39);
            this.label1.TabIndex = 1;
            this.label1.Text = "電力制限開始時間";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(18, 259);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(269, 39);
            this.label2.TabIndex = 2;
            this.label2.Text = "電力制限値設定";
            // 
            // PowerLimitValue
            // 
            this.PowerLimitValue.Font = new System.Drawing.Font("HGP創英角ｺﾞｼｯｸUB", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PowerLimitValue.Location = new System.Drawing.Point(48, 320);
            this.PowerLimitValue.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.PowerLimitValue.Name = "PowerLimitValue";
            this.PowerLimitValue.Size = new System.Drawing.Size(196, 71);
            this.PowerLimitValue.TabIndex = 3;
            this.PowerLimitValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PowerLimitValue.ValueChanged += new System.EventHandler(this.PowerLimitSettingChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(266, 334);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 39);
            this.label3.TabIndex = 4;
            this.label3.Text = "W";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(383, 259);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(299, 39);
            this.label4.TabIndex = 5;
            this.label4.Text = "現在の電力制限値";
            // 
            // StatusLimit
            // 
            this.StatusLimit.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusLimit.ForeColor = System.Drawing.Color.Black;
            this.StatusLimit.Location = new System.Drawing.Point(455, 312);
            this.StatusLimit.Name = "StatusLimit";
            this.StatusLimit.Size = new System.Drawing.Size(227, 79);
            this.StatusLimit.TabIndex = 6;
            this.StatusLimit.Text = "0W";
            this.StatusLimit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ForceLimit
            // 
            this.ForceLimit.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForceLimit.Location = new System.Drawing.Point(70, 446);
            this.ForceLimit.Name = "ForceLimit";
            this.ForceLimit.Size = new System.Drawing.Size(247, 44);
            this.ForceLimit.TabIndex = 7;
            this.ForceLimit.Text = "強制電力制限";
            this.ForceLimit.UseVisualStyleBackColor = true;
            this.ForceLimit.Click += new System.EventHandler(this.ForceLimit_Click);
            // 
            // ForceUnlimit
            // 
            this.ForceUnlimit.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForceUnlimit.Location = new System.Drawing.Point(418, 446);
            this.ForceUnlimit.Name = "ForceUnlimit";
            this.ForceUnlimit.Size = new System.Drawing.Size(247, 44);
            this.ForceUnlimit.TabIndex = 8;
            this.ForceUnlimit.Text = "電力制限解除";
            this.ForceUnlimit.UseVisualStyleBackColor = true;
            this.ForceUnlimit.Click += new System.EventHandler(this.ForceUnlimit_Click);
            // 
            // GpuIndex
            // 
            this.GpuIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.GpuIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GpuIndex.FormattingEnabled = true;
            this.GpuIndex.Location = new System.Drawing.Point(32, 509);
            this.GpuIndex.Name = "GpuIndex";
            this.GpuIndex.Size = new System.Drawing.Size(617, 33);
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
            this.LoadDefaultLimit.Location = new System.Drawing.Point(70, 399);
            this.LoadDefaultLimit.Name = "LoadDefaultLimit";
            this.LoadDefaultLimit.Size = new System.Drawing.Size(109, 25);
            this.LoadDefaultLimit.TabIndex = 10;
            this.LoadDefaultLimit.Text = "デフォルト値をロード";
            this.LoadDefaultLimit.UseVisualStyleBackColor = true;
            this.LoadDefaultLimit.Click += new System.EventHandler(this.LoadDefaultLimit_Click);
            // 
            // AutoDetect
            // 
            this.AutoDetect.AutoSize = true;
            this.AutoDetect.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AutoDetect.Location = new System.Drawing.Point(13, 15);
            this.AutoDetect.Name = "AutoDetect";
            this.AutoDetect.Size = new System.Drawing.Size(191, 28);
            this.AutoDetect.TabIndex = 11;
            this.AutoDetect.Text = "自動検出(ベータ版)";
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
            // GPUTemp
            // 
            this.GPUTemp.AutoSize = true;
            this.GPUTemp.Font = new System.Drawing.Font("Nirmala UI", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GPUTemp.Location = new System.Drawing.Point(414, 172);
            this.GPUTemp.Name = "GPUTemp";
            this.GPUTemp.Size = new System.Drawing.Size(251, 50);
            this.GPUTemp.TabIndex = 13;
            this.GPUTemp.Text = "GPU温度: 0℃";
            // 
            // LimitStatusText
            // 
            this.LimitStatusText.AutoSize = true;
            this.LimitStatusText.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LimitStatusText.ForeColor = System.Drawing.Color.Red;
            this.LimitStatusText.Location = new System.Drawing.Point(484, 399);
            this.LimitStatusText.Name = "LimitStatusText";
            this.LimitStatusText.Size = new System.Drawing.Size(125, 39);
            this.LimitStatusText.TabIndex = 16;
            this.LimitStatusText.Text = "制限中";
            this.LimitStatusText.Visible = false;
            // 
            // GPUusageThreshold
            // 
            this.GPUusageThreshold.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GPUusageThreshold.Location = new System.Drawing.Point(222, 52);
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
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(265, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(21, 18);
            this.label6.TabIndex = 18;
            this.label6.Text = "%";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(219, 28);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 16);
            this.label7.TabIndex = 19;
            this.label7.Text = "しきい値";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.GPUusageThreshold);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.AutoDetect);
            this.groupBox1.Location = new System.Drawing.Point(19, 157);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(293, 86);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ベータ機能";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(377, 10);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(305, 39);
            this.label8.TabIndex = 22;
            this.label8.Text = "電力制限終了時間";
            // 
            // EndTime
            // 
            this.EndTime.CustomFormat = "H:mm";
            this.EndTime.Font = new System.Drawing.Font("HGP創英角ｺﾞｼｯｸUB", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.EndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.EndTime.Location = new System.Drawing.Point(433, 62);
            this.EndTime.Name = "EndTime";
            this.EndTime.ShowUpDown = true;
            this.EndTime.Size = new System.Drawing.Size(218, 71);
            this.EndTime.TabIndex = 21;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(725, 564);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.EndTime);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.LimitStatusText);
            this.Controls.Add(this.GPUTemp);
            this.Controls.Add(this.LoadDefaultLimit);
            this.Controls.Add(this.GpuIndex);
            this.Controls.Add(this.ForceUnlimit);
            this.Controls.Add(this.ForceLimit);
            this.Controls.Add(this.StatusLimit);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.PowerLimitValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BeginTime);
            this.Font = new System.Drawing.Font("Gadugi", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VRChat向け GPU電力制限ツール Ver ";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PowerLimitValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GPUusageThreshold)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker BeginTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown PowerLimitValue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label StatusLimit;
        private System.Windows.Forms.Button ForceLimit;
        private System.Windows.Forms.Button ForceUnlimit;
        private System.Windows.Forms.ComboBox GpuIndex;
        private System.Windows.Forms.Timer GPUreadTimer;
        private System.Windows.Forms.Button LoadDefaultLimit;
        private System.Windows.Forms.CheckBox AutoDetect;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label GPUTemp;
        private System.Windows.Forms.Label LimitStatusText;
        private System.Windows.Forms.NumericUpDown GPUusageThreshold;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DateTimePicker EndTime;
    }
}

