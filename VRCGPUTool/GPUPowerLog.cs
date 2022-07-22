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
            public int[] DayPowerLog { get; set; } = new int[31];
            public int[] HourPowerLog { get; set; } = new int[24];
        }

        internal void AddPowerDeltaData(int hour,int value)
        {
            rawdata.HourPowerLog[hour] += value;
            //MessageBox.Show(rdata.HourPowerLog[hour].ToString());
        }
    }
}
