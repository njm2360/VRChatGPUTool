using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public interface IPowerLogService
{
    Task<HourlyPowerLog> LoadForDateAsync(DateOnly date);
    Task SaveAsync(HourlyPowerLog log);
}
