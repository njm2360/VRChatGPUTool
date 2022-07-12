using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
using System.Drawing;
using System.Text;

namespace VRCGPUTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
        }

        List<GpuStatus> gpuStatuses = new List<GpuStatus>();

        private bool limitstatus = false;
        private int[] recentutil = new int[300];
        private int writeaddr = 0;
        private bool data_ready = false;

        DateTime datetime_now = DateTime.Now;

        public class Config
        {
            public DateTime BeginTime { get; set; } = DateTime.Now;
            public DateTime EndTime { get; set; } = DateTime.Now;
        }

        private void checkUpdateWorker_DoWork(object sender, DoWorkEventArgs e) {
            Task<string> worker = Task.Run<string>(async () => {
                BackgroundWorker w = sender as BackgroundWorker;

                var client = new HttpClient();

                var message = new HttpRequestMessage {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://api.github.com/repos/njm2360/VRChatGPUTool/releases/latest"),
                };

                message.Headers.UserAgent.Add(new ProductInfoHeaderValue("VRChatGPUTool", "0.0.0.0"));

                var result = await client.SendAsync(message).ConfigureAwait(false);

                result.EnsureSuccessStatusCode();

                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            });

            worker.Wait();

            e.Result = JsonSerializer.Deserialize<GitHubReleaseAPIStructure>(
                worker.Result,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
            );
        }

        private void checkUpdateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null)
            {
                MessageBox.Show(string.Format("アップデートチェック中にエラーが発生しました。\n\n{0}", e.Error.ToString()));
                return;
            }
            string tag_name = ((GitHubReleaseAPIStructure)e.Result).tag_name;

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = "v" + fileVersionInfo.ProductVersion;

            //更新があるかチェック
            if(tag_name != version)
            {
                var res = MessageBox.Show("アップデートがあります\n\n最新バージョンは " + tag_name + " です\n\nアップデートページ(Booth)を開きますか?","アップデート",MessageBoxButtons.OKCancel);
                if(res == DialogResult.OK)
                {
                    Process.Start(new ProcessStartInfo { FileName = "https://njm2360.booth.pm/items/3993173", UseShellExecute = true });
                }
            }
        }

        void refreshGPUStatus()
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
            };

            string query = string.Join(",", queryColumns);
            string output = nvidia_smi(
                string.Format("--query-gpu={0} --format=csv,noheader,nounits", query)
            );

            try
            {
                using (var r = new StringReader(output)) {
                    for (string l = r.ReadLine(); l != null; l = r.ReadLine()) {
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
                            (int)double.Parse(v[7])
                        ));
                    }
                }
            }
            catch (FormatException)
            {
                //現状LapTop GPUは対応しない可能性あり
                MessageBox.Show("このGPUは電力制限に対応していません。\nアプリケーションを終了します。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }

            if (!gpuStatuses.Any()) {
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
            //アイコン設定
            Icon appIcon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            this.Icon = appIcon;

            //アプデチェック
            checkUpdateWorker.RunWorkerAsync();

            //バージョン反映
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            this.Text += fileVersionInfo.ProductVersion;

            //nvidia-smiがインストールされていない環境をはじく
            if (!File.Exists(@"C:\Windows\system32\nvidia-smi.exe"))
            {
                MessageBox.Show("nvidia-smiが見つかりません。\nNVIDIAグラフィックドライバが正しくインストールされていることを確認してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            //直近のGPU使用率配列初期化
            for (int i = 0; i < recentutil.Length; i++)
            {
                recentutil[i] = -1;
            }

            //全GPUステータスを更新
            //NVIDIA GPUがインストールされていない環境をはじく
            refreshGPUStatus();

            //GPUリストを更新
            foreach (GpuStatus g in gpuStatuses)
            {
                GpuIndex.Items.Add(g.Name);
            }

            GpuIndex.SelectedIndex = 0;

            // 電力制限値の範囲を設定
            PowerLimitValue.Value = Convert.ToDecimal(gpuStatuses.First().PLimit);

            StatusLimit.Text = gpuStatuses.First().PLimit.ToString() + "W";

            //時間制限用
            BeginTime.Value = DateTime.Now.AddMinutes(15);
            EndTime.Value = DateTime.Now.AddMinutes(30);

            //設定ファイルがあれば読み込む、なければ生成
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
            
            //タイマー有効
            GPUreadTimer.Enabled = true;
        }

        

        private void ForceLimit_Click(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            nvidia_smi("-pl " + PowerLimitValue.Value.ToString() + " --id=" + g.UUID);
            StatusLimit.Text = PowerLimitValue.Value.ToString() + "W";
        }

        private void ForceUnlimit_Click(object sender, EventArgs e)
        {
            //操作可能にするため表示
            limitstatus = false;
            LimitStatusText.Visible = false;
            PowerLimitValue.Enabled = true;
            BeginTime.Enabled = true;
            LoadDefaultLimit.Enabled = true;
            ForceLimit.Enabled = true;

            //設定時間を15分後にセット（再度制限を防ぐ）
            BeginTime.Value = DateTime.Now.AddMinutes(15);

            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            nvidia_smi("-pl " + g.PLimitDefault.ToString() + " --id=" + g.UUID) ;
            StatusLimit.Text = g.PLimitDefault.ToString() + "W";
        }

        private void SelectGPUChanged(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            PowerLimitValue.Value = Convert.ToDecimal(g.PLimit);
            StatusLimit.Text = g.PLimit + "W";
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
                        limitstatus = true;
                        LimitStatusText.Visible = true;
                        //制限時に変更できないように無効化
                        PowerLimitValue.Enabled = false;
                        BeginTime.Enabled = false;
                        LoadDefaultLimit.Enabled = false;
                        ForceLimit.Enabled = false;
                        //制限をかける
                        nvidia_smi("-pl " + PowerLimitValue.Value.ToString() + " --id=" + g.UUID);
                        StatusLimit.Text = PowerLimitValue.Value.ToString() + "W";
                    }
                }
                
            }

            //GPU温度を表示
            GPUTemp.Text = "GPU温度:" + g.CoreTemp.ToString() + "℃";
            //GPU電力制限
            datetime_now = DateTime.Now;
            if(datetime_now.Hour == BeginTime.Value.Hour && datetime_now.Minute == BeginTime.Value.Minute && !limitstatus)
            {
                limitstatus = true;
                LimitStatusText.Visible = true;
                //制限時に変更できないように無効化
                PowerLimitValue.Enabled = false;
                BeginTime.Enabled = false;
                LoadDefaultLimit.Enabled = false;
                ForceLimit.Enabled = false;

                nvidia_smi("-pl " + PowerLimitValue.Value.ToString() + " --id=" + g.UUID);
                StatusLimit.Text = PowerLimitValue.Value.ToString() + "W";
            }
            if(datetime_now.Hour == EndTime.Value.Hour && datetime_now.Minute == EndTime.Value.Minute && limitstatus)
            {
                //制限解除後は自動検出無効
                AutoDetect.Checked = false;
                //操作可能にするため表示
                limitstatus = false;
                LimitStatusText.Visible = false;
                PowerLimitValue.Enabled = true;
                BeginTime.Enabled = true;
                LoadDefaultLimit.Enabled = true;
                ForceLimit.Enabled = true;

                nvidia_smi("-pl " + g.PLimitDefault.ToString() + " --id=" + g.UUID);
                StatusLimit.Text = g.PLimitDefault.ToString() + "W";
            }
        }

        private void PowerLimitSettingChanged(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            //既定の範囲に収まらない場合ユーザーにメッセージ
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
                Close();
            }
        }
    }
}
