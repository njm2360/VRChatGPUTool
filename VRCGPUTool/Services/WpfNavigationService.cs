using System.Diagnostics;
using System.Windows;
using VRCGPUTool.Models;
using VRCGPUTool.ViewModels;
using VRCGPUTool.ViewModels.PowerHistory;
using VRCGPUTool.Views;

namespace VRCGPUTool.Services;

public sealed class WpfNavigationService(
    IStartupService startupService,
    IElectricityProfileService electricityProfileService,
    IDialogService dialogService) : INavigationService
{
    private PowerHistoryWindow? _powerHistoryWindow;

    public SettingsDialogResult? ShowSettingsDialog(
        AppConfig config,
        IReadOnlyList<GpuStatus> gpus,
        string selectedGpuUuid,
        ElectricityProfile electricityProfile)
    {
        var vm = new SettingsViewModel(config, startupService, electricityProfileService, electricityProfile,
            gpus, selectedGpuUuid, dialogService);
        var window = new SettingsWindow { DataContext = vm, Owner = Application.Current.MainWindow };

        if (window.ShowDialog() != true) return null;

        return new SettingsDialogResult(
            vm.SelectedGpuUuid,
            vm.PowerLimitWatts,
            vm.RestoreDefaultOnUnlimit,
            vm.RestoreToWatts,
            vm.AutoLimitEnabled,
            vm.AutoLimitThreshold,
            vm.CoreClockLimitEnabled,
            vm.CoreClockMaxMhz);
    }

    public List<ScheduleSlot>? ShowScheduleDialog(List<ScheduleSlot> currentSlots)
    {
        var vm = new ScheduleSettingViewModel(currentSlots);
        var window = new ScheduleSettingWindow { DataContext = vm, Owner = Application.Current.MainWindow };
        return window.ShowDialog() == true ? vm.GetSlots() : null;
    }

    public void ShowOrActivatePowerHistory(
        IPowerLogService logService,
        Func<HourlyPowerLog> todayLogGetter,
        ElectricityProfile profile)
    {
        if (_powerHistoryWindow is not null)
        {
            _powerHistoryWindow.Activate();
            return;
        }

        var vm = new PowerHistoryViewModel(logService, todayLogGetter, profile);
        _powerHistoryWindow = new PowerHistoryWindow { DataContext = vm };
        _powerHistoryWindow.Closed += (_, _) => _powerHistoryWindow = null;
        _powerHistoryWindow.Show();
    }

    public void OpenUrl(string url)
        => Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
}
