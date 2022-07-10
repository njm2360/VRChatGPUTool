using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

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
        DateTime setting_time = DateTime.Now;

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

                    gpuStatuses.Add(new GpuStatus(
                        v[0],
                        v[1],
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
                MessageBox.Show("nvidia-smi.exe is exist, but GPU count is 0.\nExit.");
                Close();
            }
        }

        private static string nvidia_smi(string param)
        {
            Console.WriteLine(param);

            ProcessStartInfo nvsmi = new ProcessStartInfo("nvidia-smi.exe");
            nvsmi.WorkingDirectory = @"C:\Windows\system32\";
            //nvsmi.FileName = @"nvidia-smi.exe";
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
            //nvidia-smiがインストールされていない環境をはじく
            if (!System.IO.File.Exists(@"C:\Windows\system32\nvidia-smi.exe"))
            {
                MessageBox.Show("nvidia-smiが見つかりません。\nNVIDIAグラフィックドライバが正しくインストールされていることを確認してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            //直近のGPU使用率配列初期化
            for(int i = 0; i < recentutil.Length; i++)
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
            datetime_now = DateTime.Now;
            setting_time = datetime_now.AddHours(1);
            //秒を0にする
            DateTime set0s = new DateTime(setting_time.Year,setting_time.Month,setting_time.Day,setting_time.Hour,setting_time.Minute,00);
            BeginTime.Value = setting_time = set0s;
            //現在時刻によって明日か今日か設定
            if(datetime_now.Hour >= 22)
            {
                TomorrowTime.Checked = true;
            }
            else
            {
                TodayTime.Checked = true;
            }
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
            limitstatus = false;
            LimitStatusText.Visible = false;
            //制限時に変更できないように無効化
            PowerLimitValue.Enabled = true;
            BeginTime.Enabled = true;
            TodayTime.Enabled = true;
            TomorrowTime.Enabled = true;
            LoadDefaultLimit.Enabled = true;
            ForceLimit.Enabled = true;

            //設定時間を１時間後にセット（再度制限を防ぐ）
            setting_time = BeginTime.Value = DateTime.Now.AddHours(1);

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
                    if(max_util - min_util < Convert.ToInt16(GPUusageThreshold.Value))
                    {
                        limitstatus = false;
                        LimitStatusText.Visible = false;
                        //制限時に変更できないように無効化
                        PowerLimitValue.Enabled = true;
                        BeginTime.Enabled = true;
                        TodayTime.Enabled = true;
                        TomorrowTime.Enabled = true;
                        LoadDefaultLimit.Enabled = true;
                        ForceLimit.Enabled = true;
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
            Console.WriteLine(datetime_now.ToString() + setting_time.ToString());
            if (datetime_now >= setting_time && limitstatus == false)
            {
                limitstatus = true;
                LimitStatusText.Visible = true;
                //制限時に変更できないように無効化
                PowerLimitValue.Enabled = false;
                BeginTime.Enabled = false;
                TodayTime.Enabled = false;
                TomorrowTime.Enabled = false;
                LoadDefaultLimit.Enabled = false;
                ForceLimit.Enabled = false;

                nvidia_smi("-pl " + PowerLimitValue.Value.ToString() + " --id=" + g.UUID);
                StatusLimit.Text = PowerLimitValue.Value.ToString() + "W";
            }
        }

        private void SettingTimeChanged(object sender, EventArgs e)
        {
            DateTime check_time = BeginTime.Value;
            //明日になっている場合のみ1日足す
            if (TomorrowTime.Checked == true)
            {
                check_time = check_time.AddDays(1);
                Console.WriteLine(check_time.ToString());
            }
            //現在時刻より前に設定していないかチェック
            if(check_time <= DateTime.Now)
            {
                MessageBox.Show("現在時刻より前には設定できません","エラー",MessageBoxButtons.OK,MessageBoxIcon.Error);
                BeginTime.Value = DateTime.Now.AddMinutes(5); //現在時刻+5分後に設定
                check_time = BeginTime.Value;
            }
            //設定時間をセット
            setting_time = check_time;

            Console.WriteLine(setting_time.ToString());
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
