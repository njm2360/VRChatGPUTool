using System;
using System.Threading;
using System.Windows.Forms;

namespace VRCGPUTool
{
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            string mutexName = "Test1";
            bool createdNew = false;
            Mutex mutex = new Mutex(true, mutexName, out createdNew);
            if (createdNew)
            {
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Main());
                }
                catch
                {
                }
                finally
                {
                    mutex.ReleaseMutex();
                    mutex.Close();
                }
            }
            else
            {
                MessageBox.Show("二重起動を検出しました","エラー",MessageBoxButtons.OK,MessageBoxIcon.Error);
                mutex.Close();
            }
        }
    }
}
