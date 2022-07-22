using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace VRCGPUTool.Form
{
    public partial class PowerHistory : System.Windows.Forms.Form
    {
        public PowerHistory(GPUPowerLog plogData)
        {
            InitializeComponent();
        }

        private void PowerHistory_Load(object sender, EventArgs e)
        {
            UsageGraph24.Series.Clear();
            UsageGraph24.ChartAreas.Clear();
            UsageGraph24.Titles.Clear();

            Random rdm = new Random();
            Series seriesLine = new Series("chartArea");

            Series seriesColumn = new Series();
            seriesColumn.ChartType = SeriesChartType.Column;
            for (int i = 0; i < 24; i++)
            {
                seriesColumn.Points.Add(new DataPoint(i, rdm.Next(0, 500)));
            }

            ChartArea area = new ChartArea("area");
            area.AxisX.Title = "時間(h)";
            area.AxisY.Title = "電力量(Wh)";
            area.AxisX.LabelStyle.Interval = 1;
            area.AxisX.IsMarginVisible = true;

            UsageGraph24.ChartAreas.Add(area);
            UsageGraph24.Series.Add(seriesColumn);
            UsageGraph24.ChartAreas["area"].AxisX.Minimum = 0;
            UsageGraph24.ChartAreas["area"].AxisX.Maximum = 23;
        }

        private void DrawHistoryDay()
        {

        }

        public void DrawHistoryMonth()
        {

        }
    }
}

/*
 *using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCGPUTool
{
    internal class PowerHistory
    {
        private int powerUsage = 0;
        private int unitPrice = 21;

        internal void AddPowerUsage(int value)
        {
            powerUsage += value;
        }
    }
}
*/