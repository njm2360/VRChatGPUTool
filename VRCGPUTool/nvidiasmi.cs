using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VRCGPUTool
{
    partial class Main
    {
        private void InitializeNvsmiWorker()
        {
            NvsmiWorker = new BackgroundWorker();
            NvsmiWorker.DoWork += new DoWorkEventHandler(NvsmiWorker_DoWork);
            NvsmiWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(NvsmiWorker_RunWorkerCompleted);
        }

        BackgroundWorker NvsmiWorker;

        private void NvsmiWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string param = e.Argument.ToString();

            Console.WriteLine(param);

            ProcessStartInfo nvsmi = new ProcessStartInfo("nvidia-smi.exe");
            nvsmi.WorkingDirectory = @"C:\Windows\system32\";
            nvsmi.Arguments = param;
            nvsmi.Verb = "RunAs";
            nvsmi.CreateNoWindow = true;
            nvsmi.UseShellExecute = false;
            nvsmi.RedirectStandardOutput = true;

            Process p = Process.Start(nvsmi);

            e.Result = p.StandardOutput.ReadToEnd();
        }

        private void NvsmiWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string res = e.Result.ToString();
            MessageBox.Show(res);
        }

    }
}
