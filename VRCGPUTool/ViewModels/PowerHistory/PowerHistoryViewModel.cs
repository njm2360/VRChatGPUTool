using VRCGPUTool.Models;
using VRCGPUTool.Services;

namespace VRCGPUTool.ViewModels.PowerHistory;

public sealed class PowerHistoryViewModel(
    IPowerLogService powerLogService,
    PowerLogCsvExporter exporter,
    Func<HourlyPowerLog> getLiveLog,
    ElectricityProfile profile)
{
    public DailyHistoryViewModel DailyVm { get; } = new DailyHistoryViewModel(powerLogService, exporter, getLiveLog, profile);
    public MonthlyHistoryViewModel MonthlyVm { get; } = new MonthlyHistoryViewModel(powerLogService, exporter, getLiveLog, profile);
}
