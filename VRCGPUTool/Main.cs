using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.Json;
using System.Reflection;
using System.Drawing;

namespace VRCGPUTool
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
            nvsmi = new NvidiaSmi(this);
            nvsmi.InitializeNvsmiWorker();
        }

        NvidiaSmi nvsmi;

        internal List<GpuStatus> gpuStatuses = new List<GpuStatus>();

        internal bool limitstatus = false;
        internal long limittime = 0;
        private int[] recentutil = new int[300];
        private int writeaddr = 0;
        private bool data_ready = false;

        DateTime datetime_now = DateTime.Now;

        private string gitHubUseUrl = "https://github.com/njm2360/VRChatGPUTool#readme";

        

        private void Form1_Load(object sender, EventArgs e)
        {
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

            string query = string.Join(",", nvsmi.queryColumns);
            string res = nvsmi.nvidia_smi(string.Format("--query-gpu={0} --format=csv,noheader,nounits", query));
            try
            {
                using (var r = new StringReader(res))
                {
                    for (string l = r.ReadLine(); l != null; l = r.ReadLine())
                    {
                        string[] v = l.Split(',');
                        if (v.Length != nvsmi.queryColumns.Length) continue;

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

            foreach (GpuStatus g in gpuStatuses)
            {
                GpuIndex.Items.Add(g.Name);
            }

            GpuIndex.SelectedIndex = 0;

            SpecificPLValue.Value = Convert.ToDecimal(gpuStatuses.First().PLimit);
            PowerLimitValue.Value = Convert.ToDecimal(gpuStatuses.First().PLimit);
            GPUCorePLValue.Text = "GPUコア電力制限: " + gpuStatuses.First().PLimit.ToString() + "W";

            BeginTime.Value = DateTime.Now.AddMinutes(15);
            EndTime.Value = DateTime.Now.AddMinutes(30);

            ConfigJson config = new ConfigJson(this);
            config.LoadConfig();
            
            GPUreadTimer.Enabled = true;
        }

        internal void Limit_Action(bool limit,bool expection)
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
                    nvsmi.nvidia_smi("-lgc 210," + CoreClockSetting.Value + "--id=" + g.UUID);
                }

                nvsmi.nvidia_smi("-pl " + PowerLimitValue.Value.ToString() + " --id=" + g.UUID);
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
                    nvsmi.nvidia_smi("-rgc --id=" + g.UUID);
                }

                //BeginTime.Value = DateTime.Now.AddMinutes(15);

                if(expection == false)
                {
                    if(ResetGPUDefaultPL.Checked == true)
                    {
                        nvsmi.nvidia_smi("-pl " + g.PLimitDefault.ToString() + " --id=" + g.UUID);
                        GPUCorePLValue.Text = "GPUコア電力制限: " + g.PLimitDefault.ToString() + "W";
                    }
                    else
                    {
                        nvsmi.nvidia_smi("-pl " + SpecificPLValue.Value.ToString() + " --id=" + g.UUID);
                        GPUCorePLValue.Text = "GPUコア電力制限: " + SpecificPLValue.Value.ToString() + "W";
                    }
                }
                else
                {
                    //GPUCorePLValue.Text = "GPUコア電力制限: " + g.PLimit + "W";
                }
                
            }
        }

        private void ForceLimit_Click(object sender, EventArgs e)
        {
            Limit_Action(true, false);
        }

        private void ForceUnlimit_Click(object sender, EventArgs e)
        {
            if (datetime_now.Hour == BeginTime.Value.Hour && datetime_now.Minute == BeginTime.Value.Minute)
            {
                BeginTime.Value = DateTime.Now.AddMinutes(10);
                MessageBox.Show("開始時間と被っているため開始時間を変更しました","情報",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            Limit_Action(false,false);
        }

        private void SelectGPUChanged(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            PowerLimitValue.Value = Convert.ToDecimal(g.PLimit);
            GPUCorePLValue.Text = "GPUコア電力制限: " + g.PLimit + "W";
        }

        private void GPUreadTimer_Tick(object sender, EventArgs e)
        {
            nvsmi.NvsmiWorker.RunWorkerAsync();

            if (limitstatus)
            {
                limittime++;
            }
            else
            {
                limittime = 0;
            }

            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);

            const int AVE_DELTA = 20;

            if (AutoDetect.Checked == true)
            {
                recentutil[writeaddr] = g.CoreLoad;
                writeaddr++;
                if(writeaddr >= 300)
                {
                    writeaddr = 0;
                    data_ready = true;
                }
                if(data_ready == true)
                {
                    int max_util = 0;
                    int min_util = 100;
                    int[] ave_util = new int[(recentutil.Length / AVE_DELTA)];

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

                    if(max_util - min_util < Convert.ToInt16(GPUusageThreshold.Value) && !limitstatus)
                    {
                        Limit_Action(true,false);
                    }
                }
                
            }

            datetime_now = DateTime.Now;
            if(datetime_now.Hour == BeginTime.Value.Hour && datetime_now.Minute == BeginTime.Value.Minute && !limitstatus)
            {
                Limit_Action(true, false);
            }
            if(datetime_now.Hour == EndTime.Value.Hour && datetime_now.Minute == EndTime.Value.Minute && limitstatus)
            {
                AutoDetect.Checked = false;

                Limit_Action(false, false);
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
        private void SpecificPLValue_ValueChanged(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            if (SpecificPLValue.Value > g.PLimitMax)
            {
                MessageBox.Show("電力制限値が設定可能な範囲外です。\n" + g.Name + "の最大電力制限は" + g.PLimitMax + "Wです。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SpecificPLValue.Value = g.PLimitMax;
            }
            if (SpecificPLValue.Value < g.PLimitMin)
            {
                MessageBox.Show("電力制限値が設定可能な範囲外です。\n" + g.Name + "の最小電力制限は" + g.PLimitMin + "Wです。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SpecificPLValue.Value = g.PLimitMin;
            }
        }

        private void SetGPUPLSpecific_CheckedChanged(object sender, EventArgs e)
        {
            if(SetGPUPLSpecific.Checked == true)
            {
                SpecificPLValue.Enabled = true;
            }
            else
            {
                SpecificPLValue.Enabled = false;
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

            ConfigJson config = new ConfigJson(this);
            config.SaveConfig();
        }

        private void ResetClockSetting_Click(object sender, EventArgs e)
        {
            GpuStatus g = gpuStatuses.ElementAt(GpuIndex.SelectedIndex);
            nvsmi.nvidia_smi("-rgc --id=" + g.UUID);
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

        private void SettingTimeChange(object sender, EventArgs e)
        {
            if ((BeginTime.Value.Hour == EndTime.Value.Hour) && (BeginTime.Value.Minute == EndTime.Value.Minute))
            {
                if (limitstatus)
                {
                    Limit_Action(false, false);
                }
                BeginTime.Value = BeginTime.Value.AddMinutes(15);
                MessageBox.Show("開始時間と終了時刻は同じ時間に設定できません","エラー",MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }
        }
    }
}
