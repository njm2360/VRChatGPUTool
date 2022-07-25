﻿using System;
using VRCGPUTool.Util;

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

        internal void PowerLogging(DateTime dt, GpuStatus g,GPUPowerLog gpuPlog)
        {
            if (gpuPlog.rawdata.logdate.Day != dt.Day)
            {
                PowerLogFile plog = new PowerLogFile(gpuPlog);
                plog.SaveConfig(gpuPlog.rawdata.logdate);
                gpuPlog = null;
                gpuPlog = new GPUPowerLog();
            }

            gpuPlog.AddPowerDeltaData(dt.Hour, g.PowerDraw);
        }

        private void AddPowerDeltaData(int hour,int value)
        {
            rawdata.hourPowerLog[hour] += value;
        }
    }
}
