namespace VRCGPUTool.Models;

[Flags]
public enum ScheduleDays
{
    None = 0,
    Monday = 1 << 0,
    Tuesday = 1 << 1,
    Wednesday = 1 << 2,
    Thursday = 1 << 3,
    Friday = 1 << 4,
    Saturday = 1 << 5,
    Sunday = 1 << 6,
}

public sealed class ScheduleSlot
{
    public bool Enabled { get; set; } = true;
    public int StartHour { get; set; }
    public int StartMinute { get; set; }
    public int EndHour { get; set; }
    public int EndMinute { get; set; }
    public ScheduleDays Days { get; set; } =
        ScheduleDays.Monday | ScheduleDays.Tuesday | ScheduleDays.Wednesday |
        ScheduleDays.Thursday | ScheduleDays.Friday | ScheduleDays.Saturday | ScheduleDays.Sunday;
}
