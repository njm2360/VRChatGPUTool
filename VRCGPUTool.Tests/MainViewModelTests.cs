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

    private static MainViewModel CreateVm(
        Mock<INvidiaSmiService>?          nvidiaSmi              = null,
        Mock<IConfigService>?             configService          = null,
        Mock<IPowerLogService>?           powerLogService        = null,
        Mock<IElectricityProfileService>? electricityService     = null,
        Mock<IUpdateCheckService>?        updateCheckService     = null,
        Mock<IDialogService>?             dialogService          = null)
    {
        nvidiaSmi          ??= new Mock<INvidiaSmiService>();
        configService      ??= new Mock<IConfigService>();
        powerLogService    ??= new Mock<IPowerLogService>();
        electricityService ??= new Mock<IElectricityProfileService>();
        updateCheckService ??= new Mock<IUpdateCheckService>();
        dialogService      ??= new Mock<IDialogService>();

        configService.Setup(s => s.SaveAsync(It.IsAny<AppConfig>()))
                     .Returns(Task.CompletedTask);
        powerLogService.Setup(s => s.SaveAsync(It.IsAny<HourlyPowerLog>()))
                       .Returns(Task.CompletedTask);

        return new MainViewModel(
            nvidiaSmi.Object,
            configService.Object,
            powerLogService.Object,
            electricityService.Object,
            updateCheckService.Object,
            new AutoLimitDetector(),
            new StartupService(),
            dialogService.Object);
    }

    /// <summary>
    /// privateフィールドを反射で書き換える。
    /// ProcessGpuUpdate は WPF 非依存の private メソッドであり、
    /// 内部状態を制御するために使用する。
    /// </summary>
    private static void SetField(MainViewModel vm, string name, object value)
        => typeof(MainViewModel)
               .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!
               .SetValue(vm, value);

    /// <summary>ProcessGpuUpdate を直接呼び出す。</summary>
    private static void CallProcessGpuUpdate(MainViewModel vm, IReadOnlyList<GpuStatus> gpus)
        => typeof(MainViewModel)
               .GetMethod("ProcessGpuUpdate", BindingFlags.NonPublic | BindingFlags.Instance)!
               .Invoke(vm, [gpus]);

    // ─────────────────────────────────────────────────────────
    // 初期状態
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_StatusText_IsInitializing()
    {
        var vm = CreateVm();
        vm.StatusText.Should().Be("初期化中...");
    }

    [Fact]
    public void Constructor_IsLimiting_IsFalse()
    {
        var vm = CreateVm();
        vm.IsLimiting.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ScheduleSummaryText_IsNoSchedule()
    {
        var vm = CreateVm();
        vm.ScheduleSummaryText.Should().Be("スケジュールなし");
    }

    [Fact]
    public void Constructor_AvailableUpdate_IsNull()
    {
        var vm = CreateVm();
        vm.AvailableUpdate.Should().BeNull();
    }

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

    // GPU リストが空のため ApplyLimit は実行不可
    [Fact]
    public void ApplyLimitCommand_CannotExecute_WhenNoGpus()
    {
        var vm = CreateVm();
        vm.ApplyLimitCommand.CanExecute(null).Should().BeFalse();
    }

    // 制限中でない＆GPU なし のため RemoveLimit も実行不可
    [Fact]
    public void RemoveLimitCommand_CannotExecute_Initially()
    {
        var vm = CreateVm();
        vm.RemoveLimitCommand.CanExecute(null).Should().BeFalse();
    }

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
        var vm = CreateVm(powerLogService: powerLogService);

        await vm.DisposeAsync();

        powerLogService.Verify(s => s.SaveAsync(It.IsAny<HourlyPowerLog>()), Times.AtLeastOnce);
    }

    // ─────────────────────────────────────────────────────────
    // ProcessGpuUpdate (reflection 経由)
    // ─────────────────────────────────────────────────────────
    // ※ このメソッドは WPF 非依存の private void メソッド。
    //   ポーリングループからは Dispatcher.InvokeAsync 越しに呼ばれるが、
    //   メソッド自体に WPF 依存はないため直接テスト可能。

    // GPU 情報が正しく表示プロパティへ反映される
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

    // 制限中に GPU が切断されると IsLimiting が解除され StatusText が更新される
    [Fact]
    public void ProcessGpuUpdate_WhenGpuDisconnected_WhileLimiting_ClearsIsLimiting()
    {
        var vm = CreateVm();
        var gpu = MakeGpu("gpu0");
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        SetField(vm, "_selectedGpuIndex", 0);
        vm.IsLimiting = true;

        CallProcessGpuUpdate(vm, []); // GPU リストが空 = 切断

        vm.IsLimiting.Should().BeFalse();
        vm.StatusText.Should().Contain("GPUが切断");
    }

    // 外部ツールによる電力制限変更を検出すると警告ダイアログを表示する
    [Fact]
    public void ProcessGpuUpdate_WhenExternalPowerLimitChange_ShowsWarning()
    {
        var dialogService  = new Mock<IDialogService>();
        var powerLogService = new Mock<IPowerLogService>();
        powerLogService.Setup(s => s.SaveAsync(It.IsAny<HourlyPowerLog>())).Returns(Task.CompletedTask);
        var vm = CreateVm(powerLogService: powerLogService, dialogService: dialogService);

        // GPU の現在 PowerLimit を 200 W に設定
        var gpu = MakeGpu("gpu0", powerLimit: 200);
        SetField(vm, "_gpus", (IReadOnlyList<GpuStatus>)[gpu]);
        SetField(vm, "_selectedGpuIndex", 0);
        SetField(vm, "_selectedGpuUuid", "gpu0");
        vm.IsLimiting = true;
        SetField(vm, "_appliedPowerLimitWatts", 100); // 適用値(100) != 現在値(200) → 外部変更

        CallProcessGpuUpdate(vm, [gpu]);

        dialogService.Verify(d => d.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        vm.IsLimiting.Should().BeFalse();
    }
}
