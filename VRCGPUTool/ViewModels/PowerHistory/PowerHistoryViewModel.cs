using VRCGPUTool.Models;
using VRCGPUTool.Services;

namespace VRCGPUTool.ViewModels.PowerHistory;

public sealed class PowerHistoryViewModel(
    IPowerLogService powerLogService,
    Func<HourlyPowerLog> getLiveLog,
    ElectricityProfile profile)
{
    public DailyHistoryViewModel DailyVm { get; } = new DailyHistoryViewModel(powerLogService, getLiveLog, profile);
    public MonthlyHistoryViewModel MonthlyVm { get; } = new MonthlyHistoryViewModel(powerLogService, getLiveLog, profile);
}
