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
            public int[] hourPowerLog { get; set; } = new int[31];
        }

        internal void AddPowerDeltaData(int hour,int value)
        {
            rawdata.hourPowerLog[hour] += value;
        }
    }
}
