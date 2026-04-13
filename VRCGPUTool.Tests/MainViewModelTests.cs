using System.Reflection;
using FluentAssertions;
using Moq;
using VRCGPUTool.Models;
using VRCGPUTool.Services;
using VRCGPUTool.ViewModels;
using Xunit;

namespace VRCGPUTool.Tests;

public class MainViewModelTests
{
    // ─────────────────────────────────────────────────────────
    // ヘルパー
    // ─────────────────────────────────────────────────────────

    // 2026-04-13 (月曜) 22:00 UTC をテスト基準時刻とする
    private static readonly DateTimeOffset DefaultNow =
        new DateTimeOffset(2026, 4, 13, 22, 0, 0, TimeSpan.Zero);

    private static GpuStatus MakeGpu(
        string uuid = "gpu0",
        int powerLimit = 150,
        int powerDraw = 100,
        int temp = 65,
        int coreClock = 1800,
        int memClock = 7000,
        int util = 80)
        => new(Name: "RTX Test", Uuid: uuid,
               PowerLimit: powerLimit, PowerLimitMin: 50,
               PowerLimitMax: 350, PowerLimitDefault: 200,
               GpuUtilization: util, CoreTemperature: temp,
               PowerDraw: powerDraw, CoreClock: coreClock, MemoryClock: memClock);

    /// <summary>
    /// InvokeOnUiAsync がアクションを同期実行するモックを返す。
    /// テスト環境に WPF Dispatcher がないため必須。
    /// </summary>
    private static Mock<IApplicationHost> MakeSyncApplicationHost()
    {
        var host = new Mock<IApplicationHost>();
        host.Setup(h => h.InvokeOnUiAsync(It.IsAny<Action>()))
            .Returns<Action>(a => { a(); return Task.CompletedTask; });
        return host;
    }

