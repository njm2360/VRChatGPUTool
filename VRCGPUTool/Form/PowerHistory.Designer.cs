namespace VRCGPUTool.Form
{
    partial class PowerHistory
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PowerHistory));
            this.UsageGraphDay = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.TabRange = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.NextDayData = new System.Windows.Forms.Button();
            this.PreviousDayData = new System.Windows.Forms.Button();
            this.LogDateLabel = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.DateRefresh = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.DataRefreshDate = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.UsageGraphDay)).BeginInit();
            this.TabRange.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // UsageGraphDay
            // 
            chartArea1.AxisX.MajorGrid.Interval = 1D;
            chartArea1.AxisX.MajorTickMark.Interval = 1D;
            chartArea1.Name = "ChartArea1";
            this.UsageGraphDay.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.UsageGraphDay.Legends.Add(legend1);
            this.UsageGraphDay.Location = new System.Drawing.Point(6, 40);
            this.UsageGraphDay.Name = "UsageGraphDay";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.UsageGraphDay.Series.Add(series1);
            this.UsageGraphDay.Size = new System.Drawing.Size(782, 377);
            this.UsageGraphDay.TabIndex = 0;
            this.UsageGraphDay.Text = "chart1";
            title1.Name = "Title1";
            title1.Text = "電力使用量(24時間)";
            this.UsageGraphDay.Titles.Add(title1);
            // 
            // TabRange
            // 
            this.TabRange.Controls.Add(this.tabPage1);
            this.TabRange.Controls.Add(this.tabPage2);
            this.TabRange.Location = new System.Drawing.Point(12, 12);
            this.TabRange.Name = "TabRange";
            this.TabRange.SelectedIndex = 0;
            this.TabRange.Size = new System.Drawing.Size(826, 495);
            this.TabRange.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.DataRefreshDate);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.DateRefresh);
            this.tabPage1.Controls.Add(this.NextDayData);
            this.tabPage1.Controls.Add(this.PreviousDayData);
            this.tabPage1.Controls.Add(this.LogDateLabel);
            this.tabPage1.Controls.Add(this.UsageGraphDay);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(818, 469);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "電力使用量【24時間】";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // NextDayData
            // 
            this.NextDayData.Location = new System.Drawing.Point(388, 431);
            this.NextDayData.Name = "NextDayData";
            this.NextDayData.Size = new System.Drawing.Size(155, 25);
            this.NextDayData.TabIndex = 3;
            this.NextDayData.Text = "翌日＞＞";
            this.NextDayData.UseVisualStyleBackColor = true;
            this.NextDayData.Click += new System.EventHandler(this.NextDayData_Click);
            // 
            // PreviousDayData
            // 
            this.PreviousDayData.Location = new System.Drawing.Point(155, 431);
            this.PreviousDayData.Name = "PreviousDayData";
            this.PreviousDayData.Size = new System.Drawing.Size(155, 25);
            this.PreviousDayData.TabIndex = 2;
            this.PreviousDayData.Text = "＜＜前日";
            this.PreviousDayData.UseVisualStyleBackColor = true;
            this.PreviousDayData.Click += new System.EventHandler(this.PreviousDayData_Click);
            // 
            // LogDateLabel
            // 
            this.LogDateLabel.AutoSize = true;
            this.LogDateLabel.Font = new System.Drawing.Font("游ゴシック", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LogDateLabel.Location = new System.Drawing.Point(16, 12);
            this.LogDateLabel.Name = "LogDateLabel";
            this.LogDateLabel.Size = new System.Drawing.Size(294, 27);
            this.LogDateLabel.TabIndex = 1;
            this.LogDateLabel.Text = "2020年1月1日の電力使用履歴";
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(818, 469);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "電力使用量【直近1カ月】";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // DateRefresh
            // 
            this.DateRefresh.Location = new System.Drawing.Point(692, 431);
            this.DateRefresh.Name = "DateRefresh";
            this.DateRefresh.Size = new System.Drawing.Size(106, 25);
            this.DateRefresh.TabIndex = 4;
            this.DateRefresh.Text = "最新データに更新";
            this.DateRefresh.UseVisualStyleBackColor = true;
            this.DateRefresh.Click += new System.EventHandler(this.DateRefresh_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(490, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "データ取得日時:";
            // 
            // DataRefreshDate
            // 
            this.DataRefreshDate.AutoSize = true;
            this.DataRefreshDate.Location = new System.Drawing.Point(582, 16);
            this.DataRefreshDate.Name = "DataRefreshDate";
            this.DataRefreshDate.Size = new System.Drawing.Size(36, 13);
            this.DataRefreshDate.TabIndex = 6;
            this.DataRefreshDate.Text = "(Date)";
            // 
            // PowerHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 519);
            this.Controls.Add(this.TabRange);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PowerHistory";
            this.Text = "電力使用履歴";
            this.Load += new System.EventHandler(this.PowerHistory_Load);
            ((System.ComponentModel.ISupportInitialize)(this.UsageGraphDay)).EndInit();
            this.TabRange.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart UsageGraphDay;
        private System.Windows.Forms.TabControl TabRange;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button NextDayData;
        private System.Windows.Forms.Button PreviousDayData;
        private System.Windows.Forms.Label LogDateLabel;
        private System.Windows.Forms.Button DateRefresh;
        private System.Windows.Forms.Label DataRefreshDate;
        private System.Windows.Forms.Label label1;
    }
}