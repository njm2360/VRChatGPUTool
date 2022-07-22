using System;
using System.Text.Json;
using System.IO;
using System.Windows.Forms;
//using static VRCGPUTool.GPUPowerLog;

namespace VRCGPUTool.Util
{
    internal class PowerLog
    {
        public PowerLog(GPUPowerLog glog)
        {
            gpupowerlog = glog;
        }

        GPUPowerLog gpupowerlog;

        internal void LoadPowerLog()
        {
            if (File.Exists("power_history.json"))
            {
                using (FileStream fs = File.OpenRead("power_history.json"))
                {
                    using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8))
                    {
                        while (!sr.EndOfStream)
                        {
                            gpupowerlog.rawdata = JsonSerializer.Deserialize<GPUPowerLog.RawData>(sr.ReadLine());
                        }
                    }
                }
            }
            else
            {
                try
                {
                    GPUPowerLog plog = new GPUPowerLog();
                    
                    string logjson = JsonSerializer.Serialize(plog);

                    using (StreamWriter sw = new StreamWriter("power_history.json"))
                    {
                        sw.WriteLine(logjson);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("設定ファイル作成時にエラーが発生しました\n\n{0}", ex.Message.ToString()), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(-1);
                }

            }
        }
        
        internal void SaveConfig()
        {
            try
            {
                GPUPowerLog plog = new GPUPowerLog();

                string logjson = JsonSerializer.Serialize(gpupowerlog.rawdata);

                using (StreamWriter sw = new StreamWriter("power_history.json"))
                {
                    sw.WriteLine(logjson);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("設定ファイル更新時にエラーが発生しました\n\n{0}", ex.Message.ToString()), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }
    }
}

