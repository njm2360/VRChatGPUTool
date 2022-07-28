using System;
using System.IO;
using System.Windows.Forms;
using VRCGPUTool.Form;

namespace VRCGPUTool.Util
{
    internal class PowerLogCsv
    {
        MainForm fm;
        PowerHistory historyForm;


        public PowerLogCsv(MainForm mainform, PowerHistory historyForm)
        {
            fm = mainform;
            this.historyForm = historyForm;
        }

        internal void ExportCsvDay(GPUPowerLog gPLog)
        {
            var res = historyForm.saveFileDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                string fName = historyForm.saveFileDialog1.FileName;
                using (FileStream fs = new FileStream(fName, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        DateTime dt = gPLog.rawdata.logdate;

                        sw.WriteLine($"{dt.Year}年{dt.Month}月{dt.Day}日");
                        sw.WriteLine($"時,使用量(Wh)");

                        for (int i = 0; i < 24; i++)
                        {
                            sw.WriteLine($"{i},{gPLog.rawdata.hourPowerLog[i]:f2}");
                        }
                    }
                }
            }
        }

        internal void ExportCsvMonth(DateTime dt, bool isThisMonth)
        {
            var res = historyForm.saveFileDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                string fName = historyForm.saveFileDialog1.FileName;
                using (FileStream fs = new FileStream(fName, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        if (isThisMonth)
                        {
                            for (int i = 1; i < fm.gpuPlog.rawdata.logdate.Day; i++)
                            {
                                DateTime loadDate = new DateTime(fm.gpuPlog.rawdata.logdate.Year, fm.gpuPlog.rawdata.logdate.Month, i);
                                GPUPowerLog recentlog = new GPUPowerLog();
                                PowerLogFile logfile = new PowerLogFile(recentlog);
                                int res2 = logfile.LoadPowerLog(loadDate, true);

                                if (res2 == 0)
                                {
                                    for (int j = 0; j < 24; j++)
                                    {
                                        sw.WriteLine($"{j},{recentlog.rawdata.hourPowerLog[j]}");
                                    }
                                }
                                else
                                {
                                    for (int j = 0; j < 24; j++)
                                    {
                                        sw.WriteLine($"{j},0");
                                    }
                                }
                            }
                            for (int i = 0; i < 24; i++)
                            {
                                sw.WriteLine($"{i},{fm.gpuPlog.rawdata.hourPowerLog[i]}");
                            }
                        }
                        else
                        {
                            int Days = DateTime.DaysInMonth(dt.Year, dt.Month);

                            for (int i = 1; i <= Days; i++)
                            {
                                DateTime loadDate = new DateTime(dt.Year, dt.Month, i);
                                GPUPowerLog recentlog = new GPUPowerLog();
                                PowerLogFile logfile = new PowerLogFile(recentlog);
                                int res2 = logfile.LoadPowerLog(loadDate, true);

                                if (res2 == 0)
                                {
                                    for (int j = 0; j < 24; j++)
                                    {
                                        sw.WriteLine($"{j},{recentlog.rawdata.hourPowerLog[j]}");
                                    }
                                }
                                else
                                {
                                    for (int j = 0; j < 24; j++)
                                    {
                                        sw.WriteLine($"{j},0");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}