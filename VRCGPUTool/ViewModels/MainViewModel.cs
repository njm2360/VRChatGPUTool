using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VRCGPUTool.Models;
using VRCGPUTool.Services;
using static VRCGPUTool.Services.ScheduleEvaluator;

namespace VRCGPUTool.ViewModels;

public sealed partial class MainViewModel(
    INvidiaSmiService nvidiaSmi,
    IConfigService configService,
    IPowerLogService powerLogService,
    IElectricityProfileService electricityProfileService,
    IUpdateCheckService updateCheckService,
    IAutoLimitDetector autoLimitDetector,
    IDialogService dialogService,
    IApplicationHost applicationHost,
    INavigationService navigationService,
    TimeProvider timeProvider) : ObservableObject, IAsyncDisposable
{
    private readonly INvidiaSmiService _nvidiaSmi = nvidiaSmi;
    private readonly IConfigService _configService = configService;
    private readonly IPowerLogService _powerLogService = powerLogService;
    private readonly IElectricityProfileService _electricityProfileService = electricityProfileService;
    private readonly IUpdateCheckService _updateCheckService = updateCheckService;
    private readonly IAutoLimitDetector _autoLimitDetector = autoLimitDetector;
    private readonly IDialogService _dialogService = dialogService;
    private readonly IApplicationHost _applicationHost = applicationHost;
    private readonly INavigationService _navigationService = navigationService;
    private readonly TimeProvider _timeProvider = timeProvider;

    private AppConfig _config = new();
    private HourlyPowerLog _todayLog = new();
    private ElectricityProfile _electricityProfile = new();

    private IReadOnlyList<GpuStatus> _gpus = [];
    private int _selectedGpuIndex;
    private string _selectedGpuUuid = string.Empty;

    private CancellationTokenSource _cts = new();
    private Task? _pollingTask;

    private int _lastCheckedMinute = -1;
    private int _appliedPowerLimitWatts;
    private bool _isApplyingLimit;
    private bool _externalChangeWarningPending;

    // ─────────────────────────────────────────
    // Observable Properties
    // ─────────────────────────────────────────

    [ObservableProperty] private string _scheduleSummaryText = "スケジュールなし";
    [ObservableProperty] private string _currentTimeText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyLimitCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveLimitCommand))]
    private bool _isLimiting;

    [ObservableProperty] private string _statusText = "初期化中...";
    [ObservableProperty] private string _coreTempText = "-- °C";
    [ObservableProperty] private string _powerDrawText = "-- W";
    [ObservableProperty] private string _currentPlText = "-- W";
    [ObservableProperty] private string _coreClockText = "-- MHz";
    [ObservableProperty] private string _memClockText = "-- MHz";
    [ObservableProperty] private string _gpuUtilText = "--%";
    [ObservableProperty] private UpdateInfo? _availableUpdate;

    private GpuStatus? GetSelectedGpu()
        => _selectedGpuIndex >= 0 && _selectedGpuIndex < _gpus.Count
            ? _gpus[_selectedGpuIndex]
            : null;

    private void SetSelectedGpuIndex(int value)
    {
        _selectedGpuIndex = value;
        _autoLimitDetector.Reset();
        if (value >= 0 && value < _gpus.Count)
            _selectedGpuUuid = _gpus[value].Uuid;
    }

    // ─────────────────────────────────────────
    // Initialization
    // ─────────────────────────────────────────

    public async Task InitializeAsync()
    {
        StatusText = "サービス接続確認中...";
        if (!_nvidiaSmi.IsAvailable())
        {
            _dialogService.ShowError(
                "Nvidia-Smi-Proxyに接続できませんでした。\n" +
                "サービスが起動しているか確認してください。");
            _applicationHost.Shutdown();
            return;
        }

        StatusText = "設定読み込み中...";
        _config = await _configService.LoadAsync().ConfigureAwait(true);
        _todayLog = await _powerLogService.LoadForDateAsync(
            DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime)).ConfigureAwait(true);
        _electricityProfile = await _electricityProfileService.LoadAsync().ConfigureAwait(true);

        StatusText = "GPU情報取得中...";
        var gpus = await _nvidiaSmi.QueryAllGpusAsync().ConfigureAwait(true);
        if (gpus.Count == 0)
        {
            _dialogService.ShowError("NVIDIA GPUが検出されませんでした。");
            _applicationHost.Shutdown();
            return;
        }

        _gpus = gpus;
        ApplyLimitCommand.NotifyCanExecuteChanged();
        RemoveLimitCommand.NotifyCanExecuteChanged();

        var savedUuid = _config.SelectedGpuUuid;
        var match = gpus.Select((g, i) => (g, i)).FirstOrDefault(t => t.g.Uuid == savedUuid);
        SetSelectedGpuIndex(match.g is not null ? match.i : 0);
        UpdateScheduleSummary();

        StatusText = "待機中";

        // 起動時にスケジュール範囲内なら即制限
        if (GetSelectedGpu() is { } gpuAtStart)
        {
            var now = _timeProvider.GetLocalNow().DateTime;
            if (_config.Schedules.Any(s => s.Enabled && IsSlotActiveNow(s, now)))
                await ApplyLimitInternalAsync(gpuAtStart).ConfigureAwait(true);
        }

        _ = CheckForUpdateAsync();

        _pollingTask = RunPollingLoopAsync(_cts.Token);
        _ = RunClockLoopAsync(_cts.Token);
    }

    // ─────────────────────────────────────────
    // Polling Loop
    // ─────────────────────────────────────────

    private async Task RunClockLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        do
        {
            var now = _timeProvider.GetLocalNow().DateTime;
            var text = $"{now:yyyy/MM/dd}({DateTimeLabels.DayLabel(now.DayOfWeek)}) {now:HH:mm:ss}";
            await _applicationHost.InvokeOnUiAsync(() => CurrentTimeText = text);
        }
        while (await timer.WaitForNextTickAsync(ct).ConfigureAwait(false));
    }

    private async Task RunPollingLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
        {
            try
            {
                var gpus = await _nvidiaSmi.QueryAllGpusAsync(ct).ConfigureAwait(false);
                await _applicationHost.InvokeOnUiAsync(() => ProcessGpuUpdate(gpus));
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { return; }
            catch (Exception ex)
            {
                await _applicationHost.InvokeOnUiAsync(() =>
                    StatusText = $"GPUの読み取りエラー: {ex.Message}");
            }
        }
    }

    private void ProcessGpuUpdate(IReadOnlyList<GpuStatus> gpus)
    {
        // GPUが増減したときのみリストを差し替える
        if (!gpus.Select(g => g.Uuid).SequenceEqual(_gpus.Select(g => g.Uuid)))
        {
            _gpus = gpus;
            ApplyLimitCommand.NotifyCanExecuteChanged();
            RemoveLimitCommand.NotifyCanExecuteChanged();

            var match = gpus.Select((g, i) => (g, i)).FirstOrDefault(t => t.g.Uuid == _selectedGpuUuid);
            if (match.g is not null)
            {
                _selectedGpuIndex = match.i;
                _autoLimitDetector.Reset();
            }
            else
            {
                // GPUが取り外されたケース
                if (IsLimiting)
                {
                    IsLimiting = false;
                    StatusText = "GPUが切断されたため制限を解除しました";
                    UpdateScheduleSummary();
                }

                _selectedGpuUuid = gpus.Count > 0 ? gpus[0].Uuid : string.Empty;
                _selectedGpuIndex = 0;
                _autoLimitDetector.Reset();
            }
        }

        if (_selectedGpuIndex < 0 || _selectedGpuIndex >= gpus.Count) return;
        var gpu = gpus[_selectedGpuIndex];

        // UI更新
        CoreTempText = $"{gpu.CoreTemperature} °C";
        PowerDrawText = $"{gpu.PowerDraw} W";
        CurrentPlText = $"{gpu.PowerLimit} W";
        CoreClockText = $"{gpu.CoreClock} MHz";
        MemClockText = $"{gpu.MemoryClock} MHz";
        GpuUtilText = $"{gpu.GpuUtilization} %";

        var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
        if (_todayLog.Date != today)
        {
            _ = _powerLogService.SaveAsync(_todayLog);
            _todayLog = new HourlyPowerLog { Date = today };
        }
        _todayLog.Accumulate(_timeProvider.GetLocalNow().DateTime.Hour, gpu.PowerDraw);

        CheckSchedule(gpu);

        // 自動制限判定 (制限中でないときのみ)
        if (!IsLimiting && _config.AutoLimitEnabled)
        {
            if (_autoLimitDetector.Update(gpu.GpuUtilization, _config.AutoLimitThreshold))
                _ = ApplyLimitInternalAsync(gpu, statusLabel: "自動制限中");
        }

        // 外部ツールによる制限変更検出
        if (IsLimiting && !_isApplyingLimit && !_externalChangeWarningPending && gpu.PowerLimit != _appliedPowerLimitWatts)
        {
            _externalChangeWarningPending = true;
            _ = RemoveLimitInternalAsync(gpu, reason: "外部ツールにより制限を解除しました", restorePowerLimit: false);

            var warningMessage =
                $"外部ツールによって電力制限値が変更されたため制限を終了しました。\n" +
                $"（設定値: {_config.PowerLimitWatts} W → 現在値: {gpu.PowerLimit} W）";
            _ = _applicationHost.InvokeOnUiAsync(() =>
            {
                try
                {
                    _dialogService.ShowWarning(warningMessage, "制限解除");
                }
                finally
                {
                    _externalChangeWarningPending = false;
                }
            });
        }
    }

    private void CheckSchedule(GpuStatus gpu)
    {
        var now = _timeProvider.GetLocalNow().DateTime;
        int currentMinutes = now.Hour * 60 + now.Minute;

        // スケジュール設定の最小は1分単位のため分が変わらない時はスキップ
        if (currentMinutes == _lastCheckedMinute) return;
        _lastCheckedMinute = currentMinutes;

        UpdateScheduleSummary();

        // 開始を終了より優先する（同時刻に開始と終了が重なる場合は開始を採用）
        var today = DayFlagOf(now.DayOfWeek);
        var yesterday = DayFlagOf(now.AddDays(-1).DayOfWeek);
        bool anyStart = _config.Schedules.Any(s => s.Enabled && IsStartTrigger(s, currentMinutes, today));

        if (anyStart && !IsLimiting)
        {
            _ = ApplyLimitInternalAsync(gpu);
            return;
        }

        if (!anyStart && IsLimiting)
        {
            bool anyEnd = _config.Schedules.Any(s => s.Enabled && IsEndTrigger(s, currentMinutes, today, yesterday));

            // 終了トリガーがあっても、他のスロットがまだアクティブなら解除しない
            if (anyEnd && !_config.Schedules.Any(s => s.Enabled && IsSlotActiveNow(s, now)))
                _ = RemoveLimitInternalAsync(gpu, reason: "スケジュールにより制限を解除しました");
        }
    }

    private void UpdateScheduleSummary()
        => ScheduleSummaryText = GetSummaryText(_config.Schedules, IsLimiting, _timeProvider.GetLocalNow().DateTime);

    // ─────────────────────────────────────────
    // Commands — Limit
    // ─────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanApplyLimit))]
    private async Task ApplyLimitAsync()
    {
        if (GetSelectedGpu() is not { } gpu) return;
        await ApplyLimitInternalAsync(gpu);
    }

    private bool CanApplyLimit() => !IsLimiting && _gpus.Count > 0;

    [RelayCommand(CanExecute = nameof(CanRemoveLimit))]
    private async Task RemoveLimitAsync()
    {
        if (GetSelectedGpu() is not { } gpu) return;
        await RemoveLimitInternalAsync(gpu, reason: null);
    }

    private bool CanRemoveLimit() => IsLimiting && _gpus.Count > 0;

    private async Task ApplyLimitInternalAsync(GpuStatus gpu, string statusLabel = "制限中")
    {
        int powerLimitWatts = Math.Clamp(_config.PowerLimitWatts, gpu.PowerLimitMin, gpu.PowerLimitMax);
        IsLimiting = true;
        _appliedPowerLimitWatts = powerLimitWatts;
        _isApplyingLimit = true;
        try
        {
            if (_config.CoreClockLimitEnabled)
                await _nvidiaSmi.SetCoreClockLimitAsync(gpu.Uuid, 200, _config.CoreClockMaxMhz).ConfigureAwait(true);

            await _nvidiaSmi.SetPowerLimitAsync(gpu.Uuid, powerLimitWatts).ConfigureAwait(true);

            StatusText = $"{statusLabel} ({powerLimitWatts} W)";
            UpdateScheduleSummary();
        }
        catch (Exception ex)
        {
            IsLimiting = false;
            _appliedPowerLimitWatts = 0;
            StatusText = $"制限の適用に失敗しました: {ex.Message}";
        }
        finally
        {
            _isApplyingLimit = false;
        }
    }

    private async Task RemoveLimitInternalAsync(GpuStatus gpu, string? reason, bool restorePowerLimit = true)
    {
        IsLimiting = false;
        try
        {
            if (_config.CoreClockLimitEnabled)
                await _nvidiaSmi.ResetCoreClockLimitAsync(gpu.Uuid).ConfigureAwait(true);

            if (restorePowerLimit)
            {
                int restoreWatts = Math.Clamp(
                    _config.RestoreDefaultOnUnlimit ? gpu.PowerLimitDefault : _config.RestoreToWatts,
                    gpu.PowerLimitMin, gpu.PowerLimitMax);
                await _nvidiaSmi.SetPowerLimitAsync(gpu.Uuid, restoreWatts).ConfigureAwait(true);
            }

            _autoLimitDetector.Reset();
            StatusText = reason ?? "制限を解除しました";
            UpdateScheduleSummary();
        }
        catch (Exception ex)
        {
            IsLimiting = true;
            StatusText = $"制限解除に失敗しました: {ex.Message}";
        }
    }

    // ─────────────────────────────────────────
    // Commands — Navigation
    // ─────────────────────────────────────────

    [RelayCommand]
    private void OpenSettings()
    {
        var result = _navigationService.ShowSettingsDialog(_config, _gpus, _selectedGpuUuid, _electricityProfile);
        if (result is null) return;

        result.ApplyTo(_config);
        var match = _gpus.Select((g, i) => (g, i)).FirstOrDefault(t => t.g.Uuid == _config.SelectedGpuUuid);
        SetSelectedGpuIndex(match.g is not null ? match.i : 0);
        _ = _configService.SaveAsync(_config);

        // 制限中に制限値が変更された場合は即時再適用
        if (IsLimiting && GetSelectedGpu() is { } gpu)
            _ = ApplyLimitInternalAsync(gpu);
    }

    [RelayCommand]
    private void OpenScheduleSetting()
    {
        var now = _timeProvider.GetLocalNow().DateTime;
        // 変更前のスケジュールで現在時刻がアクティブかどうか
        bool wasScheduled = _config.Schedules.Any(s => s.Enabled && IsSlotActiveNow(s, now));

        var newSlots = _navigationService.ShowScheduleDialog(_config.Schedules);
        if (newSlots is null) return;

        _config.Schedules = newSlots;
        UpdateScheduleSummary();
        _ = _configService.SaveAsync(_config);

        // 変更後のスケジュールで現在時刻がアクティブかどうか
        bool shouldLimit = _config.Schedules.Any(s => s.Enabled && IsSlotActiveNow(s, now));

        if (shouldLimit && !IsLimiting && GetSelectedGpu() is { } gpuToLimit)
            _ = ApplyLimitInternalAsync(gpuToLimit);
        // 変更前もスケジュール範囲外だった場合は手動制限の可能性があるため解除しない
        else if (!shouldLimit && wasScheduled && IsLimiting && GetSelectedGpu() is { } gpuToRelease)
            _ = RemoveLimitInternalAsync(gpuToRelease, reason: "スケジュール変更により制限を解除しました");
    }

    [RelayCommand]
    private void OpenPowerHistory()
        => _navigationService.ShowOrActivatePowerHistory(_powerLogService, () => _todayLog, _electricityProfile);

    [RelayCommand]
    private void OpenReleasePage()
    {
        if (AvailableUpdate is null) return;
        _navigationService.OpenUrl("https://github.com/njm2360/VRChatGPUTool/releases/latest");
    }

    [RelayCommand]
    private void DismissUpdate() => AvailableUpdate = null;

    // ─────────────────────────────────────────
    // Update Check
    // ─────────────────────────────────────────

    private async Task CheckForUpdateAsync()
    {
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);
        var update = await _updateCheckService.CheckForUpdateAsync(currentVersion).ConfigureAwait(true);
        AvailableUpdate = update;
    }

    // ─────────────────────────────────────────
    // Shutdown
    // ─────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();

        if (_pollingTask is not null)
            await _pollingTask.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        _cts.Dispose();

        // 設定を保存
        _config.SelectedGpuUuid = _selectedGpuUuid;

        await _configService.SaveAsync(_config).ConfigureAwait(false);
        await _powerLogService.SaveAsync(_todayLog).ConfigureAwait(false);
    }
}
