using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using VRCGPUTool.Form;

namespace VRCGPUTool.Util
{
    internal class ConfigFile
    {
        public MainForm MainObj;

        public ConfigFile(MainForm Main_Obj)
        {
            MainObj = Main_Obj;
        }

        private class Conf
        {
            public int BeginHour { get; set; } = DateTime.Now.Hour;
            public int BeginMinute { get; set; } = DateTime.Now.Minute;
            public int EndHour { get; set; } = DateTime.Now.Hour;
            public int EndMinute { get; set; } = DateTime.Now.Minute;
            public int PowerLimitSetting { get; set; } = 100;
            public int UnlimitPLSetting { get; set; } = 100;
            public bool RestoreGPUPLDefault { get; set; } = false;
        }

        private void CreateConfigFile()
        {
            try
            {
                Conf config = new Conf();
                config.BeginHour = MainObj.BeginTime.Value.Hour;
                config.BeginMinute = MainObj.BeginTime.Value.Minute;
                config.EndHour = MainObj.EndTime.Value.Hour;
                config.EndMinute = MainObj.EndTime.Value.Minute;
                config.PowerLimitSetting = (int)MainObj.PowerLimitValue.Value;
                config.UnlimitPLSetting = (int)MainObj.SpecificPLValue.Value;
                config.RestoreGPUPLDefault = true;

                string confjson = JsonSerializer.Serialize(config);

                using (StreamWriter sw = new StreamWriter("config.json"))
                {
                    sw.WriteLine(confjson);
                }

                Directory.CreateDirectory("./powerlog");
                var resmsg = MessageBox.Show("この度は「VRChat向け GPU電力制限ツール」\nをダウンロードしていただきありがとうございます。\n\nリリースノートを開きますか?", "ようこそ", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (resmsg == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo { FileName = "https://github.com/njm2360/VRChatGPUTool#readme", UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("設定ファイル作成時にエラーが発生しました\n\n{0}", ex.Message.ToString()), "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        internal void LoadConfig()
        {
            if (File.Exists("config.json"))
            {
                using (FileStream fs = File.OpenRead("config.json"))
                {
                    using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8))
                    {
                        try
                        {
                            while (!sr.EndOfStream)
                            {
                                Conf config = JsonSerializer.Deserialize<Conf>(sr.ReadLine());
                                MainObj.BeginTime.Value = new DateTime(1970, 1, 1, config.BeginHour, config.BeginMinute, 0);
                                MainObj.EndTime.Value = new DateTime(1970, 1, 1, config.EndHour, config.EndMinute, 0);
                                MainObj.PowerLimitValue.Value = config.PowerLimitSetting;
                                MainObj.SpecificPLValue.Value = config.UnlimitPLSetting;
                                if (config.RestoreGPUPLDefault == true)
                                {
                                    MainObj.ResetGPUDefaultPL.Checked = true;
                                    MainObj.SpecificPLValue.Enabled = false;
                                }
                                else
                                {
                                    MainObj.SetGPUPLSpecific.Checked = true;
                                    MainObj.SpecificPLValue.Enabled = true;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            var res = MessageBox.Show("設定ファイルに誤りがあります。\n設定ファイルを再生成しますか?","エラー", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                            if(res == DialogResult.Yes)
                            {
                                try                                {
                                    File.Delete("config.json");
                                }
                                catch(Exception)                                {
                                    MessageBox.Show("設定ファイルを削除できませんでした\n設定ファイルを手動で消してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    Environment.Exit(-1);
                                }
                                Application.Restart();
                            }
                            else
                            {
                                Application.Exit();
                            }
                        }
                    }
                }
            }
            else
            {
                CreateConfigFile();
            }
        }

        internal void SaveConfig()
        {
            try
            {
                Conf config = new Conf();

                config.BeginHour = MainObj.BeginTime.Value.Hour;
                config.BeginMinute = MainObj.BeginTime.Value.Minute;
                config.EndHour = MainObj.EndTime.Value.Hour;
                config.EndMinute = MainObj.EndTime.Value.Minute;
                config.PowerLimitSetting = (int)MainObj.PowerLimitValue.Value;
                config.UnlimitPLSetting = (int)MainObj.SpecificPLValue.Value;
                config.RestoreGPUPLDefault = MainObj.ResetGPUDefaultPL.Checked;

                string confjson = JsonSerializer.Serialize(config);

                using (StreamWriter sw = new StreamWriter("config.json"))
                {
                    sw.WriteLine(confjson);
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
