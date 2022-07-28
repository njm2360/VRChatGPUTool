﻿using System;
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
        const string ConfigFileName = "config.json";

        public ConfigFile(MainForm Main_Obj)
        {
            MainObj = Main_Obj;
        }
        private class Config
        {
            public string Guid { get; set; } = "";
            public int BeginHour { get; set; } = DateTime.Now.Hour;
            public int BeginMinute { get; set; } = DateTime.Now.Minute;
            public int EndHour { get; set; } = DateTime.Now.Hour;
            public int EndMinute { get; set; } = DateTime.Now.Minute;
            public int PowerLimitSetting { get; set; } = 0;
            public int UnlimitPLSetting { get; set; } = 0;
            public bool RestoreGPUPLDefault { get; set; } = false;
            public int SelectGPUIndex { get; set; } = 0;
            public bool AllowDataProvide { get; set; } = false;
        }

        private bool isFirstCreate = false;

        private void CreateConfigFile()
        {
            var resmsg = MessageBox.Show("この度は「VRChat向け GPU電力制限ツール」\nをダウンロードしていただきありがとうございます。\n\nリリースノートを開きますか?", "ようこそ", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (resmsg == DialogResult.Yes)
            {
                Process.Start(new ProcessStartInfo { FileName = "https://github.com/njm2360/VRChatGPUTool#readme", UseShellExecute = true });
            }

            Config config = new Config
            {
                Guid = Guid.NewGuid().ToString(),
                BeginHour = MainObj.BeginTime.Value.Hour,
                BeginMinute = MainObj.BeginTime.Value.Minute,
                EndHour = MainObj.EndTime.Value.Hour,
                EndMinute = MainObj.EndTime.Value.Minute,
                PowerLimitSetting = (int)MainObj.PowerLimitValue.Value,
                UnlimitPLSetting = (int)MainObj.SpecificPLValue.Value,
                RestoreGPUPLDefault = true,
                SelectGPUIndex = 0
            };

            var resmsg2 = MessageBox.Show("確認", "ようこそ", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (resmsg2 == DialogResult.Yes)
            {
                config.AllowDataProvide = true;
            }

            string confjson = JsonSerializer.Serialize(config);
            try
            {
                Directory.CreateDirectory("./powerlog");
                using (StreamWriter sw = new StreamWriter(ConfigFileName))
                {
                    sw.WriteLine(confjson);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"設定ファイル作成時にエラーが発生しました\n\n{ex.Message.ToString()}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }


        internal void LoadConfig()
        {
            if (File.Exists(ConfigFileName))
            {
                using (FileStream fs = File.OpenRead(ConfigFileName))
                {
                    using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8))
                    {
                        try
                        {
                            while (!sr.EndOfStream)
                            {
                                Config config = JsonSerializer.Deserialize<Config>(sr.ReadToEnd());
                                MainObj.BeginTime.Value = new DateTime(1970, 1, 1, config.BeginHour, config.BeginMinute, 0);
                                MainObj.EndTime.Value = new DateTime(1970, 1, 1, config.EndHour, config.EndMinute, 0);
                                try
                                {
                                    MainObj.GpuIndex.SelectedIndex = config.SelectGPUIndex;
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    MainObj.GpuIndex.SelectedIndex = 0;
                                }

                                if (!isFirstCreate)
                                {
                                    MainObj.PowerLimitValue.Value = config.PowerLimitSetting;
                                    MainObj.SpecificPLValue.Value = config.UnlimitPLSetting;
                                }

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

                                MainObj.allowDataProvide = config.AllowDataProvide;
                                MainObj.guid = config.Guid;
                            }
                        }
                        catch (Exception)
                        {
                            var res = MessageBox.Show("設定ファイルに誤りがあります。\n設定ファイルを再生成しますか?", "エラー", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                            if (res == DialogResult.Yes)
                            {
                                try
                                {
                                    File.Delete(ConfigFileName);
                                }
                                catch (Exception)
                                {
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
                isFirstCreate = true;
                CreateConfigFile();
                LoadConfig();
            }
        }

        internal void SaveConfig()
        {
            try
            {
                Config config = new Config
                {
                    BeginHour = MainObj.BeginTime.Value.Hour,
                    BeginMinute = MainObj.BeginTime.Value.Minute,
                    EndHour = MainObj.EndTime.Value.Hour,
                    EndMinute = MainObj.EndTime.Value.Minute,
                    PowerLimitSetting = (int)MainObj.PowerLimitValue.Value,
                    UnlimitPLSetting = (int)MainObj.SpecificPLValue.Value,
                    RestoreGPUPLDefault = MainObj.ResetGPUDefaultPL.Checked,
                    SelectGPUIndex = MainObj.GpuIndex.SelectedIndex,
                    AllowDataProvide = MainObj.allowDataProvide,
                    Guid = MainObj.guid
                };

                string confjson = JsonSerializer.Serialize(config);

                using (StreamWriter sw = new StreamWriter(ConfigFileName))
                {
                    sw.WriteLine(confjson);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"設定ファイル更新時にエラーが発生しました\n\n{ex.Message.ToString()}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }
    }
}