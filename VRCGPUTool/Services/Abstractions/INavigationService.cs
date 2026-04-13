using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

/// <summary>
/// Settings ダイアログの確定結果。キャンセル時は ShowSettingsDialog が null を返す。
/// </summary>
public sealed record SettingsDialogResult(
    string SelectedGpuUuid,
    int PowerLimitWatts,
    bool RestoreDefaultOnUnlimit,
    int RestoreToWatts,
    bool AutoLimitEnabled,
    int AutoLimitThreshold,
    bool CoreClockLimitEnabled,
    int CoreClockMaxMhz)
{
    public void ApplyTo(AppConfig config)
    {
        config.SelectedGpuUuid = SelectedGpuUuid;
        config.PowerLimitWatts = PowerLimitWatts;
        config.RestoreDefaultOnUnlimit = RestoreDefaultOnUnlimit;
        config.RestoreToWatts = RestoreToWatts;
        config.AutoLimitEnabled = AutoLimitEnabled;
        config.AutoLimitThreshold = AutoLimitThreshold;
        config.CoreClockLimitEnabled = CoreClockLimitEnabled;
        config.CoreClockMaxMhz = CoreClockMaxMhz;
    }
}

public interface INavigationService
{
    /// <returns>キャンセルなら null、OK なら確定値</returns>
    SettingsDialogResult? ShowSettingsDialog(
        AppConfig config,
        IReadOnlyList<GpuStatus> gpus,
        string selectedGpuUuid,
        ElectricityProfile electricityProfile);

    /// <returns>キャンセルなら null、OK なら新しいスロットリスト</returns>
    List<ScheduleSlot>? ShowScheduleDialog(List<ScheduleSlot> currentSlots);

    void ShowOrActivatePowerHistory(
        IPowerLogService logService,
        Func<HourlyPowerLog> todayLogGetter,
        ElectricityProfile profile);

    void OpenUrl(string url);
}
