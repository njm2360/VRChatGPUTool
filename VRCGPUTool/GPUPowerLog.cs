namespace VRCGPUTool
{
    public class GPUPowerLog
    {
        public GPUPowerLog()
        {
            rdata = new RawData();
        }

        internal static RawData rdata;

        internal class RawData
        {
            public int[] DayPowerLog { get; set; } = new int[31];
            public int[] HourPowerLog { get; set; } = new int[24];
        }

        internal void AddPowerDeltaData(int hour,int value)
        {
            rdata.HourPowerLog[hour] += value;
            //MessageBox.Show(rdata.HourPowerLog[hour].ToString());
        }
    }
}
