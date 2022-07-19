using System;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VRCGPUTool
{
    public partial class BugReport : Form
    {

        public BugReport(int typeIndex)
        {
            InitializeComponent();
            if(typeIndex == 0)
            {
                bug.Checked = true;
            }
            else
            {
                func.Checked = true;
            }
            openFileDialog1.Filter = "画像ファイル(*.png, *.jpg)|*.png;*.jpg";
        }

        private void InitializeReportWorker()
        {
            reportSendWorker = new BackgroundWorker();
            reportSendWorker.DoWork += new DoWorkEventHandler(reportSendWorker_DoWork);
            reportSendWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(reportSendWorker_RunWorkerCompleted);
        }

        BackgroundWorker reportSendWorker;

        private void reportSendWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Task<string> worker = Task.Run<string>(async () => {
                BackgroundWorker w = sender as BackgroundWorker;
                APIs api = new APIs();

                var client = new HttpClient();

                var message = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(api.ReportAPI),
                };

                message.Headers.UserAgent.Add(new ProductInfoHeaderValue("VRChatGPUTool", "0.0.0.0"));

                var result = await client.SendAsync(message).ConfigureAwait(false);

                result.EnsureSuccessStatusCode();

                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            });

            worker.Wait();

            e.Result = worker.Result;

            MessageBox.Show(e.Result.ToString());

            e.Result = JsonSerializer.Deserialize<GitHubApi>(
                worker.Result,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
            );
        }

        private void reportSendWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            { 
                MessageBox.Show(string.Format("送信中にエラーが発生しました。\n\n{0}", e.Error.ToString()));
                return;
            }
            string body = ((GitHubApi)e.Result).body;
            MessageBox.Show(string.Format("{0}",body));
        }

        private void Submit(object sender, EventArgs e)
        {
            InitializeReportWorker();
        }

        private void fileadd_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = openFileDialog1;
            if (ofd.ShowDialog() == DialogResult.OK)
            {

            }
        }
    }
}
