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
            this.UsageGraph24 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.TabRange = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.UsageGraph24)).BeginInit();
            this.TabRange.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // UsageGraph24
            // 
            chartArea1.AxisX.MajorGrid.Interval = 1D;
            chartArea1.AxisX.MajorTickMark.Interval = 1D;
            chartArea1.Name = "ChartArea1";
            this.UsageGraph24.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.UsageGraph24.Legends.Add(legend1);
            this.UsageGraph24.Location = new System.Drawing.Point(16, 20);
            this.UsageGraph24.Name = "UsageGraph24";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.UsageGraph24.Series.Add(series1);
            this.UsageGraph24.Size = new System.Drawing.Size(782, 377);
            this.UsageGraph24.TabIndex = 0;
            this.UsageGraph24.Text = "chart1";
            title1.Name = "Title1";
            title1.Text = "電力使用量(24時間)";
            this.UsageGraph24.Titles.Add(title1);
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
            this.tabPage1.Controls.Add(this.UsageGraph24);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(818, 469);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "電力使用量【24時間】";
            this.tabPage1.UseVisualStyleBackColor = true;
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
            ((System.ComponentModel.ISupportInitialize)(this.UsageGraph24)).EndInit();
            this.TabRange.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart UsageGraph24;
        private System.Windows.Forms.TabControl TabRange;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
    }
}