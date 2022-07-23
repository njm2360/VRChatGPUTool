using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using VRCGPUTool.Form;
using VRCGPUTool.Util;

namespace VRCGPUTool
{
    partial class NvidiaSmi 
    {
        public MainForm MainObj;

        internal string[] queryColumns = {
            "name",
            "uuid",
            "power.limit",
            "power.min_limit",
            "power.max_limit",
            "power.default_limit",
            "utilization.gpu",
            "temperature.gpu",
            "power.draw",
            "clocks.gr",
            "clocks.mem",
        };

        public NvidiaSmi(Form.MainForm Main_Obj)
        {
            MainObj = Main_Obj;
        }

        internal void InitializeNvsmiWorker()
        {
            NvsmiWorker = new BackgroundWorker();
            NvsmiWorker.DoWork += new DoWorkEventHandler(NvsmiWorker_DoWork);
            NvsmiWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(NvsmiWorker_RunWorkerCompleted);
        }

        internal BackgroundWorker NvsmiWorker;
        internal string nvidia_smi(string param)
        {
            Console.WriteLine(param);

            ProcessStartInfo nvsmi = new ProcessStartInfo("nvidia-smi.exe");
            nvsmi.WorkingDirectory = @"C:\Windows\system32\";
            nvsmi.Arguments = param;
            nvsmi.Verb = "RunAs";
            nvsmi.CreateNoWindow = true;
            nvsmi.UseShellExecute = false;
            nvsmi.RedirectStandardOutput = true;

            Process p = Process.Start(nvsmi);

            string output = p.StandardOutput.ReadToEnd();

            p.Dispose();

            return output;
        }

        internal void NvsmiWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string query = string.Join(",", queryColumns);

            string output = nvidia_smi(string.Format("--query-gpu={0} --format=csv,noheader,nounits", query));

            e.Result = output;
        }

        private void NvsmiWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                MainObj.gpuStatuses.Clear();
                using (var r = new StringReader(e.Result.ToString()))
                {
                    for (string l = r.ReadLine(); l != null; l = r.ReadLine())
                    {
                        string[] v = l.Split(',');
                        if (v.Length != queryColumns.Length) continue;

                        MainObj.gpuStatuses.Add(new GpuStatus(
                            v[0].Trim(),
                            v[1].Trim(),
                            (int)double.Parse(v[2]),
                            (int)double.Parse(v[3]),
                            (int)double.Parse(v[4]),
                            (int)double.Parse(v[5]),
                            (int)double.Parse(v[6]),
                            (int)double.Parse(v[7]),
                            (int)double.Parse(v[8]),
                            (int)double.Parse(v[9]),
                            (int)double.Parse(v[10])
                        ));
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("予期せぬエラーが発生しました。\nアプリケーションを強制終了します。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }

            if (!MainObj.gpuStatuses.Any())
            {
                MessageBox.Show("NVIDIA GPUがシステムで検出されなくなりました。\n対応GPUが認識されているか確認してください");
                Application.Exit();
            }

            GpuStatus g = MainObj.gpuStatuses.ElementAt(MainObj.GpuIndex.SelectedIndex);
            MainObj.GPUCoreTemp.Text = "GPUコア温度:" + g.CoreTemp.ToString() + "℃";
            MainObj.GPUTotalPower.Text = "GPU全体電力: " + g.PowerDraw.ToString() + "W";
            MainObj.GPUCorePLValue.Text = "GPUコア電力制限: " + g.PLimit.ToString() + "W";
            MainObj.GPUCoreClockValue.Text = "GPUコアクロック: " + g.CoreClock.ToString() + "MHz";
            MainObj.GPUMemoryClockValue.Text = "GPUメモリクロック: " + g.MemoryClock.ToString() + "MHz";

            DateTime datetime_now = DateTime.Now;

            if(MainObj.gpuPlog.rawdata.logdate.Day != datetime_now.Day)
            {
                PowerLogFile plog = new PowerLogFile(MainObj.gpuPlog);
                plog.SaveConfig(MainObj.gpuPlog.rawdata.logdate);
                MainObj.gpuPlog = null;
                MainObj.gpuPlog = new GPUPowerLog();
            }
            
            MainObj.gpuPlog.AddPowerDeltaData(datetime_now.Hour,g.PowerDraw);

            if ((MainObj.PowerLimitValue.Value != g.PLimit) && MainObj.limitstatus && (MainObj.limittime > 2))
            {
                if (datetime_now.Hour == MainObj.BeginTime.Value.Hour && datetime_now.Minute == MainObj.BeginTime.Value.Minute)
                {
                    MainObj.BeginTime.Value = DateTime.Now.AddMinutes(15);
                }
                MainObj.Limit_Action(false, true);
                MessageBox.Show("外部ツールによって電力制限値が変更されたため制限を終了しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        internal void InitGPU()
        {
            string query = string.Join(",", queryColumns);
            string res = nvidia_smi(string.Format("--query-gpu={0} --format=csv,noheader,nounits", query));
            try
            {
                using (var r = new StringReader(res))
                {
                    for (string l = r.ReadLine(); l != null; l = r.ReadLine())
                    {
                        string[] v = l.Split(',');
                        if (v.Length != queryColumns.Length) continue;

                        MainObj.gpuStatuses.Add(new GpuStatus(
                            v[0].Trim(),
                            v[1].Trim(),
                            (int)double.Parse(v[2]),
                            (int)double.Parse(v[3]),
                            (int)double.Parse(v[4]),
                            (int)double.Parse(v[5]),
                            (int)double.Parse(v[6]),
                            (int)double.Parse(v[7]),
                            (int)double.Parse(v[8]),
                            (int)double.Parse(v[9]),
                            (int)double.Parse(v[10])
                        ));
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("このGPUは電力制限に対応していません。\nアプリケーションを終了します。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MainObj.Close();
            }

            if (!MainObj.gpuStatuses.Any())
            {
                MessageBox.Show("NVIDIA GPUがシステムで検出されませんでした。\n対応GPUが搭載されているか確認してください");
                Application.Exit();
            }
        }
    }
}
