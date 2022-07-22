using System;
using System.IO;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VRCGPUTool
{
    public partial class BugReport : Form
    {

        private string selectFilePath = null;

        public BugReport(int typeIndex)
        {
            InitializeComponent();
            InitializeReportWorker();
            if (typeIndex == 0)
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

        internal BackgroundWorker reportSendWorker;

        private void reportSendWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Task<string> worker = Task.Run<string>(async () => {
                BackgroundWorker w = sender as BackgroundWorker;

                var client = new HttpClient();

                var multipart = new MultipartFormDataContent();

                if(bug.Checked == true)
                {
                    multipart.Add(new StringContent("BugReport"), "Type");
                }
                if(func.Checked == true)
                {
                    multipart.Add(new StringContent("Function"), "Type");
                }
                if(emailinput.Text != "")
                {
                    multipart.Add(new StringContent(emailinput.Text), "Email");
                }

                multipart.Add(new StringContent(body.Text),"Text");

                if (selectFilePath != null)
                {
                    var finfo = new FileInfo(selectFilePath);
                    FileStream fs = new FileStream(finfo.FullName, FileMode.Open);

                    BinaryReader br = new BinaryReader(fs);

                    ByteArrayContent imageContent = new ByteArrayContent(br.ReadBytes((int)fs.Length));

                    multipart.Add(imageContent, "Image", finfo.Name);

                }

                var result = await client.PostAsync(APIEndpoints.FeedbackEndpoint, multipart).ConfigureAwait(false);

                result.EnsureSuccessStatusCode();

                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            });

            worker.Wait();

            e.Result = worker.Result;

            e.Result = JsonSerializer.Deserialize<ReportApiRes>(
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
        }

        private void Submit(object sender, EventArgs e)
        {
            if(body.Text == "")
            {
                MessageBox.Show("内容が入力されていません","エラー",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            var res = MessageBox.Show("送信してもよろしいですか?","確認",MessageBoxButtons.OKCancel,MessageBoxIcon.Question);
            if (res == DialogResult.OK)
            {
                if (reportSendWorker.IsBusy == false)
                {
                    reportSendWorker.RunWorkerAsync();
                    RepoTypeGroup.Enabled = false;
                    body.Enabled = false;
                    MessageBox.Show("送信しました", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                }
                else
                {
                    MessageBox.Show("送信エラーが発生しました。時間をおいてからやり直してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
        }

        private void fileadd_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = openFileDialog1;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectFilePath = ofd.FileName;
                var finfo = new FileInfo(selectFilePath);
                if (finfo.Length > 1024 * 1024 * 4)
                {
                    MessageBox.Show("ファイルサイズは4MB以下にしてください", "エラー", MessageBoxButtons.OK);
                    selectFilePath = null;
                }
                string [] fPath = selectFilePath.Split('\\');
                fileCount.Text = fPath[fPath.Length - 1];
            }
        }
    }
}
