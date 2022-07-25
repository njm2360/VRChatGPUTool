using System;
using VRCGPUTool.Form;

namespace VRCGPUTool.Util
{
    internal class AutoLimit
    {
        private int[] recentutil = new int[300];
        private int writeaddr = 0;
        private bool data_ready = false;

        private const int AVE_DELTA = 20;

        MainForm MainObj;

        public AutoLimit(MainForm Main_Obj)
        {
            MainObj = Main_Obj;
            DataInitialize();
        }

        private void DataInitialize()
        {
            for (int i = 0; i < recentutil.Length; i++)
            {
                recentutil[i] = -1;
            }
        }

        internal bool CheckAutoLimit(GpuStatus g)
        {
            recentutil[writeaddr] = g.CoreLoad;
            writeaddr++;
            if (writeaddr >= 300)
            {
                writeaddr = 0;
                data_ready = true;
            }
            if (data_ready == true)
            {
                int max_util = 0;
                int min_util = 100;
                int[] ave_util = new int[(recentutil.Length / AVE_DELTA)];

                for (int i = 0; i < ave_util.Length; i++)
                {
                    for (int s = 0; s < AVE_DELTA; s++)
                    {
                        ave_util[i] += recentutil[AVE_DELTA * i + s];
                    }
                    ave_util[i] /= AVE_DELTA;
                    if (ave_util[i] > max_util)
                    {
                        max_util = ave_util[i];
                    }
                    if (ave_util[i] < min_util)
                    {
                        min_util = ave_util[i];
                    }
                }

                if (max_util - min_util < Convert.ToInt16(MainObj.GPUusageThreshold.Value) && !MainObj.limitstatus)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
