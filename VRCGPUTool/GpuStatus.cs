namespace VRCGPUTool
{
    internal class GpuStatus
    {
        public string Name { get; private set; }
        public string UUID { get; private set; }
        public int PLimit { get; private set; }
        public int PLimitMin { get; private set; }
        public int PLimitMax { get; private set; }
        public int PLimitDefault { get; private set; }
        public int CoreLoad { get; private set; }
        public int CoreTemp { get; private set; }

        public GpuStatus(
            string Name,
            string UUID,
            int PLimit,
            int PLimitMin,
            int PLimitMax,
            int PLimitDefault,
            int CoreLoad,
            int CoreTemp
        )
        {
            this.Name = Name;
            this.UUID = UUID;
            this.PLimit = PLimit;
            this.PLimitMin = PLimitMin;
            this.PLimitMax = PLimitMax;
            this.PLimitDefault = PLimitDefault;
            this.CoreLoad = CoreLoad;
            this.CoreTemp = CoreTemp;
        }
    }
}
