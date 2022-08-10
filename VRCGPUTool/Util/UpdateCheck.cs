using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VRCGPUTool.Util
{
    partial class UpdateCheck
    {
        internal BackgroundWorker checkUpdateWorker;

        const string boothUrl = "https://njm2360.booth.pm/items/3993173";

        public UpdateCheck()
        {
            InitializeBackgroundWorker();
        }

        private void InitializeBackgroundWorker()
        {
            checkUpdateWorker = new BackgroundWorker();
            checkUpdateWorker.DoWork += new DoWorkEventHandler(checkUpdateWorker_DoWork);
            checkUpdateWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(checkUpdateWorker_RunWorkerCompleted);
        }

        private void checkUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string input = (string)e.Argument;

            Task<string> worker = Task.Run<string>(async () => {
                BackgroundWorker w = sender as BackgroundWorker;

                var message = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(APIEndpoints.GitHubReleaseAPIEndpoint),
                };

                message.Headers.UserAgent.Add(new ProductInfoHeaderValue("VRChatGPUTool", "0.0.0.0"));

                var result = await HttpRequest.client.SendAsync(message).ConfigureAwait(false);

                result.EnsureSuccessStatusCode();

                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            });

            worker.Wait();

            e.Result = JsonSerializer.Deserialize<GitHubApiRes>(
                worker.Result,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
            );
        }

        private void checkUpdateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(string.Format("アップデートチェック中にエラーが発生しました。\n\n{0}", e.Error.ToString()));
                return;
            }
            string tag_name = ((GitHubApiRes)e.Result).tag_name;
            string body = ((GitHubApiRes)e.Result).body;

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = "v" + fileVersionInfo.ProductVersion;

            if (tag_name != version)
            {
                var res = MessageBox.Show("アップデートがあります\n\n最新バージョンは " + tag_name + " です\n\n改定内容:\n" + body + "\n\nアップデートページ(Booth)を開きますか?", "アップデート", MessageBoxButtons.OKCancel);
                if (res == DialogResult.OK)
                {
                    Process.Start(new ProcessStartInfo { FileName = boothUrl, UseShellExecute = true });
                }
            }
        }
    }
}
