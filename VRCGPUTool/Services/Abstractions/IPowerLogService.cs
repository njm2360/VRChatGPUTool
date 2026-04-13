using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public interface IPowerLogService
{
    Task<HourlyPowerLog> LoadForDateAsync(DateOnly date);
    Task SaveAsync(HourlyPowerLog log);
    Task ExportToCsvAsync(HourlyPowerLog log, string filePath);
    Task ExportMonthToCsvAsync(DateOnly month, string filePath);
}