    private static MainViewModel CreateVm(
        Mock<INvidiaSmiService>? nvidiaSmi = null,
        Mock<IConfigService>? configService = null,
        Mock<IPowerLogService>? powerLogService = null,
        Mock<IElectricityProfileService>? electricityService = null,
        Mock<IUpdateCheckService>? updateCheckService = null,
        Mock<IDialogService>? dialogService = null,
        Mock<IAutoLimitDetector>? autoLimitDetector = null,
        Mock<IApplicationHost>? applicationHost = null,
        Mock<INavigationService>? navigationService = null,
        TimeProvider? timeProvider = null)
    {
        nvidiaSmi ??= new Mock<INvidiaSmiService>();
        configService ??= new Mock<IConfigService>();
        powerLogService ??= new Mock<IPowerLogService>();
        electricityService ??= new Mock<IElectricityProfileService>();
        updateCheckService ??= new Mock<IUpdateCheckService>();
        dialogService ??= new Mock<IDialogService>();
        autoLimitDetector ??= new Mock<IAutoLimitDetector>();
        navigationService ??= new Mock<INavigationService>();
        applicationHost ??= MakeSyncApplicationHost();

        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>()))
                     .Returns(Task.CompletedTask);
        powerLogService.Setup(s => s.SaveAsync(It.IsAny<HourlyPowerLog>()))
                       .Returns(Task.CompletedTask);
        powerLogService.Setup(s => s.LoadForDateAsync(It.IsAny<DateOnly>()))
                       .ReturnsAsync(new HourlyPowerLog());
        electricityService.Setup(s => s.LoadAsync())
                          .ReturnsAsync(new ElectricityProfile());
        updateCheckService.Setup(s => s.CheckForUpdateAsync(It.IsAny<Version>(), default))
                          .ReturnsAsync((UpdateInfo?)null);

        return new MainViewModel(
            nvidiaSmi.Object,
            configService.Object,
            powerLogService.Object,
            electricityService.Object,
            updateCheckService.Object,
            autoLimitDetector.Object,
            dialogService.Object,
            applicationHost.Object,
            navigationService.Object,
            timeProvider ?? new TestTimeProvider(DefaultNow));
    }

    private static void SetField(MainViewModel vm, string name, object value)
        => typeof(MainViewModel)
               .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!
               .SetValue(vm, value);

    private static void CallProcessGpuUpdate(MainViewModel vm, IReadOnlyList<GpuStatus> gpus)
        => typeof(MainViewModel)
               .GetMethod("ProcessGpuUpdate", BindingFlags.NonPublic | BindingFlags.Instance)!
               .Invoke(vm, [gpus]);

    // ─────────────────────────────────────────────────────────
    // 初期状態
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_StatusText_IsInitializing()
        => CreateVm().StatusText.Should().Be("初期化中...");

    [Fact]
    public void Constructor_IsLimiting_IsFalse()
        => CreateVm().IsLimiting.Should().BeFalse();

    [Fact]
    public void Constructor_ScheduleSummaryText_IsNoSchedule()
        => CreateVm().ScheduleSummaryText.Should().Be("スケジュールなし");

    [Fact]
    public void Constructor_AvailableUpdate_IsNull()
        => CreateVm().AvailableUpdate.Should().BeNull();

    [Fact]
    public void Constructor_DisplayTexts_AreDefaultDashes()
    {
        var vm = CreateVm();
        vm.CoreTempText.Should().Be("-- °C");
        vm.PowerDrawText.Should().Be("-- W");
        vm.CurrentPlText.Should().Be("-- W");
        vm.CoreClockText.Should().Be("-- MHz");
        vm.MemClockText.Should().Be("-- MHz");
        vm.GpuUtilText.Should().Be("--%");
    }

    // ─────────────────────────────────────────────────────────
    // CanExecute 初期状態
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ApplyLimitCommand_CannotExecute_WhenNoGpus()
        => CreateVm().ApplyLimitCommand.CanExecute(null).Should().BeFalse();

    [Fact]
    public void RemoveLimitCommand_CannotExecute_Initially()
        => CreateVm().RemoveLimitCommand.CanExecute(null).Should().BeFalse();

    // ─────────────────────────────────────────────────────────
    // DismissUpdate
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void DismissUpdateCommand_ClearsAvailableUpdate()
    {
        var vm = CreateVm();
        vm.AvailableUpdate = new UpdateInfo("v2.0.0", "新機能追加");

        vm.DismissUpdateCommand.Execute(null);

        vm.AvailableUpdate.Should().BeNull();
    }

    // ─────────────────────────────────────────────────────────
    // DisposeAsync
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DisposeAsync_SavesConfig()
    {
        var configService = new Mock<IConfigService>();
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);
        var vm = CreateVm(configService: configService);

        await vm.DisposeAsync();

        configService.Verify(s => s.SaveAsync(It.IsAny<AppConfig>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DisposeAsync_SavesPowerLog()
    {
        var powerLogService = new Mock<IPowerLogService>();
        powerLogService.Setup(s => s.SaveAsync(It.IsAny<HourlyPowerLog>())).Returns(Task.CompletedTask);
        powerLogService.Setup(s => s.LoadForDateAsync(It.IsAny<DateOnly>())).ReturnsAsync(new HourlyPowerLog());
        var vm = CreateVm(powerLogService: powerLogService);

        await vm.DisposeAsync();

        powerLogService.Verify(s => s.SaveAsync(It.IsAny<HourlyPowerLog>()), Times.AtLeastOnce);
    }

    // ─────────────────────────────────────────────────────────
    // InitializeAsync
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task InitializeAsync_WhenNvidiaSmiUnavailable_ShowsErrorAndShutdown()
    {
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var dialogService = new Mock<IDialogService>();
        var appHost = MakeSyncApplicationHost();
        nvidiaSmi.Setup(s => s.IsAvailable()).Returns(false);

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, dialogService: dialogService, applicationHost: appHost);
        await vm.InitializeAsync();

        dialogService.Verify(d => d.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        appHost.Verify(h => h.Shutdown(), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WhenNoGpus_ShowsErrorAndShutdown()
    {
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var configService = new Mock<IConfigService>();
        var dialogService = new Mock<IDialogService>();
        var appHost = MakeSyncApplicationHost();

        nvidiaSmi.Setup(s => s.IsAvailable()).Returns(true);
        nvidiaSmi.Setup(s => s.QueryAllGpusAsync(default)).ReturnsAsync([]);
        configService.Setup(s => s.LoadAsync()).ReturnsAsync(new AppConfig());

        var vm = CreateVm(
            nvidiaSmi: nvidiaSmi, configService: configService,
            dialogService: dialogService, applicationHost: appHost);
        await vm.InitializeAsync();

        dialogService.Verify(d => d.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        appHost.Verify(h => h.Shutdown(), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_SelectsGpuBySavedUuid()
    {
        var gpu0 = MakeGpu("gpu0");
        var gpu1 = MakeGpu("gpu1");
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var configService = new Mock<IConfigService>();

        nvidiaSmi.Setup(s => s.IsAvailable()).Returns(true);
        nvidiaSmi.Setup(s => s.QueryAllGpusAsync(default))
                 .ReturnsAsync((IReadOnlyList<GpuStatus>)[gpu0, gpu1]);
        configService.Setup(s => s.LoadAsync())
                     .ReturnsAsync(new AppConfig { SelectedGpuUuid = "gpu1" });
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, configService: configService);
        await vm.InitializeAsync();

        // selectedGpuIndex が 1 になっていること (private フィールドで検証)
        var idx = (int)typeof(MainViewModel)
            .GetField("_selectedGpuIndex", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(vm)!;
        idx.Should().Be(1);
    }

    [Fact]
    public async Task InitializeAsync_WhenNoMatchingUuid_SelectsFirstGpu()
    {
        var gpu0 = MakeGpu("gpu0");
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var configService = new Mock<IConfigService>();

        nvidiaSmi.Setup(s => s.IsAvailable()).Returns(true);
        nvidiaSmi.Setup(s => s.QueryAllGpusAsync(default))
                 .ReturnsAsync((IReadOnlyList<GpuStatus>)[gpu0]);
        configService.Setup(s => s.LoadAsync())
                     .ReturnsAsync(new AppConfig { SelectedGpuUuid = "unknown-uuid" });
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, configService: configService);
        await vm.InitializeAsync();

        var idx = (int)typeof(MainViewModel)
            .GetField("_selectedGpuIndex", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(vm)!;
        idx.Should().Be(0);
    }

    [Fact]
    public async Task InitializeAsync_WhenScheduleActiveAtStartup_AppliesLimit()
    {
        // 2026-04-13 (月) 22:00 — 月曜 21:00〜23:00 のスケジュールがアクティブ
        var tp = new TestTimeProvider(DefaultNow);
        var gpu = MakeGpu("gpu0");
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var configService = new Mock<IConfigService>();

        nvidiaSmi.Setup(s => s.IsAvailable()).Returns(true);
        nvidiaSmi.Setup(s => s.QueryAllGpusAsync(default))
                 .ReturnsAsync((IReadOnlyList<GpuStatus>)[gpu]);
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);
        configService.Setup(s => s.LoadAsync())
                     .ReturnsAsync(new AppConfig
                     {
                         Schedules =
                         [
                             new ScheduleSlot
                             {
                                 Enabled = true,
                                 Days = ScheduleDays.Monday,
                                 StartHour = 21, StartMinute = 0,
                                 EndHour = 23,   EndMinute   = 0,
                             }
                         ]
                     });
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, configService: configService, timeProvider: tp);
        await vm.InitializeAsync();

        vm.IsLimiting.Should().BeTrue();
        nvidiaSmi.Verify(s => s.SetPowerLimitAsync("gpu0", It.IsAny<int>(), default), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WhenScheduleInactiveAtStartup_DoesNotApplyLimit()
    {
        // 22:00 だが スケジュールは 23:00〜01:00 → まだ始まっていない
        var tp = new TestTimeProvider(DefaultNow);
        var gpu = MakeGpu("gpu0");
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var configService = new Mock<IConfigService>();

        nvidiaSmi.Setup(s => s.IsAvailable()).Returns(true);
        nvidiaSmi.Setup(s => s.QueryAllGpusAsync(default))
                 .ReturnsAsync((IReadOnlyList<GpuStatus>)[gpu]);
        configService.Setup(s => s.LoadAsync())
                     .ReturnsAsync(new AppConfig
                     {
                         Schedules =
                         [
                             new ScheduleSlot
                             {
                                 Enabled = true,
                                 Days = ScheduleDays.Monday,
                                 StartHour = 23, StartMinute = 0,
                                 EndHour = 1,    EndMinute   = 0,
                             }
                         ]
                     });
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, configService: configService, timeProvider: tp);
        await vm.InitializeAsync();

        vm.IsLimiting.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // ProcessGpuUpdate — 表示プロパティ
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProcessGpuUpdate_UpdatesDisplayProperties()
    {
        var powerLogService = new Mock<IPowerLogService>();
        powerLogService.Setup(s => s.SaveAsync(It.IsAny<HourlyPowerLog>())).Returns(Task.CompletedTask);
        var vm = CreateVm(powerLogService: powerLogService);
        var gpu = MakeGpu("gpu0", powerLimit: 150, powerDraw: 120, temp: 72, coreClock: 1950, memClock: 8000, util: 95);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");

        CallProcessGpuUpdate(vm, [gpu]);

        vm.CoreTempText.Should().Be("72 °C");
        vm.PowerDrawText.Should().Be("120 W");
        vm.CurrentPlText.Should().Be("150 W");
        vm.CoreClockText.Should().Be("1950 MHz");
        vm.MemClockText.Should().Be("8000 MHz");
        vm.GpuUtilText.Should().Be("95 %");
    }

    // ─────────────────────────────────────────────────────────
    // ProcessGpuUpdate — GPU 切断
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProcessGpuUpdate_WhenGpuDisconnected_WhileLimiting_ClearsIsLimiting()
    {
        var vm = CreateVm();
        var gpu = MakeGpu("gpu0");
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_selectedGpuIndex", 0);
        vm.IsLimiting = true;

        CallProcessGpuUpdate(vm, []);

        vm.IsLimiting.Should().BeFalse();
        vm.StatusText.Should().Contain("GPUが切断");
    }

    // ─────────────────────────────────────────────────────────
    // ProcessGpuUpdate — 外部ツール変更検出
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProcessGpuUpdate_WhenExternalPowerLimitChange_ShowsWarning()
    {
        var dialogService = new Mock<IDialogService>();
        var powerLogService = new Mock<IPowerLogService>();
        powerLogService.Setup(s => s.SaveAsync(It.IsAny<HourlyPowerLog>())).Returns(Task.CompletedTask);

        var vm = CreateVm(powerLogService: powerLogService, dialogService: dialogService);
        var gpu = MakeGpu("gpu0", powerLimit: 200);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        vm.IsLimiting = true;
        SetField(vm, "_appliedPowerLimitWatts", 100); // 適用値(100) ≠ 現在値(200) → 外部変更

        CallProcessGpuUpdate(vm, [gpu]);

        dialogService.Verify(d => d.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        vm.IsLimiting.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // CheckSchedule — スケジュール開始トリガー
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProcessGpuUpdate_WhenScheduleStartTrigger_AndNotLimiting_AppliesLimit()
    {
        // 月曜 22:00 ちょうど → 22:00 開始スケジュールのトリガー
        var tp = new TestTimeProvider(DefaultNow); // 2026-04-13 Mon 22:00
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, timeProvider: tp);
        // powerLimit を _config.PowerLimitWatts (100W) に合わせて外部変更検出を回避する
        var gpu = MakeGpu("gpu0", powerLimit: 100);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_lastCheckedMinute", -1); // まだ未チェック → CheckSchedule が走る
        SetField(vm, "_config", new AppConfig
        {
            Schedules =
            [
                new ScheduleSlot
                {
                    Enabled = true,
                    Days = ScheduleDays.Monday,
                    StartHour = 22, StartMinute = 0,
                    EndHour   = 23, EndMinute   = 0,
                }
            ]
        });

        CallProcessGpuUpdate(vm, [gpu]);

        vm.IsLimiting.Should().BeTrue();
    }

    [Fact]
    public void ProcessGpuUpdate_WhenSameMinuteAsLastCheck_SkipsScheduleCheck()
    {
        var tp = new TestTimeProvider(DefaultNow); // 22:00 = 1320 分
        var nvidiaSmi = new Mock<INvidiaSmiService>();

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, timeProvider: tp);
        var gpu = MakeGpu("gpu0");
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_lastCheckedMinute", 22 * 60); // すでに今分をチェック済み
        SetField(vm, "_config", new AppConfig
        {
            Schedules =
            [
                new ScheduleSlot
                {
                    Enabled = true,
                    Days = ScheduleDays.Monday,
                    StartHour = 22, StartMinute = 0,
                    EndHour   = 23, EndMinute   = 0,
                }
            ]
        });

        CallProcessGpuUpdate(vm, [gpu]);

        vm.IsLimiting.Should().BeFalse(); // スキップされたので制限は入らない
        nvidiaSmi.Verify(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default), Times.Never);
    }

    // ─────────────────────────────────────────────────────────
    // CheckSchedule — スケジュール終了トリガー
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProcessGpuUpdate_WhenScheduleEndTrigger_AndLimiting_RemovesLimit()
    {
        // 月曜 23:00 ちょうど → 22:00〜23:00 スケジュールの終了トリガー
        var tp = new TestTimeProvider(new DateTimeOffset(2026, 4, 13, 23, 0, 0, TimeSpan.Zero));
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, timeProvider: tp);
        var gpu = MakeGpu("gpu0");
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_lastCheckedMinute", -1);
        SetField(vm, "_config", new AppConfig
        {
            Schedules =
            [
                new ScheduleSlot
                {
                    Enabled = true,
                    Days = ScheduleDays.Monday,
                    StartHour = 22, StartMinute = 0,
                    EndHour   = 23, EndMinute   = 0,
                }
            ]
        });
        SetField(vm, "_appliedPowerLimitWatts", 100);
        vm.IsLimiting = true;

        CallProcessGpuUpdate(vm, [gpu]);

        vm.IsLimiting.Should().BeFalse();
        vm.StatusText.Should().Contain("スケジュール");
    }

    [Fact]
    public void ProcessGpuUpdate_WhenScheduleEndTrigger_ButAnotherSlotActive_KeepsLimiting()
    {
        // 月曜 23:00 に slot1 が終了するが slot2 がまだアクティブ (23:00〜01:00)
        var tp = new TestTimeProvider(new DateTimeOffset(2026, 4, 13, 23, 0, 0, TimeSpan.Zero));
        var vm = CreateVm(timeProvider: tp);
        var gpu = MakeGpu("gpu0");
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_lastCheckedMinute", -1);
        // 外部変更検出を回避するため適用済み制限値を GPU の現在値に合わせる
        SetField(vm, "_appliedPowerLimitWatts", gpu.PowerLimit);
        SetField(vm, "_config", new AppConfig
        {
            Schedules =
            [
                new ScheduleSlot
                {
                    Enabled = true, Days = ScheduleDays.Monday,
                    StartHour = 22, StartMinute = 0,
                    EndHour   = 23, EndMinute   = 0, // 23:00 終了
                },
                new ScheduleSlot
                {
                    Enabled = true, Days = ScheduleDays.Monday,
                    StartHour = 23, StartMinute = 0, // 23:00 開始 → アクティブ
                    EndHour   = 1,  EndMinute   = 0,
                },
            ]
        });
        vm.IsLimiting = true;

        CallProcessGpuUpdate(vm, [gpu]);

        // slot2 がアクティブなので解除されない
        vm.IsLimiting.Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────
    // ProcessGpuUpdate — 自動制限
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProcessGpuUpdate_WhenAutoLimitEnabled_AndDetectorTriggered_AppliesLimit()
    {
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var autoDetector = new Mock<IAutoLimitDetector>();
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);
        autoDetector.Setup(d => d.Update(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, autoLimitDetector: autoDetector);
        // powerLimit を _config.PowerLimitWatts (100W) に合わせて外部変更検出を回避する
        var gpu = MakeGpu("gpu0", powerLimit: 100);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_config", new AppConfig { AutoLimitEnabled = true, AutoLimitThreshold = 10 });

        CallProcessGpuUpdate(vm, [gpu]);

        vm.IsLimiting.Should().BeTrue();
        vm.StatusText.Should().Contain("自動制限中");
    }

    [Fact]
    public void ProcessGpuUpdate_WhenAutoLimitEnabled_AndDetectorNotTriggered_DoesNotApplyLimit()
    {
        var autoDetector = new Mock<IAutoLimitDetector>();
        autoDetector.Setup(d => d.Update(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

        var vm = CreateVm(autoLimitDetector: autoDetector);
        var gpu = MakeGpu("gpu0");
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_config", new AppConfig { AutoLimitEnabled = true });

        CallProcessGpuUpdate(vm, [gpu]);

        vm.IsLimiting.Should().BeFalse();
    }

    [Fact]
    public void ProcessGpuUpdate_WhenAlreadyLimiting_AutoLimitDetectorNotCalled()
    {
        var autoDetector = new Mock<IAutoLimitDetector>();

        var vm = CreateVm(autoLimitDetector: autoDetector);
        var gpu = MakeGpu("gpu0");
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_config", new AppConfig { AutoLimitEnabled = true });
        SetField(vm, "_appliedPowerLimitWatts", gpu.PowerLimit); // 外部変更検出を無効化
        vm.IsLimiting = true;

        CallProcessGpuUpdate(vm, [gpu]);

        autoDetector.Verify(d => d.Update(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────
    // OpenSettings コマンド
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void OpenSettings_WhenCancelled_DoesNotModifyConfig()
    {
        var navService = new Mock<INavigationService>();
        var configService = new Mock<IConfigService>();
        navService.Setup(n => n.ShowSettingsDialog(
            It.IsAny<AppConfig>(), It.IsAny<IReadOnlyList<GpuStatus>>(),
            It.IsAny<string>(), It.IsAny<ElectricityProfile>()))
            .Returns((SettingsDialogResult?)null);

        var vm = CreateVm(navigationService: navService, configService: configService);

        vm.OpenSettingsCommand.Execute(null);

        configService.Verify(s => s.SaveAsync(It.IsAny<AppConfig>()), Times.Never);
    }

    [Fact]
    public void OpenSettings_WhenAccepted_UpdatesConfigAndSaves()
    {
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var navService = new Mock<INavigationService>();
        var configService = new Mock<IConfigService>();
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);

        var result = new SettingsDialogResult(
            SelectedGpuUuid: "gpu0",
            PowerLimitWatts: 120,
            RestoreDefaultOnUnlimit: true,
            RestoreToWatts: 0,
            AutoLimitEnabled: false,
            AutoLimitThreshold: 10,
            CoreClockLimitEnabled: false,
            CoreClockMaxMhz: 1500);
        navService.Setup(n => n.ShowSettingsDialog(
            It.IsAny<AppConfig>(), It.IsAny<IReadOnlyList<GpuStatus>>(),
            It.IsAny<string>(), It.IsAny<ElectricityProfile>()))
            .Returns(result);

        var gpu = MakeGpu("gpu0");
        var vm = CreateVm(nvidiaSmi: nvidiaSmi, navigationService: navService, configService: configService);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");

        vm.OpenSettingsCommand.Execute(null);

        var config = (AppConfig)typeof(MainViewModel)
            .GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(vm)!;
        config.PowerLimitWatts.Should().Be(120);
        configService.Verify(s => s.SaveAsync(It.IsAny<AppConfig>()), Times.Once);
    }

    [Fact]
    public void OpenSettings_WhenAcceptedWhileLimiting_ReAppliesLimit()
    {
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var navService = new Mock<INavigationService>();
        var configService = new Mock<IConfigService>();
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);

        var result = new SettingsDialogResult(
            "gpu0", PowerLimitWatts: 80,
            RestoreDefaultOnUnlimit: true, RestoreToWatts: 0,
            AutoLimitEnabled: false, AutoLimitThreshold: 10,
            CoreClockLimitEnabled: false, CoreClockMaxMhz: 1500);
        navService.Setup(n => n.ShowSettingsDialog(
            It.IsAny<AppConfig>(), It.IsAny<IReadOnlyList<GpuStatus>>(),
            It.IsAny<string>(), It.IsAny<ElectricityProfile>()))
            .Returns(result);

        var gpu = MakeGpu("gpu0");
        var vm = CreateVm(nvidiaSmi: nvidiaSmi, navigationService: navService, configService: configService);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        vm.IsLimiting = true;

        vm.OpenSettingsCommand.Execute(null);

        // 制限中なので即時再適用 → SetPowerLimitAsync が呼ばれる
        nvidiaSmi.Verify(s => s.SetPowerLimitAsync("gpu0", 80, default), Times.Once);
    }

    [Fact]
    public void OpenSettings_WhenAcceptedWhileNotLimiting_DoesNotCallSetPowerLimit()
    {
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var navService = new Mock<INavigationService>();
        var configService = new Mock<IConfigService>();
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);

        var result = new SettingsDialogResult(
            "gpu0", 100, true, 0, false, 10, false, 1500);
        navService.Setup(n => n.ShowSettingsDialog(
            It.IsAny<AppConfig>(), It.IsAny<IReadOnlyList<GpuStatus>>(),
            It.IsAny<string>(), It.IsAny<ElectricityProfile>()))
            .Returns(result);

        var gpu = MakeGpu("gpu0");
        var vm = CreateVm(nvidiaSmi: nvidiaSmi, navigationService: navService, configService: configService);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        // IsLimiting = false のまま

        vm.OpenSettingsCommand.Execute(null);

        nvidiaSmi.Verify(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default), Times.Never);
    }

    // ─────────────────────────────────────────────────────────
    // OpenScheduleSetting コマンド
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void OpenScheduleSetting_WhenCancelled_DoesNotModifySchedules()
    {
        var navService = new Mock<INavigationService>();
        var configService = new Mock<IConfigService>();
        navService.Setup(n => n.ShowScheduleDialog(It.IsAny<List<ScheduleSlot>>()))
                  .Returns((List<ScheduleSlot>?)null);

        var vm = CreateVm(navigationService: navService, configService: configService);

        vm.OpenScheduleSettingCommand.Execute(null);

        configService.Verify(s => s.SaveAsync(It.IsAny<AppConfig>()), Times.Never);
    }

    [Fact]
    public void OpenScheduleSetting_WhenAccepted_ShouldLimit_AppliesLimit()
    {
        // 月曜 22:00 → 21:00〜23:00 スケジュールがアクティブ → 制限を適用すべき
        var tp = new TestTimeProvider(DefaultNow);
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var navService = new Mock<INavigationService>();
        var configService = new Mock<IConfigService>();
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);

        var newSlots = new List<ScheduleSlot>
        {
            new() { Enabled = true, Days = ScheduleDays.Monday,
                    StartHour = 21, StartMinute = 0, EndHour = 23, EndMinute = 0 }
        };
        navService.Setup(n => n.ShowScheduleDialog(It.IsAny<List<ScheduleSlot>>()))
                  .Returns(newSlots);

        var gpu = MakeGpu("gpu0");
        var vm = CreateVm(nvidiaSmi: nvidiaSmi, navigationService: navService,
                           configService: configService, timeProvider: tp);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");

        vm.OpenScheduleSettingCommand.Execute(null);

        vm.IsLimiting.Should().BeTrue();
        nvidiaSmi.Verify(s => s.SetPowerLimitAsync("gpu0", It.IsAny<int>(), default), Times.Once);
    }

    [Fact]
    public void OpenScheduleSetting_WhenAccepted_WasScheduledAndNowNot_RemovesLimit()
    {
        // 月曜 23:00
        // 旧スケジュール: 22:00〜23:30 (23:00 時点でアクティブ) → wasScheduled = true
        // 新スケジュール: 22:00〜23:00 (23:00 = endMin → アクティブ外) → shouldLimit = false
        var tp = new TestTimeProvider(new DateTimeOffset(2026, 4, 13, 23, 0, 0, TimeSpan.Zero));
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var navService = new Mock<INavigationService>();
        var configService = new Mock<IConfigService>();
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);

        var newSlots = new List<ScheduleSlot>
        {
            new() { Enabled = true, Days = ScheduleDays.Monday,
                    StartHour = 22, StartMinute = 0, EndHour = 23, EndMinute = 0 }
        };
        navService.Setup(n => n.ShowScheduleDialog(It.IsAny<List<ScheduleSlot>>()))
                  .Returns(newSlots);

        var gpu = MakeGpu("gpu0");
        var vm = CreateVm(nvidiaSmi: nvidiaSmi, navigationService: navService,
                           configService: configService, timeProvider: tp);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        // 旧スケジュールを設定 (22:00〜23:30 → 23:00 時点でアクティブ)
        SetField(vm, "_config", new AppConfig
        {
            Schedules =
            [
                new ScheduleSlot
                {
                    Enabled = true, Days = ScheduleDays.Monday,
                    StartHour = 22, StartMinute = 0, EndHour = 23, EndMinute = 30
                }
            ]
        });
        SetField(vm, "_appliedPowerLimitWatts", 100);
        vm.IsLimiting = true;

        vm.OpenScheduleSettingCommand.Execute(null);

        vm.IsLimiting.Should().BeFalse();
        vm.StatusText.Should().Contain("スケジュール変更");
    }

    [Fact]
    public void OpenScheduleSetting_WhenAccepted_WasManualLimit_DoesNotRemoveLimit()
    {
        // スケジュール変更前もアクティブ外 → 手動制限の可能性があるため解除しない
        var tp = new TestTimeProvider(DefaultNow); // 月曜 22:00
        var navService = new Mock<INavigationService>();
        var configService = new Mock<IConfigService>();
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);

        // 新スケジュール: 23:00〜24:00 → 22:00 時点で非アクティブ
        var newSlots = new List<ScheduleSlot>
        {
            new() { Enabled = true, Days = ScheduleDays.Monday,
                    StartHour = 23, StartMinute = 0, EndHour = 0, EndMinute = 0 }
        };
        navService.Setup(n => n.ShowScheduleDialog(It.IsAny<List<ScheduleSlot>>()))
                  .Returns(newSlots);

        var gpu = MakeGpu("gpu0");
        var vm = CreateVm(navigationService: navService, configService: configService, timeProvider: tp);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        // 旧スケジュールも 23:00〜24:00 → 22:00 時点で非アクティブ (wasScheduled = false)
        SetField(vm, "_config", new AppConfig
        {
            Schedules =
            [
                new ScheduleSlot
                {
                    Enabled = true, Days = ScheduleDays.Monday,
                    StartHour = 23, StartMinute = 0, EndHour = 0, EndMinute = 0
                }
            ]
        });
        vm.IsLimiting = true; // 手動制限中

        vm.OpenScheduleSettingCommand.Execute(null);

        // wasScheduled = false なので解除しない
        vm.IsLimiting.Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────
    // ApplyLimitInternalAsync — 例外ハンドリング
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyLimitInternalAsync_WhenSetPowerLimitThrows_RevertsIsLimitingAndShowsError()
    {
        // InitializeAsync → スケジュールアクティブ → ApplyLimitInternalAsync が直接 await される
        var gpu = MakeGpu("gpu0");
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var configService = new Mock<IConfigService>();

        nvidiaSmi.Setup(s => s.IsAvailable()).Returns(true);
        nvidiaSmi.Setup(s => s.QueryAllGpusAsync(default)).ReturnsAsync((IReadOnlyList<GpuStatus>)[gpu]);
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .ThrowsAsync(new InvalidOperationException("設定失敗"));
        configService.Setup(s => s.LoadAsync()).ReturnsAsync(new AppConfig
        {
            Schedules =
            [
                new ScheduleSlot
                {
                    Enabled = true,
                    Days    = ScheduleDays.Monday,
                    StartHour = 21, StartMinute = 0,
                    EndHour   = 23, EndMinute   = 0,
                }
            ]
        });
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);

        var tp = new TestTimeProvider(DefaultNow); // 月曜 22:00 → アクティブ
        var vm = CreateVm(nvidiaSmi: nvidiaSmi, configService: configService, timeProvider: tp);
        await vm.InitializeAsync();

        vm.IsLimiting.Should().BeFalse();
        vm.StatusText.Should().Contain("制限の適用に失敗しました");
    }

    // ─────────────────────────────────────────────────────────
    // RemoveLimitInternalAsync — 例外ハンドリング
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void RemoveLimitInternalAsync_WhenSetPowerLimitThrows_RevertsIsLimitingAndShowsError()
    {
        // 月曜 23:00 → スケジュール終了トリガー → RemoveLimitInternalAsync が呼ばれる
        var tp = new TestTimeProvider(new DateTimeOffset(2026, 4, 13, 23, 0, 0, TimeSpan.Zero));
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .ThrowsAsync(new InvalidOperationException("解除失敗"));

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, timeProvider: tp);
        var gpu = MakeGpu("gpu0", powerLimit: 150);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_lastCheckedMinute", -1);
        // _appliedPowerLimitWatts を gpu.PowerLimit に合わせ外部変更検出を無効化
        SetField(vm, "_appliedPowerLimitWatts", 150);
        SetField(vm, "_config", new AppConfig
        {
            RestoreDefaultOnUnlimit = false,
            RestoreToWatts = 200,
            Schedules =
            [
                new ScheduleSlot
                {
                    Enabled = true,
                    Days    = ScheduleDays.Monday,
                    StartHour = 22, StartMinute = 0,
                    EndHour   = 23, EndMinute   = 0,
                }
            ]
        });
        vm.IsLimiting = true;

        CallProcessGpuUpdate(vm, [gpu]);

        vm.IsLimiting.Should().BeTrue();
        vm.StatusText.Should().Contain("制限解除に失敗しました");
    }

    // ─────────────────────────────────────────────────────────
    // ApplyLimitInternalAsync — CoreClockLimitEnabled
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ApplyLimitInternalAsync_WhenCoreClockLimitEnabled_CallsSetCoreClockLimit()
    {
        var tp = new TestTimeProvider(DefaultNow); // 月曜 22:00 → 開始トリガー
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);
        nvidiaSmi.Setup(s => s.SetCoreClockLimitAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, timeProvider: tp);
        // powerLimit を PowerLimitWatts(100) に合わせ、外部変更検出を回避
        var gpu = MakeGpu("gpu0", powerLimit: 100);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_lastCheckedMinute", -1);
        SetField(vm, "_config", new AppConfig
        {
            CoreClockLimitEnabled = true,
            CoreClockMaxMhz = 1200,
            Schedules =
            [
                new ScheduleSlot
                {
                    Enabled = true,
                    Days    = ScheduleDays.Monday,
                    StartHour = 22, StartMinute = 0,
                    EndHour   = 23, EndMinute   = 0,
                }
            ]
        });

        CallProcessGpuUpdate(vm, [gpu]);

        nvidiaSmi.Verify(s => s.SetCoreClockLimitAsync("gpu0", 200, 1200, default), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    // RemoveLimitInternalAsync — CoreClockLimitEnabled
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void RemoveLimitInternalAsync_WhenCoreClockLimitEnabled_CallsResetCoreClockLimit()
    {
        // 月曜 23:00 → 終了トリガー
        var tp = new TestTimeProvider(new DateTimeOffset(2026, 4, 13, 23, 0, 0, TimeSpan.Zero));
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);
        nvidiaSmi.Setup(s => s.ResetCoreClockLimitAsync(It.IsAny<string>(), default))
                 .Returns(Task.CompletedTask);

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, timeProvider: tp);
        var gpu = MakeGpu("gpu0", powerLimit: 150);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_lastCheckedMinute", -1);
        SetField(vm, "_appliedPowerLimitWatts", 150); // 外部変更検出を無効化
        SetField(vm, "_config", new AppConfig
        {
            CoreClockLimitEnabled = true,
            Schedules =
            [
                new ScheduleSlot
                {
                    Enabled = true,
                    Days    = ScheduleDays.Monday,
                    StartHour = 22, StartMinute = 0,
                    EndHour   = 23, EndMinute   = 0,
                }
            ]
        });
        vm.IsLimiting = true;

        CallProcessGpuUpdate(vm, [gpu]);

        nvidiaSmi.Verify(s => s.ResetCoreClockLimitAsync("gpu0", default), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    // RemoveLimitInternalAsync — restorePowerLimit=false（外部ツール変更時）
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProcessGpuUpdate_WhenExternalPowerLimitChange_DoesNotCallSetPowerLimit()
    {
        var dialogService = new Mock<IDialogService>();
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var powerLogService = new Mock<IPowerLogService>();
        powerLogService.Setup(s => s.SaveAsync(It.IsAny<HourlyPowerLog>())).Returns(Task.CompletedTask);

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, powerLogService: powerLogService, dialogService: dialogService);
        var gpu = MakeGpu("gpu0", powerLimit: 200);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_appliedPowerLimitWatts", 100); // 適用値(100) ≠ 現在値(200) → 外部変更検出
        vm.IsLimiting = true;

        CallProcessGpuUpdate(vm, [gpu]);

        // restorePowerLimit=false のため SetPowerLimitAsync は呼ばれない
        nvidiaSmi.Verify(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default), Times.Never);
    }

    // ─────────────────────────────────────────────────────────
    // RemoveLimitInternalAsync — RestoreDefaultOnUnlimit
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void RemoveLimitAsync_WhenRestoreDefaultOnUnlimit_True_RestoresToPowerLimitDefault()
    {
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);

        var gpu = MakeGpu("gpu0"); // PowerLimitDefault = 200
        var vm = CreateVm(nvidiaSmi: nvidiaSmi);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_config", new AppConfig { RestoreDefaultOnUnlimit = true });
        vm.IsLimiting = true;

        vm.RemoveLimitCommand.Execute(null);

        nvidiaSmi.Verify(s => s.SetPowerLimitAsync("gpu0", 200, default), Times.Once);
    }

    [Fact]
    public void RemoveLimitAsync_WhenRestoreDefaultOnUnlimit_False_RestoresToRestoreToWatts()
    {
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);

        var gpu = MakeGpu("gpu0");
        var vm = CreateVm(nvidiaSmi: nvidiaSmi);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_config", new AppConfig { RestoreDefaultOnUnlimit = false, RestoreToWatts = 120 });
        vm.IsLimiting = true;

        vm.RemoveLimitCommand.Execute(null);

        nvidiaSmi.Verify(s => s.SetPowerLimitAsync("gpu0", 120, default), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    // ProcessGpuUpdate — GPU切断 & IsLimiting=false
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProcessGpuUpdate_WhenGpuDisconnected_WhileNotLimiting_ResetsSelectedGpu()
    {
        var vm = CreateVm();
        var gpu0 = MakeGpu("gpu0");
        var gpu1 = MakeGpu("gpu1");
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu0]);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_selectedGpuIndex", 0);
        vm.IsLimiting = false;

        // gpu0 が消えて gpu1 だけになる
        CallProcessGpuUpdate(vm, [gpu1]);

        var uuid = (string)typeof(MainViewModel)
            .GetField("_selectedGpuUuid", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(vm)!;
        uuid.Should().Be("gpu1");
        vm.IsLimiting.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // ProcessGpuUpdate — GPU構成変更・選択UUID継続
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProcessGpuUpdate_WhenGpuListChanges_AndSelectedUuidStillPresent_UpdatesSelectedIndex()
    {
        var vm = CreateVm();
        var gpu0 = MakeGpu("gpu0");
        var gpu1 = MakeGpu("gpu1");

        // 初期: [gpu0]、選択 = gpu0 (index 0)
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu0]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");

        // 新リスト: [gpu1, gpu0] → gpu0 が index 1 に移動
        CallProcessGpuUpdate(vm, [gpu1, gpu0]);

        var idx = (int)typeof(MainViewModel)
            .GetField("_selectedGpuIndex", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(vm)!;
        idx.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────
    // ProcessGpuUpdate — 日付跨ぎログロールオーバー
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProcessGpuUpdate_WhenDayChanges_SavesOldLogAndCreatesNewLog()
    {
        var powerLogService = new Mock<IPowerLogService>();
        powerLogService.Setup(s => s.SaveAsync(It.IsAny<HourlyPowerLog>())).Returns(Task.CompletedTask);

        var tp = new TestTimeProvider(DefaultNow); // 2026-04-13
        var vm = CreateVm(powerLogService: powerLogService, timeProvider: tp);
        var gpu = MakeGpu("gpu0");
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");

        // 昨日の日付のログをセットしてロールオーバーを誘発
        var yesterday = new DateOnly(2026, 4, 12);
        SetField(vm, "_todayLog", new HourlyPowerLog { Date = yesterday });

        CallProcessGpuUpdate(vm, [gpu]);

        // 旧ログが保存されること
        powerLogService.Verify(s => s.SaveAsync(It.Is<HourlyPowerLog>(l => l.Date == yesterday)), Times.Once);

        // 新ログが今日の日付で作られていること
        var newLog = (HourlyPowerLog)typeof(MainViewModel)
            .GetField("_todayLog", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(vm)!;
        newLog.Date.Should().Be(new DateOnly(2026, 4, 13));
    }

    // ─────────────────────────────────────────────────────────
    // CheckForUpdateAsync — アップデートあり
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task InitializeAsync_WhenUpdateAvailable_SetsAvailableUpdate()
    {
        var gpu = MakeGpu("gpu0");
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var configService = new Mock<IConfigService>();
        var updateCheckService = new Mock<IUpdateCheckService>();

        nvidiaSmi.Setup(s => s.IsAvailable()).Returns(true);
        nvidiaSmi.Setup(s => s.QueryAllGpusAsync(default)).ReturnsAsync((IReadOnlyList<GpuStatus>)[gpu]);
        configService.Setup(s => s.LoadAsync()).ReturnsAsync(new AppConfig());
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);

        // CreateVm 内で Setup が null 返しで上書きされるため、VM 生成後に再設定する
        var vm = CreateVm(nvidiaSmi: nvidiaSmi, configService: configService,
                           updateCheckService: updateCheckService);
        updateCheckService.Setup(s => s.CheckForUpdateAsync(It.IsAny<Version>(), default))
                          .ReturnsAsync(new UpdateInfo("v99.0.0", "新機能"));

        await vm.InitializeAsync();

        vm.AvailableUpdate.Should().NotBeNull();
        vm.AvailableUpdate!.TagName.Should().Be("v99.0.0");
    }

    // ─────────────────────────────────────────────────────────
    // OpenReleasePage コマンド
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void OpenReleasePage_WhenUpdateAvailable_CallsOpenUrl()
    {
        var navService = new Mock<INavigationService>();
        var vm = CreateVm(navigationService: navService);
        vm.AvailableUpdate = new UpdateInfo("v2.0.0", "新機能");

        vm.OpenReleasePageCommand.Execute(null);

        navService.Verify(n => n.OpenUrl(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void OpenReleasePage_WhenNoUpdate_DoesNotCallOpenUrl()
    {
        var navService = new Mock<INavigationService>();
        var vm = CreateVm(navigationService: navService);
        vm.AvailableUpdate = null;

        vm.OpenReleasePageCommand.Execute(null);

        navService.Verify(n => n.OpenUrl(It.IsAny<string>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────
    // OpenPowerHistory コマンド
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void OpenPowerHistory_CallsShowOrActivatePowerHistory()
    {
        var navService = new Mock<INavigationService>();
        var vm = CreateVm(navigationService: navService);

        vm.OpenPowerHistoryCommand.Execute(null);

        navService.Verify(n => n.ShowOrActivatePowerHistory(
            It.IsAny<IPowerLogService>(),
            It.IsAny<Func<HourlyPowerLog>>(),
            It.IsAny<ElectricityProfile>()), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    // CanExecute — GPU存在・IsLimiting 状態変化
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ApplyLimitCommand_CanExecute_WhenGpuAvailableAndNotLimiting()
    {
        var vm = CreateVm();
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[MakeGpu("gpu0")]);
        vm.IsLimiting = false;

        vm.ApplyLimitCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void ApplyLimitCommand_CannotExecute_WhenLimiting()
    {
        var vm = CreateVm();
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[MakeGpu("gpu0")]);
        vm.IsLimiting = true;

        vm.ApplyLimitCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void RemoveLimitCommand_CanExecute_WhenGpuAvailableAndLimiting()
    {
        var vm = CreateVm();
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[MakeGpu("gpu0")]);
        vm.IsLimiting = true;

        vm.RemoveLimitCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void RemoveLimitCommand_CannotExecute_WhenNotLimiting()
    {
        var vm = CreateVm();
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[MakeGpu("gpu0")]);
        vm.IsLimiting = false;

        vm.RemoveLimitCommand.CanExecute(null).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // ① ApplyLimitCommand.Execute — 実行パス
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ApplyLimitCommand_WhenExecuted_CallsSetPowerLimitAndSetsIsLimiting()
    {
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);

        var gpu = MakeGpu("gpu0");
        var vm = CreateVm(nvidiaSmi: nvidiaSmi);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        // IsLimiting = false（デフォルト）

        vm.ApplyLimitCommand.Execute(null);

        vm.IsLimiting.Should().BeTrue();
        nvidiaSmi.Verify(s => s.SetPowerLimitAsync("gpu0", It.IsAny<int>(), default), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    // ② CheckSchedule — 開始と終了が同一分に発火、開始を優先
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProcessGpuUpdate_WhenStartAndEndTriggerSameMinute_AndNotLimiting_AppliesLimit()
    {
        // 月曜 22:00: slotA(21:00〜22:00)の終了と slotB(22:00〜23:00)の開始が同時発火
        // IsLimiting=false のとき開始が優先され制限が適用される
        var tp = new TestTimeProvider(new DateTimeOffset(2026, 4, 13, 22, 0, 0, TimeSpan.Zero));
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        nvidiaSmi.Setup(s => s.SetPowerLimitAsync(It.IsAny<string>(), It.IsAny<int>(), default))
                 .Returns(Task.CompletedTask);

        var vm = CreateVm(nvidiaSmi: nvidiaSmi, timeProvider: tp);
        var gpu = MakeGpu("gpu0", powerLimit: 100); // PowerLimitWatts デフォルト(100)に合わせ外部変更検出を回避
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_lastCheckedMinute", -1);
        SetField(vm, "_config", new AppConfig
        {
            Schedules =
            [
                new ScheduleSlot
                {
                    Enabled = true, Days = ScheduleDays.Monday,
                    StartHour = 21, StartMinute = 0,
                    EndHour   = 22, EndMinute   = 0, // 22:00 終了
                },
                new ScheduleSlot
                {
                    Enabled = true, Days = ScheduleDays.Monday,
                    StartHour = 22, StartMinute = 0, // 22:00 開始
                    EndHour   = 23, EndMinute   = 0,
                },
            ]
        });
        // IsLimiting = false（デフォルト）

        CallProcessGpuUpdate(vm, [gpu]);

        vm.IsLimiting.Should().BeTrue();
        nvidiaSmi.Verify(s => s.SetPowerLimitAsync("gpu0", It.IsAny<int>(), default), Times.Once);
    }

    // ─────────────────────────────────────────────────────────
    // ③ DisposeAsync — SelectedGpuUuid が config に書き込まれる
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DisposeAsync_SavesSelectedGpuUuidToConfig()
    {
        var configService = new Mock<IConfigService>();
        // CreateVm 内で SaveAsync の Setup が上書きされるため VM 生成後に再設定する
        var vm = CreateVm(configService: configService);

        AppConfig? saved = null;
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>()))
                     .Callback<AppConfig>(c => saved = c)
                     .Returns(Task.CompletedTask);

        SetField(vm, "_selectedGpuUuid", "gpu1");

        await vm.DisposeAsync();

        saved.Should().NotBeNull();
        saved!.SelectedGpuUuid.Should().Be("gpu1");
    }

    // ─────────────────────────────────────────────────────────
    // ④ ProcessGpuUpdate — GPU 全台切断で _selectedGpuUuid が空文字
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProcessGpuUpdate_WhenAllGpusDisconnected_SetsSelectedUuidToEmpty()
    {
        var vm = CreateVm();
        var gpu = MakeGpu("gpu0");
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_selectedGpuIndex", 0);
        vm.IsLimiting = false;

        CallProcessGpuUpdate(vm, []);

        var uuid = (string)typeof(MainViewModel)
            .GetField("_selectedGpuUuid", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(vm)!;
        uuid.Should().BeEmpty();
    }

    // ─────────────────────────────────────────────────────────
    // ⑤ ProcessGpuUpdate — _todayLog に PowerDraw が蓄積される
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void ProcessGpuUpdate_AccumulatesPowerDrawIntoTodayLog()
    {
        // DefaultNow = 2026-04-13 22:00 → hour=22 に蓄積されるはず
        var tp = new TestTimeProvider(DefaultNow);
        var vm = CreateVm(timeProvider: tp);
        var gpu = MakeGpu("gpu0", powerDraw: 120);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");

        // 今日の日付と一致するログをセット（ロールオーバーを起こさない）
        var todayLog = new HourlyPowerLog { Date = new DateOnly(2026, 4, 13) };
        SetField(vm, "_todayLog", todayLog);

        CallProcessGpuUpdate(vm, [gpu]);

        todayLog.HourlyWatts[22].Should().Be(120);
    }

    // ─────────────────────────────────────────────────────────
    // ⑥ OpenSettings — GPU UUID が変更されたとき _selectedGpuIndex が更新される
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void OpenSettings_WhenAccepted_ChangesGpuUuid_UpdatesSelectedGpuIndex()
    {
        var nvidiaSmi = new Mock<INvidiaSmiService>();
        var navService = new Mock<INavigationService>();
        var configService = new Mock<IConfigService>();
        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>())).Returns(Task.CompletedTask);

        var result = new SettingsDialogResult(
            SelectedGpuUuid: "gpu1", // gpu0 → gpu1 に変更
            PowerLimitWatts: 100,
            RestoreDefaultOnUnlimit: true, RestoreToWatts: 0,
            AutoLimitEnabled: false, AutoLimitThreshold: 10,
            CoreClockLimitEnabled: false, CoreClockMaxMhz: 1500);
        navService.Setup(n => n.ShowSettingsDialog(
            It.IsAny<AppConfig>(), It.IsAny<IReadOnlyList<GpuStatus>>(),
            It.IsAny<string>(), It.IsAny<ElectricityProfile>()))
            .Returns(result);

        var gpu0 = MakeGpu("gpu0");
        var gpu1 = MakeGpu("gpu1");
        var vm = CreateVm(nvidiaSmi: nvidiaSmi, navigationService: navService, configService: configService);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu0, gpu1]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");

        vm.OpenSettingsCommand.Execute(null);

        var idx = (int)typeof(MainViewModel)
            .GetField("_selectedGpuIndex", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(vm)!;
        idx.Should().Be(1); // gpu1 は index 1
    }
}
