using System;
using System.IO;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;
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

        BackgroundWorker reportSendWorker;

        private void reportSendWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Task<string> worker = Task.Run<string>(async () => {
                BackgroundWorker w = sender as BackgroundWorker;
                APIs api = new APIs();

                var client = new HttpClient();

                var multipart = new MultipartFormDataContent("report-data-split");

                if(bug.Checked == true)
                {
                    multipart.Add(new StringContent("BugReport"), "Type");
                }
                if(func.Checked == true)
                {
                    multipart.Add(new StringContent("Function"), "Type");
                }

                multipart.Add(new StringContent("<STX>" + body.Text + "<ETX>"),"Text");//Encoding.UTF8

                if (selectFilePath != null)
                {
                    var finfo = new FileInfo(selectFilePath);
                    FileStream fs = new FileStream(finfo.FullName, FileMode.Open);

                    BinaryReader br = new BinaryReader(fs);

                    ByteArrayContent imageContent = new ByteArrayContent(br.ReadBytes((int)fs.Length));

                    multipart.Add(imageContent, "Image", finfo.Name);

                }

                var result = await client.PostAsync(api.ReportAPI,multipart).ConfigureAwait(false);

                result.EnsureSuccessStatusCode();

                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            });

            worker.Wait();

            e.Result = worker.Result;

            e.Result = JsonSerializer.Deserialize<RepoApiRes>(
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
            //string body = ((RepoApiRes)e.Result).body;
            //MessageBox.Show(string.Format("{0}",body));

            MessageBox.Show("送信が完了しました", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
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
                }
            }
        }

        private void fileadd_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = openFileDialog1;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectFilePath = ofd.FileName;
                string [] fPath = selectFilePath.Split('\\');
                fileCount.Text = fPath[fPath.Length - 1];
            }
        }
    }
}
