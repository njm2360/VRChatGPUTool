using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public interface IPowerLogService
{
    Task<HourlyPowerLog> LoadForDateAsync(DateOnly date);

    /// <summary>指定月の全日分のログを返す (インデックス = 日 - 1、データのない日は空ログ)。</summary>
    Task<IReadOnlyList<HourlyPowerLog>> LoadMonthAsync(DateOnly month);

    Task SaveAsync(HourlyPowerLog log);
}
