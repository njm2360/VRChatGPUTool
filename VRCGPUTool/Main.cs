using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.Json;
using System.Reflection;
using System.Drawing;
using System.Threading.Tasks;

namespace VRCGPUTool
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
            InitializeNvsmiWorker();
        }

        List<GpuStatus> gpuStatuses = new List<GpuStatus>();

        private bool limitstatus = false;
        private int[] recentutil = new int[300];
        private int writeaddr = 0;
        private bool data_ready = false;

        DateTime datetime_now = DateTime.Now;

        private string gitHubUseUrl = "https://github.com/njm2360/VRChatGPUTool#readme";

        public class Config
        {
            public DateTime BeginTime { get; set; } = DateTime.Now;
            public DateTime EndTime { get; set; } = DateTime.Now;
        }
        

        private void refreshGPUStatus()
        {
            gpuStatuses.Clear();

            string[] queryColumns = {
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

            string query = string.Join(",", queryColumns);

            //NvsmiWorker.RunWorkerAsync(string.Format("--query-gpu={0} --format=csv,noheader,nounits", query));

            string output = nvidia_smi(string.Format("--query-gpu={0} --format=csv,noheader,nounits", query)).Result;

            try
            {
                using (var r = new StringReader(output))
                {
                    for (string l = r.ReadLine(); l != null; l = r.ReadLine())
                    {
                        string[] v = l.Split(',');
                        if (v.Length != queryColumns.Length) continue;

                        gpuStatuses.Add(new GpuStatus(
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
                Close();
            }

            if (!gpuStatuses.Any())
            {
                MessageBox.Show("NVIDIA GPUがシステムで検出されませんでした。\n対応GPUが搭載されているか確認してください");
                Application.Exit();
            }

        }
        private static string nvidia_smi(string param)
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

            return output;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //Mutex/SuspendLayout / ResumeLayout

            Icon appIcon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            Icon = appIcon;

            checkUpdateWorker.RunWorkerAsync();

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            this.Text += fileVersionInfo.ProductVersion;

            if (!File.Exists(@"C:\Windows\system32\nvidia-smi.exe"))
            {
                MessageBox.Show("nvidia-smiが見つかりません。\nNVIDIAグラフィックドライバが正しくインストールされていることを確認してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            for (int i = 0; i < recentutil.Length; i++)
            {
                recentutil[i] = -1;
            }

            refreshGPUStatus();

            foreach (GpuStatus g in gpuStatuses)
            {
                GpuIndex.Items.Add(g.Name);
            }

            GpuIndex.SelectedIndex = 0;

            
            PowerLimitValue.Value = Convert.ToDecimal(gpuStatuses.First().PLimit);
            GPUCorePLValue.Text = "GPUコア電力制限: " + gpuStatuses.First().PLimit.ToString() + "W";

            BeginTime.Value = DateTime.Now.AddMinutes(15);
            EndTime.Value = DateTime.Now.AddMinutes(30);

            if (File.Exists("config.json"))
            {
                using (FileStream fs = File.OpenRead("config.json"))
                {
                    using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8))
                    {
                        while (!sr.EndOfStream)
                        {
                            Config config = JsonSerializer.Deserialize<Config>(sr.ReadLine());
                            BeginTime.Value = config.BeginTime;
                            EndTime.Value = config.EndTime;
                        }
                    }
                }
            }
            else
            {
                try
                {
                    Config config = new Config();
                    config.BeginTime = BeginTime.Value;
                    config.EndTime = EndTime.Value;
                    
                    string confjson = JsonSerializer.Serialize(config);

                    using (StreamWriter sw = new StreamWriter("config.json"))
                    {
                        sw.WriteLine(confjson);
                    }
                    var res = MessageBox.Show("この度は「VRChat向け GPU電力制限ツール」\nをダウンロードしていただきありがとうございます。\n\nリリースノートを開きますか?","ようこそ",MessageBoxButtons.YesNo,MessageBoxIcon.Information);
                    if(res == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo { FileName = "https://github.com/njm2360/VRChatGPUTool#readme", UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("設定ファイル作成時にエラーが発生しました\n\n{0}",ex.Message.ToString()), "エラー",MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                }
                
            }
            
            GPUreadTimer.Enabled = true;
        }

        private void Limit_Action(bool limit)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            if (limit == true)
            {
                limitstatus = true;
                LimitStatusText.Visible = true;
                PowerLimitValue.Enabled = false;
                BeginTime.Enabled = false;
                LoadDefaultLimit.Enabled = false;
                LoadMaximumLimit.Enabled = false;
                LoadMinimumLimit.Enabled = false;
                ForceLimit.Enabled = false;
                CoreLimitEnable.Enabled = false;
                CoreClockSetting.Enabled = false;

                if (CoreLimitEnable.Checked == true)
                {
                    //nvidia_smi("-lgc 210," + CoreClockSetting.Value + "--id=" + g.UUID);
                }

                //nvidia_smi("-pl " + PowerLimitValue.Value.ToString() + " --id=" + g.UUID);
                GPUCorePLValue.Text = "GPUコア電力制限: " + PowerLimitValue.Value.ToString() + "W";
            }
            else
            {
                limitstatus = false;
                LimitStatusText.Visible = false;
                PowerLimitValue.Enabled = true;
                BeginTime.Enabled = true;
                LoadDefaultLimit.Enabled = true;
                LoadMaximumLimit.Enabled = true;
                LoadMinimumLimit.Enabled = true;
                ForceLimit.Enabled = true;
                CoreLimitEnable.Enabled = true;
                CoreClockSetting.Enabled = true;

                if (CoreLimitEnable.Checked == true)
                {
                    //nvidia_smi("-rgc --id=" + g.UUID);
                }

                //BeginTime.Value = DateTime.Now.AddMinutes(15);
                
                //nvidia_smi("-pl " + g.PLimitDefault.ToString() + " --id=" + g.UUID);
                GPUCorePLValue.Text = "GPUコア電力制限: " + g.PLimitDefault.ToString() + "W";
            }
        }

        private void ForceLimit_Click(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            //nvidia_smi("-pl " + PowerLimitValue.Value.ToString() + " --id=" + g.UUID);
            GPUCorePLValue.Text = "GPUコア電力制限: " + PowerLimitValue.Value.ToString() + "W";
        }

        private void ForceUnlimit_Click(object sender, EventArgs e)
        {
            Limit_Action(false);
        }

        private void SelectGPUChanged(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            PowerLimitValue.Value = Convert.ToDecimal(g.PLimit);
            GPUCorePLValue.Text = "GPUコア電力制限: " + g.PLimit + "W";
        }

        private void GPUreadTimer_Tick(object sender, EventArgs e)
        {
            //GPUのステータスを更新
            refreshGPUStatus();
            //自動検出（ベータ）

            //パラメータ定義
            const int AVE_DELTA = 20;
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);

            if (AutoDetect.Checked == true)
            {
                recentutil[writeaddr] = g.CoreLoad;
                writeaddr++;
                if(writeaddr >= 300)
                {
                    writeaddr = 0;
                    data_ready = true;
                }
                //5分間のデータを取得完了
                if(data_ready == true)
                {
                    int max_util = 0;
                    int min_util = 100;
                    int[] ave_util = new int[(recentutil.Length / AVE_DELTA)];

                    //指定微小時間当たりの移動平均を求めて最大最小を求める
                    for(int i = 0;i < ave_util.Length; i++)
                    {
                        for (int s = 0; s < AVE_DELTA; s++)
                        {
                            ave_util[i] += recentutil[AVE_DELTA * i + s];
                        }
                        ave_util[i] /= AVE_DELTA;
                        if(ave_util[i] > max_util)
                        {
                            max_util = ave_util[i];
                        }
                        if (ave_util[i] < min_util)
                        {
                            min_util = ave_util[i];
                        }
                    }

                    //使用率が指定の範囲の場合寝落ちと判断
                    if(max_util - min_util < Convert.ToInt16(GPUusageThreshold.Value) && !limitstatus)
                    {
                        Limit_Action(true);
                    }
                }
                
            }

            //GPUステータス表示
            GPUCoreTemp.Text = "GPU温度:" + g.CoreTemp.ToString() + "℃";
            GPUTotalPower.Text = "GPU全体電力: " + g.PowerDraw.ToString() + "W";
            GPUCoreClockValue.Text = "GPUコアクロック: " + g.CoreClock.ToString() + "MHz";
            GPUMemoryClockValue.Text = "GPUメモリクロック: " + g.MemoryClock.ToString() + "MHz";

            //GPU電力制限
            datetime_now = DateTime.Now;
            if(datetime_now.Hour == BeginTime.Value.Hour && datetime_now.Minute == BeginTime.Value.Minute && !limitstatus)
            {
                Limit_Action(true);
            }
            if(datetime_now.Hour == EndTime.Value.Hour && datetime_now.Minute == EndTime.Value.Minute && limitstatus)
            {
                AutoDetect.Checked = false;

                Limit_Action(false);
            }
        }

        private void PowerLimitSettingChanged(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            if(PowerLimitValue.Value > g.PLimitMax)
            {
                MessageBox.Show("電力制限値が設定可能な範囲外です。\n" + g.Name + "の最大電力制限は" + g.PLimitMax + "Wです。", "エラー",MessageBoxButtons.OK, MessageBoxIcon.Error);
                PowerLimitValue.Value = g.PLimitMax;
            }
            if(PowerLimitValue.Value < g.PLimitMin)
            {
                MessageBox.Show("電力制限値が設定可能な範囲外です。\n" + g.Name + "の最小電力制限は" + g.PLimitMin + "Wです。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PowerLimitValue.Value = g.PLimitMin;
            }
        }

        private void LoadMinimumLimit_Click(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            PowerLimitValue.Value = Convert.ToDecimal(g.PLimitMin);
        }

        private void LoadDefaultLimit_Click(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            PowerLimitValue.Value = Convert.ToDecimal(g.PLimitDefault);
        }

        private void LoadMaximumLimit_Click(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            PowerLimitValue.Value = Convert.ToDecimal(g.PLimitMax);
        }

        private void AppClosing(object sender, FormClosingEventArgs e)
        {
            if(limitstatus == true)
            {
                MessageBox.Show("アプリを終了する前に制限を解除してください","エラー",MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
                return;
            }
            try
            {
                Config config = new Config();
                config.BeginTime = BeginTime.Value;
                config.EndTime = EndTime.Value;

                string confjson = JsonSerializer.Serialize(config);

                using (StreamWriter sw = new StreamWriter("config.json"))
                {
                    sw.WriteLine(confjson);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("設定ファイル更新時にエラーが発生しました\n\n{0}", ex.Message.ToString()), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private void ResetClockSetting_Click(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            //nvidia_smi("-rgc --id=" + g.UUID);
            MessageBox.Show("クロック制限をデフォルト値に設定しました", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowHowToUse(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = gitHubUseUrl, UseShellExecute = true });
        }

        private void Reporter(object sender, EventArgs e)
        {
            Form report = new BugReport(Convert.ToInt32(((Button)sender).Tag));
            
            report.ShowDialog();
        }
    }
}
