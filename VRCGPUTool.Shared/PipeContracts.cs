namespace VRCGPUTool.Shared;

public static class PipeNames
{
    public const string NvidiaSmi = "VRCGPUTool_NvidiaSmi";
}

public sealed record PipeRequest(
    string Cmd,
    string? Uuid = null,
    int? Watts = null,
    int? MinCoreClock = null,
    int? MaxCoreClock = null);

public sealed record PipeResponse(
    bool Ok,
    string? Error = null,
    IReadOnlyList<GpuStatusDto>? Gpus = null);

public sealed record GpuStatusDto(
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
    int MemoryClock);
