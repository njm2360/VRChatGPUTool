namespace VRCGPUTool.Models;

public sealed record GpuStatus(
    string Name,
    string Uuid,
    int PowerLimit,
    int PowerLimitMin,
    int PowerLimitMax,
    int PowerLimitDefault,
    int GpuUtilization,
    int CoreTemperature,
    int PowerDraw,
    int CoreClock,
    int MemoryClock
);
