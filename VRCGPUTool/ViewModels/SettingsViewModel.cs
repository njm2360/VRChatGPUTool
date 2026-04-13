using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VRCGPUTool.Models;
using VRCGPUTool.Services;

namespace VRCGPUTool.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IStartupService _startupService;
    private readonly IElectricityProfileService _profileService;
    private readonly ElectricityProfile _profile;
    private readonly IDialogService _dialogService;

    [ObservableProperty] private IReadOnlyList<GpuStatus> _gpus;
    [ObservableProperty] private int _selectedGpuIndex;

    [ObservableProperty] private bool _launchAtStartup;
    [ObservableProperty] private bool _autoLimitEnabled;
    [ObservableProperty] private int _autoLimitThreshold;
    [ObservableProperty] private bool _coreClockLimitEnabled;
    [ObservableProperty] private int _coreClockMaxMhz;
    [ObservableProperty] private int _powerLimitWatts;
    [ObservableProperty] private bool _restoreDefaultOnUnlimit;
    [ObservableProperty] private int _restoreToWatts;

    private GpuStatus? SelectedGpu
        => SelectedGpuIndex >= 0 && SelectedGpuIndex < Gpus.Count ? Gpus[SelectedGpuIndex] : null;

    public string SelectedGpuUuid => SelectedGpu?.Uuid ?? string.Empty;

    public SettingsViewModel(AppConfig config, IStartupService startupService,
        IElectricityProfileService profileService, ElectricityProfile profile,
        IReadOnlyList<GpuStatus> gpus, string selectedGpuUuid,
        IDialogService dialogService)
    {
        _startupService = startupService;
        _profileService = profileService;
        _profile = profile;
        _gpus = gpus;
        _dialogService = dialogService;

        var match = gpus.Select((g, i) => (g, i)).FirstOrDefault(t => t.g.Uuid == selectedGpuUuid);
        _selectedGpuIndex = match.g is not null ? match.i : 0;

        LaunchAtStartup = startupService.IsEnabled();
        AutoLimitEnabled = config.AutoLimitEnabled;
        AutoLimitThreshold = config.AutoLimitThreshold;
        CoreClockLimitEnabled = config.CoreClockLimitEnabled;
        CoreClockMaxMhz = config.CoreClockMaxMhz;
        RestoreDefaultOnUnlimit = config.RestoreDefaultOnUnlimit;

        var gpu = SelectedGpu;
        int min = gpu?.PowerLimitMin ?? 0;
        int max = gpu?.PowerLimitMax ?? int.MaxValue;
        PowerLimitWatts = Math.Clamp(config.PowerLimitWatts, min, max);
        RestoreToWatts = Math.Clamp(config.RestoreToWatts, min, max);
    }

    partial void OnSelectedGpuIndexChanged(int value)
    {
        var gpu = SelectedGpu;
        if (gpu is null) return;
        PowerLimitWatts = Math.Clamp(PowerLimitWatts, gpu.PowerLimitMin, gpu.PowerLimitMax);
        RestoreToWatts = Math.Clamp(RestoreToWatts, gpu.PowerLimitMin, gpu.PowerLimitMax);
    }

    partial void OnPowerLimitWattsChanged(int value)
    {
        var gpu = SelectedGpu;
        if (gpu is null) return;
        PowerLimitWatts = Math.Clamp(value, gpu.PowerLimitMin, gpu.PowerLimitMax);
    }

    partial void OnRestoreToWattsChanged(int value)
    {
        var gpu = SelectedGpu;
        if (gpu is null) return;
        RestoreToWatts = Math.Clamp(value, gpu.PowerLimitMin, gpu.PowerLimitMax);
    }

    [RelayCommand] private void LoadMinWatts() => PowerLimitWatts = SelectedGpu?.PowerLimitMin ?? PowerLimitWatts;
    [RelayCommand] private void LoadDefaultWatts() => PowerLimitWatts = SelectedGpu?.PowerLimitDefault ?? PowerLimitWatts;
    [RelayCommand] private void LoadMaxWatts() => PowerLimitWatts = SelectedGpu?.PowerLimitMax ?? PowerLimitWatts;

    [RelayCommand]
    private void OpenPriceSetting(Window owner)
    {
        var vm = new UnitPriceSettingViewModel(_profileService, _profile);
        var window = new Views.UnitPriceSettingWindow { DataContext = vm, Owner = owner };
        window.ShowDialog();
    }

    [RelayCommand]
    private void Save(Window window)
    {
        if (AutoLimitEnabled && AutoLimitThreshold is < 1 or > 99)
        {
            _dialogService.ShowWarning("安定判定閾値は1〜99で指定してください。", "入力エラー");
            return;
        }

        if (CoreClockLimitEnabled && CoreClockMaxMhz is < 200 or > 2000)
        {
            _dialogService.ShowWarning("最大クロックは200〜2000MHzで指定してください。", "入力エラー");
            return;
        }

        try
        {
            if (LaunchAtStartup) _startupService.Enable();
            else _startupService.Disable();
        }
        catch (Exception ex)
        {
            _dialogService.ShowWarning($"スタートアップ設定の保存に失敗しました:\n{ex.Message}");
        }

        window.DialogResult = true;
        window.Close();
    }

    [RelayCommand]
    private static void Cancel(Window window)
    {
        window.DialogResult = false;
        window.Close();
    }

    /// <summary>保存確定後に AppConfig へ値を書き戻す</summary>
    public void ApplyTo(AppConfig config)
    {
        config.SelectedGpuUuid = SelectedGpuUuid;
        config.AutoLimitEnabled = AutoLimitEnabled;
        config.AutoLimitThreshold = AutoLimitThreshold;
        config.CoreClockLimitEnabled = CoreClockLimitEnabled;
        config.CoreClockMaxMhz = CoreClockMaxMhz;
        config.PowerLimitWatts = PowerLimitWatts;
        config.RestoreDefaultOnUnlimit = RestoreDefaultOnUnlimit;
        config.RestoreToWatts = RestoreToWatts;
    }
}
