using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public interface INvidiaSmiService
{
    bool IsAvailable();
    Task<IReadOnlyList<GpuStatus>> QueryAllGpusAsync(CancellationToken ct = default);
    Task SetPowerLimitAsync(string uuid, int watts, CancellationToken ct = default);
    Task SetCoreClockLimitAsync(string uuid, int minCoreClock, int maxCoreClock, CancellationToken ct = default);
    Task ResetCoreClockLimitAsync(string uuid, CancellationToken ct = default);
}
