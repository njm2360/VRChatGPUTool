using System;

namespace VRCGPUTool
{
    public class GPUPowerLog
    {
        public GPUPowerLog()
        {
            rawdata = new RawData();
        }

        internal RawData rawdata;

        internal class RawData
        {
            public int[] hourPowerLog { get; set; } = new int[24];
            public DateTime logdate { get; set; } = DateTime.Now;
        }

        internal void ClearPowerLog()
        {
            foreach(int i in rawdata.hourPowerLog)
            {
                rawdata.hourPowerLog[i] = 0;
            }
        }

        internal void AddPowerDeltaData(int hour,int value)
        {
            rawdata.hourPowerLog[hour] += value;
        }
    }
}
