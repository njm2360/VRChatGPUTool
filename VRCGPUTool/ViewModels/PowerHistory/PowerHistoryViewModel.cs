using Microsoft.Extensions.Logging;
using VRCGPUTool.Models;
using VRCGPUTool.Services;

namespace VRCGPUTool.ViewModels.PowerHistory;

public sealed class PowerHistoryViewModel(
    IPowerLogService powerLogService,
    PowerLogCsvExporter exporter,
    Func<HourlyPowerLog> getLiveLog,
    ElectricityProfile profile,
    ILoggerFactory? loggerFactory = null)
{
    public DailyHistoryViewModel DailyVm { get; } = new DailyHistoryViewModel(powerLogService, exporter, getLiveLog, profile,
        loggerFactory?.CreateLogger<DailyHistoryViewModel>());
    public MonthlyHistoryViewModel MonthlyVm { get; } = new MonthlyHistoryViewModel(powerLogService, exporter, getLiveLog, profile,
        loggerFactory?.CreateLogger<MonthlyHistoryViewModel>());
}
