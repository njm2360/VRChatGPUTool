namespace VRCGPUTool.Models;

public sealed class AppConfig
{
    public int Version { get; init; } = 2;

    public string SelectedGpuUuid { get; set; } = string.Empty;
    public int PowerLimitWatts { get; set; } = 100;
    public bool RestoreDefaultOnUnlimit { get; set; } = true;
    public int RestoreToWatts { get; set; } = 0;

    public List<ScheduleSlot> Schedules { get; set; } = [];

    public bool AutoLimitEnabled { get; set; } = false;
    public int AutoLimitThreshold { get; set; } = 10;

    public bool CoreClockLimitEnabled { get; set; } = false;
    public int CoreClockMaxMhz { get; set; } = 1500;
}
