using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using VRCGPUTool.Form;
using System.Text;
using System.Linq;

namespace VRCGPUTool.Util
{
    public partial class DataProvide
    {
        public DataProvide()
        {
            InitializeDataProvideWorker();
        }

        private class ProvData
        {
            public string Guid { get; set; }
            public string Version { get; set; }
            public string Name { get; set; }
            public int PLimit { get; set; }
            public int PLimitMax { get; set; }
            public int PLimitMin { get; set; }
            public string tag { get; set; }
            public string desc { get; set; }
        }

        internal BackgroundWorker dataSendWorker;

        internal void InitializeRepo(MainForm fm)
        {
            //FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            GpuStatus g = fm.gpuStatuses.ElementAt(fm.GpuIndex.SelectedIndex);

            ProvData provData = new ProvData
            {
                Guid = fm.guid,
                Version = "0.0.0", //fileVersionInfo.ProductVersion,
                Name = g.Name,
                PLimit = g.PLimit,
                PLimitMax = g.PLimitMax,
                PLimitMin = g.PLimitMin
            };

            if (dataSendWorker.IsBusy == false)
            {
                dataSendWorker.RunWorkerAsync(provData);
            }
        }

        private void InitializeDataProvideWorker()
        {
            dataSendWorker = new BackgroundWorker();
            dataSendWorker.DoWork += new DoWorkEventHandler(dataSendWorker_DoWork);
            dataSendWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(dataSendWorker_RunWorkerCompleted);
        }

        private void dataSendWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Task<string> worker = Task.Run<string>(async () =>
            {
                BackgroundWorker w = sender as BackgroundWorker;

                var client = new HttpClient();

                var message = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(APIEndpoints.ProvideDataAPIEndPoint),
                };

                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.
                    Create(System.Text.Unicode.UnicodeRanges.All)
                };

                string json = JsonSerializer.Serialize(e.Argument);

                message.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var result = await client.SendAsync(message).ConfigureAwait(false);

                result.EnsureSuccessStatusCode();

                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            });

            worker.Wait();

            e.Result = worker.Result;

            e.Result = JsonSerializer.Deserialize<ProvDataApiRes>(
                worker.Result,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
            );
        }

        private void dataSendWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }
        }
    }
}

