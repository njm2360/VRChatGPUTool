using System.ComponentModel;

namespace VRCGPUTool
{
    partial class Form1
    {
        private void InitializeBackgroundWorker() {
            this.checkUpdateWorker = new BackgroundWorker();
            this.checkUpdateWorker.DoWork += new DoWorkEventHandler(checkUpdateWorker_DoWork);
            this.checkUpdateWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(checkUpdateWorker_RunWorkerCompleted);
        }

        BackgroundWorker checkUpdateWorker;
    }
}
