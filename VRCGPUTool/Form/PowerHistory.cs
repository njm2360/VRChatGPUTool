using System;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using VRCGPUTool.Util;

namespace VRCGPUTool.Form
{
    public partial class PowerHistory : System.Windows.Forms.Form
    {
        public MainForm MainObj;

        public PowerHistory(Form.MainForm fm)
        {
            MainObj = fm;
            InitializeComponent();
            PlogDara = MainObj.gpuPlog;
            dispDate = DateTime.Today;
        }

        GPUPowerLog PlogDara;

        private DateTime dispDate;

        private void PowerHistory_Load(object sender, EventArgs e)
        {
            DrawHistoryDay(PlogDara);
            DrawHistoryMonth(PlogDara);
        }

        private void DrawHistoryDay(GPUPowerLog dispdata)
        {
            DateTime dt = DateTime.Now;
            DataRefreshDate.Text = dt.ToString();

            string datelabel = string.Format("{0:D4}年{1}月{2}日の電力使用履歴",dispdata.rawdata.logdate.Year, dispdata.rawdata.logdate.Month, dispdata.rawdata.logdate.Day);

            LogDateLabel.Text = datelabel;

            UsageGraphDay.Series.Clear();
            UsageGraphDay.ChartAreas.Clear();
            UsageGraphDay.Titles.Clear();

            Series seriesLine = new Series("chartArea");

            Series seriesColumn = new Series
            {
                ChartType = SeriesChartType.Column
            };

            for (int i = 0; i < 24; i++)
            {
                seriesColumn.Points.Add(new DataPoint(i, (double)dispdata.rawdata.hourPowerLog[i] / 3600.0));
            }

            ChartArea area = new ChartArea("area");
            area.AxisX.Title = "時間(h)";
            area.AxisY.Title = "電力量(Wh)";
            area.AxisX.LabelStyle.Interval = 1;
            area.AxisX.IsMarginVisible = true;

            UsageGraphDay.ChartAreas.Add(area);
            UsageGraphDay.Series.Add(seriesColumn);
            UsageGraphDay.ChartAreas["area"].AxisX.Minimum = 0;
            UsageGraphDay.ChartAreas["area"].AxisX.Maximum = 23;
        }

        private void PreviousDayData_Click(object sender, EventArgs e)
        {
            GPUPowerLog plog = new GPUPowerLog();
            PowerLogFile plogfile = new PowerLogFile(plog);

            dispDate = dispDate.AddDays(-1);

            var res = plogfile.LoadPowerLog(dispDate,true);
            if(res == false)
            {
                MessageBox.Show("これ以上ログがないため遡れません","エラー");
                dispDate = dispDate.AddDays(1);
            }
            else
            {
                DrawHistoryDay(plog);
            }
        }

        private void NextDayData_Click(object sender, EventArgs e)
        {
            GPUPowerLog plog;
            PowerLogFile plogfile;

            dispDate = dispDate.AddDays(1);
            if (DateTime.Now.Date == dispDate)
            {
                PlogDara = MainObj.gpuPlog;
                plog = PlogDara;
                DrawHistoryDay(plog);
            }
            else
            {
                plog = new GPUPowerLog();
                plogfile = new PowerLogFile(plog);
                var res = plogfile.LoadPowerLog(dispDate, true);
                if (res == false)
                {
                    MessageBox.Show("これ以上ログがないため遡れません", "エラー");
                    dispDate = dispDate.AddDays(-1);
                }
                else
                {
                    DrawHistoryDay(plog);
                }
            }
        }

        private void DataRefresh_Click(object sender, EventArgs e)
        {
            GPUPowerLog plog;
            if (DateTime.Now.Date == dispDate)
            {
                PlogDara = MainObj.gpuPlog;
                plog = PlogDara;
                DrawHistoryDay(plog);
            }
        }

        private void DrawHistoryMonth(GPUPowerLog dat)
        {
            //こっちは当月用(ParamでGPUPowerLogを渡す)
            //過去のデータはdipsdataの日付から1日までのデータを拾う
            //ファイルを逐次Readすればよさそう
            //GPUPowerLogにReadメソッドあるか可能

            DateTime dt = DateTime.Now;
            DataRefreshDate.Text = dt.ToString();

            string datelabel = string.Format("{0:D4}年{1}月の電力使用履歴", dat.rawdata.logdate.Year, dat.rawdata.logdate.Month);

            LogDateLabel.Text = datelabel;

            UsageGraphMonth.Series.Clear();
            UsageGraphMonth.ChartAreas.Clear();
            UsageGraphMonth.Titles.Clear();

            Series seriesLine = new Series("chartArea");

            Series seriesColumn = new Series
            {
                ChartType = SeriesChartType.Column
            };

            //当日のデータを集計してグラフに追加
            int TodayUsage = 0;

            for (int i = 0; i < 24; i++)
            {
                TodayUsage += dat.rawdata.hourPowerLog[i];
            }
            seriesColumn.Points.Add(new DataPoint(dat.rawdata.logdate.Day,(double)TodayUsage / 3600.0));

            //過去のデータ読み出し（当月1日まで）
            int Days = DateTime.DaysInMonth(dat.rawdata.logdate.Year, dat.rawdata.logdate.Month);

            for (int i = 1; i < Days; i++)
            {
                DateTime loadDate = new DateTime(dat.rawdata.logdate.Year, dat.rawdata.logdate.Month,i);
                GPUPowerLog recentlog = new GPUPowerLog();
                PowerLogFile logfile = new PowerLogFile(recentlog);
                logfile.LoadPowerLog(loadDate,true);

                TodayUsage = 0;
                for (int j = 0; j < 24; j++)
                {
                    TodayUsage += recentlog.rawdata.hourPowerLog[i];
                }

                seriesColumn.Points.Add(new DataPoint(i, (double)TodayUsage / 3600.0));
            }

            ChartArea area = new ChartArea("area");
            area.AxisX.Title = "日(Day)";
            area.AxisY.Title = "電力量(Wh)";
            area.AxisX.LabelStyle.Interval = 1;
            area.AxisX.IsMarginVisible = true;

            UsageGraphMonth.ChartAreas.Add(area);
            UsageGraphMonth.Series.Add(seriesColumn);
            UsageGraphMonth.ChartAreas["area"].AxisX.Minimum = 1;
            UsageGraphMonth.ChartAreas["area"].AxisX.Maximum = Days;
        }

        private void PreviousMonthData_Click(object sender, EventArgs e)
        {

        }

        private void NextMonthData_Click(object sender, EventArgs e)
        {

        }
    }
}