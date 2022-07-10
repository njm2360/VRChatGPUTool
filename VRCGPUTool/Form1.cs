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

namespace VRCGPUTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<GpuStatus> gpuStatuses = new List<GpuStatus>();

        private bool limitstatus = false;
        private int[] recentutil = new int[300];
        private int writeaddr = 0;
        private bool data_ready = false;

        DateTime datetime_now = DateTime.Now;

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
            //アプリケーションのアップデート確認
            var client = new HttpClient();
            var result = client.GetAsync(@"https://api.github.com/repos/njm2360/VRChatGPUTool/releases/latest");

            //var json = result.Content.ReadAsStringAsync();
            //Console.WriteLine($"{(int)result.StatusCode} {result.StatusCode}");
            //Console.WriteLine(json);

            //nvidia-smiがインストールされていない環境をはじく
            if (!System.IO.File.Exists(@"C:\Windows\system32\nvidia-smi.exe"))
            {
                MessageBox.Show("nvidia-smiが見つかりません。\nNVIDIAグラフィックドライバが正しくインストールされていることを確認してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            //NVIDIA GPUがインストールされていない環境をはじく
            nvidia_smi("");

            //直近のGPU使用率配列初期化
            for (int i = 0; i < recentutil.Length; i++)
            {
                recentutil[i] = -1;
            }
            //システムに搭載されているNvidiaGPUの数を取得
            refreshGPUStatus();

            //各GPUのステータスを取得
            foreach (GpuStatus g in gpuStatuses)
            {
                GpuIndex.Items.Add(g.Name);
            }

            GpuIndex.SelectedIndex = 0;

            //電力制限値の範囲を設定
            PowerLimitValue.Value = Convert.ToDecimal(gpuStatuses.First().PLimit);

            StatusLimit.Text = gpuStatuses.First().PLimit.ToString() + "W";

            //時間制限用
            BeginTime.Value = DateTime.Now.AddMinutes(15);
            EndTime.Value = DateTime.Now.AddMinutes(30);
            
            //タイマー有効
            GPUreadTimer.Enabled = true;
        }

        private void LoadDefaultLimit_Click(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            PowerLimitValue.Value = Convert.ToDecimal(g.PLimitDefault);
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

            //設定時間を１時間後にセット（再度制限を防ぐ）
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
    }
}
