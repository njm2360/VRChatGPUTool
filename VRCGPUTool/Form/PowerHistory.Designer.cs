﻿namespace VRCGPUTool.Form
{
    partial class PowerHistory
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title2 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PowerHistory));
            this.UsageGraphDay = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.TabRange = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.DaylyTotalPower = new System.Windows.Forms.Label();
            this.DataRefreshDate = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.DataRefresh = new System.Windows.Forms.Button();
            this.NextDayData = new System.Windows.Forms.Button();
            this.PreviousDayData = new System.Windows.Forms.Button();
            this.LogDateLabel = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.MonthlyTotalPower = new System.Windows.Forms.Label();
            this.DataRefreshDate2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.NextMonthData = new System.Windows.Forms.Button();
            this.PreviousMonthData = new System.Windows.Forms.Button();
            this.LogMonthLabel = new System.Windows.Forms.Label();
            this.UsageGraphMonth = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.PowerPlanSetting = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.priceDay = new System.Windows.Forms.Label();
            this.priceMonth = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.UsageGraphDay)).BeginInit();
            this.TabRange.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UsageGraphMonth)).BeginInit();
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
            this.TabRange.SelectedIndexChanged += new System.EventHandler(this.TabChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.priceDay);
            this.tabPage1.Controls.Add(this.DaylyTotalPower);
            this.tabPage1.Controls.Add(this.DataRefreshDate);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.DataRefresh);
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
            // DaylyTotalPower
            // 
            this.DaylyTotalPower.AutoSize = true;
            this.DaylyTotalPower.Font = new System.Drawing.Font("Yu Gothic UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DaylyTotalPower.Location = new System.Drawing.Point(290, 7);
            this.DaylyTotalPower.Name = "DaylyTotalPower";
            this.DaylyTotalPower.Size = new System.Drawing.Size(132, 30);
            this.DaylyTotalPower.TabIndex = 10;
            this.DaylyTotalPower.Text = "合計:0.0kWh";
            // 
            // DataRefreshDate
            // 
            this.DataRefreshDate.AutoSize = true;
            this.DataRefreshDate.Location = new System.Drawing.Point(689, 16);
            this.DataRefreshDate.Name = "DataRefreshDate";
            this.DataRefreshDate.Size = new System.Drawing.Size(36, 13);
            this.DataRefreshDate.TabIndex = 6;
            this.DataRefreshDate.Text = "(Date)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(597, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "データ取得日時:";
            // 
            // DataRefresh
            // 
            this.DataRefresh.Location = new System.Drawing.Point(692, 431);
            this.DataRefresh.Name = "DataRefresh";
            this.DataRefresh.Size = new System.Drawing.Size(106, 25);
            this.DataRefresh.TabIndex = 4;
            this.DataRefresh.Text = "最新データに更新";
            this.DataRefresh.UseVisualStyleBackColor = true;
            this.DataRefresh.Click += new System.EventHandler(this.DataRefresh_Click);
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
            this.LogDateLabel.Font = new System.Drawing.Font("游ゴシック", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.LogDateLabel.Location = new System.Drawing.Point(16, 12);
            this.LogDateLabel.Name = "LogDateLabel";
            this.LogDateLabel.Size = new System.Drawing.Size(268, 25);
            this.LogDateLabel.TabIndex = 1;
            this.LogDateLabel.Text = "2020年1月1日の電力使用履歴";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.priceMonth);
            this.tabPage2.Controls.Add(this.MonthlyTotalPower);
            this.tabPage2.Controls.Add(this.DataRefreshDate2);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.NextMonthData);
            this.tabPage2.Controls.Add(this.PreviousMonthData);
            this.tabPage2.Controls.Add(this.LogMonthLabel);
            this.tabPage2.Controls.Add(this.UsageGraphMonth);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(818, 469);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "電力使用量【1カ月】";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // MonthlyTotalPower
            // 
            this.MonthlyTotalPower.AutoSize = true;
            this.MonthlyTotalPower.Font = new System.Drawing.Font("Yu Gothic UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MonthlyTotalPower.Location = new System.Drawing.Point(260, 7);
            this.MonthlyTotalPower.Name = "MonthlyTotalPower";
            this.MonthlyTotalPower.Size = new System.Drawing.Size(132, 30);
            this.MonthlyTotalPower.TabIndex = 9;
            this.MonthlyTotalPower.Text = "合計:0.0kWh";
            // 
            // DataRefreshDate2
            // 
            this.DataRefreshDate2.AutoSize = true;
            this.DataRefreshDate2.Location = new System.Drawing.Point(668, 16);
            this.DataRefreshDate2.Name = "DataRefreshDate2";
            this.DataRefreshDate2.Size = new System.Drawing.Size(36, 13);
            this.DataRefreshDate2.TabIndex = 8;
            this.DataRefreshDate2.Text = "(Date)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(576, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "データ取得日時:";
            // 
            // NextMonthData
            // 
            this.NextMonthData.Location = new System.Drawing.Point(388, 431);
            this.NextMonthData.Name = "NextMonthData";
            this.NextMonthData.Size = new System.Drawing.Size(155, 25);
            this.NextMonthData.TabIndex = 5;
            this.NextMonthData.Text = "翌月＞＞";
            this.NextMonthData.UseVisualStyleBackColor = true;
            this.NextMonthData.Click += new System.EventHandler(this.NextMonthData_Click);
            // 
            // PreviousMonthData
            // 
            this.PreviousMonthData.Location = new System.Drawing.Point(155, 431);
            this.PreviousMonthData.Name = "PreviousMonthData";
            this.PreviousMonthData.Size = new System.Drawing.Size(155, 25);
            this.PreviousMonthData.TabIndex = 4;
            this.PreviousMonthData.Text = "＜＜先月";
            this.PreviousMonthData.UseVisualStyleBackColor = true;
            this.PreviousMonthData.Click += new System.EventHandler(this.PreviousMonthData_Click);
            // 
            // LogMonthLabel
            // 
            this.LogMonthLabel.AutoSize = true;
            this.LogMonthLabel.Font = new System.Drawing.Font("游ゴシック", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.LogMonthLabel.Location = new System.Drawing.Point(16, 12);
            this.LogMonthLabel.Name = "LogMonthLabel";
            this.LogMonthLabel.Size = new System.Drawing.Size(238, 25);
            this.LogMonthLabel.TabIndex = 2;
            this.LogMonthLabel.Text = "2020年1月の電力使用履歴";
            // 
            // UsageGraphMonth
            // 
            chartArea2.AxisX.MajorGrid.Interval = 1D;
            chartArea2.AxisX.MajorTickMark.Interval = 1D;
            chartArea2.Name = "ChartArea1";
            this.UsageGraphMonth.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.UsageGraphMonth.Legends.Add(legend2);
            this.UsageGraphMonth.Location = new System.Drawing.Point(6, 40);
            this.UsageGraphMonth.Name = "UsageGraphMonth";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.UsageGraphMonth.Series.Add(series2);
            this.UsageGraphMonth.Size = new System.Drawing.Size(782, 377);
            this.UsageGraphMonth.TabIndex = 1;
            this.UsageGraphMonth.Text = "chart1";
            title2.Name = "Title1";
            title2.Text = "電力使用量(１カ月)";
            this.UsageGraphMonth.Titles.Add(title2);
            // 
            // PowerPlanSetting
            // 
            this.PowerPlanSetting.Location = new System.Drawing.Point(708, 513);
            this.PowerPlanSetting.Name = "PowerPlanSetting";
            this.PowerPlanSetting.Size = new System.Drawing.Size(106, 25);
            this.PowerPlanSetting.TabIndex = 11;
            this.PowerPlanSetting.Text = "電気代設定";
            this.PowerPlanSetting.UseVisualStyleBackColor = true;
            this.PowerPlanSetting.Click += new System.EventHandler(this.PowerPlanSetting_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(572, 513);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 25);
            this.button1.TabIndex = 12;
            this.button1.Text = "CSVエクスポート";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.SaveAction);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "CSV(カンマ区切り)|*.csv";
            // 
            // priceDay
            // 
            this.priceDay.AutoSize = true;
            this.priceDay.Font = new System.Drawing.Font("Yu Gothic UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.priceDay.Location = new System.Drawing.Point(437, 7);
            this.priceDay.Name = "priceDay";
            this.priceDay.Size = new System.Drawing.Size(131, 30);
            this.priceDay.TabIndex = 11;
            this.priceDay.Text = "電気代:0.0円";
            // 
            // priceMonth
            // 
            this.priceMonth.AutoSize = true;
            this.priceMonth.Font = new System.Drawing.Font("Yu Gothic UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.priceMonth.Location = new System.Drawing.Point(412, 7);
            this.priceMonth.Name = "priceMonth";
            this.priceMonth.Size = new System.Drawing.Size(131, 30);
            this.priceMonth.TabIndex = 12;
            this.priceMonth.Text = "電気代:0.0円";
            // 
            // PowerHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 549);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.PowerPlanSetting);
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
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UsageGraphMonth)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.DataVisualization.Charting.Chart UsageGraphDay;
        private System.Windows.Forms.TabControl TabRange;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button NextDayData;
        private System.Windows.Forms.Button PreviousDayData;
        private System.Windows.Forms.Label LogDateLabel;
        private System.Windows.Forms.Button DataRefresh;
        private System.Windows.Forms.Label DataRefreshDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label DataRefreshDate2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button NextMonthData;
        private System.Windows.Forms.Button PreviousMonthData;
        private System.Windows.Forms.Label LogMonthLabel;
        private System.Windows.Forms.DataVisualization.Charting.Chart UsageGraphMonth;
        private System.Windows.Forms.Label DaylyTotalPower;
        private System.Windows.Forms.Label MonthlyTotalPower;
        private System.Windows.Forms.Button PowerPlanSetting;
        private System.Windows.Forms.Button button1;
        internal System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label priceDay;
        private System.Windows.Forms.Label priceMonth;
    }
}