using FluentAssertions;
using Moq;
using VRCGPUTool.Models;
using VRCGPUTool.Services;
using VRCGPUTool.ViewModels;
using Xunit;

namespace VRCGPUTool.Tests;

public class SettingsViewModelTests
{
    // ─────────────────────────────────────────────────────────
    // ヘルパー
    // ─────────────────────────────────────────────────────────

    private static readonly GpuStatus GpuA =
        new("RTX 3080", "GPU-AAA", 200, 100, 350, 200, 0, 0, 0, 0, 0);
    private static readonly GpuStatus GpuB =
        new("RTX 3090", "GPU-BBB", 300, 150, 450, 300, 0, 0, 0, 0, 0);

    private static SettingsViewModel CreateViewModel(
        AppConfig? config = null,
        string selectedUuid = "",
        IReadOnlyList<GpuStatus>? gpus = null,
        Mock<IDialogService>? dialogMock = null)
    {
        config ??= new AppConfig
        {
            PowerLimitWatts = 200,
            RestoreToWatts = 100,
            AutoLimitEnabled = false,
            AutoLimitThreshold = 10,
            CoreClockLimitEnabled = false,
            CoreClockMaxMhz = 1500,
            RestoreDefaultOnUnlimit = true,
        };
        gpus ??= [GpuA, GpuB];

        return new SettingsViewModel(
            config,
            new StartupService(),
            new Mock<IElectricityProfileService>().Object,
            new ElectricityProfile(),
            gpus,
            selectedUuid,
            (dialogMock ?? new Mock<IDialogService>()).Object);
    }

    // ─────────────────────────────────────────────────────────
    // コンストラクタ: GPU 選択
    // ─────────────────────────────────────────────────────────

    // 指定した UUID に一致する GPU が選択される
    [Fact]
    public void Constructor_SelectsGpuByUuid()
    {
        var vm = CreateViewModel(selectedUuid: GpuB.Uuid);

        vm.SelectedGpuIndex.Should().Be(1);
        vm.SelectedGpuUuid.Should().Be(GpuB.Uuid);
    }

    // UUID が一致しない場合はインデックス 0 にフォールバック
    [Fact]
    public void Constructor_FallsBackToIndex0_WhenUuidNotFound()
    {
        var vm = CreateViewModel(selectedUuid: "GPU-UNKNOWN");

        vm.SelectedGpuIndex.Should().Be(0);
        vm.SelectedGpuUuid.Should().Be(GpuA.Uuid);
    }

    // UUID が空文字のときもインデックス 0 を選択
    [Fact]
    public void Constructor_EmptyUuid_SelectsIndex0()
    {
        var vm = CreateViewModel(selectedUuid: "");

        vm.SelectedGpuIndex.Should().Be(0);
    }

    // GPU リストが空のときはクラッシュせず SelectedGpuUuid が空文字
    [Fact]
    public void Constructor_EmptyGpuList_DoesNotThrow_AndUuidIsEmpty()
    {
        var act = () => CreateViewModel(gpus: []);

        act.Should().NotThrow();
        var vm = act();
        vm.SelectedGpuUuid.Should().BeEmpty();
    }

    // ─────────────────────────────────────────────────────────
    // コンストラクタ: PowerLimitWatts クランプ
    // ─────────────────────────────────────────────────────────

    // GPU の min/max 範囲内なら値がそのまま使われる
    [Fact]
    public void Constructor_PowerLimitWithinRange_KeepsValue()
    {
        var config = new AppConfig { PowerLimitWatts = 200 }; // GpuA: min=100, max=350
        var vm = CreateViewModel(config, GpuA.Uuid);

        vm.PowerLimitWatts.Should().Be(200);
    }

    // 下限未満は min にクランプされる
    [Fact]
    public void Constructor_PowerLimitBelowMin_ClampsToMin()
    {
        var config = new AppConfig { PowerLimitWatts = 50 }; // GpuA.min = 100
        var vm = CreateViewModel(config, GpuA.Uuid);

        vm.PowerLimitWatts.Should().Be(GpuA.PowerLimitMin);
    }

    // 上限超過は max にクランプされる
    [Fact]
    public void Constructor_PowerLimitAboveMax_ClampsToMax()
    {
        var config = new AppConfig { PowerLimitWatts = 999 }; // GpuA.max = 350
        var vm = CreateViewModel(config, GpuA.Uuid);

        vm.PowerLimitWatts.Should().Be(GpuA.PowerLimitMax);
    }

    // ─────────────────────────────────────────────────────────
    // コンフィグ設定の読み込み
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ReadsConfigProperties()
    {
        var config = new AppConfig
        {
            AutoLimitEnabled = true,
            AutoLimitThreshold = 15,
            CoreClockLimitEnabled = true,
            CoreClockMaxMhz = 2000,
            RestoreDefaultOnUnlimit = false,
        };
        var vm = CreateViewModel(config);

        vm.AutoLimitEnabled.Should().BeTrue();
        vm.AutoLimitThreshold.Should().Be(15);
        vm.CoreClockLimitEnabled.Should().BeTrue();
        vm.CoreClockMaxMhz.Should().Be(2000);
        vm.RestoreDefaultOnUnlimit.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────
    // PowerLimitWatts の動的クランプ
    // ─────────────────────────────────────────────────────────

    // 上限を超える値を設定すると max にクランプされる
    [Fact]
    public void PowerLimitWatts_AboveMax_ClampsToMax()
    {
        var vm = CreateViewModel(selectedUuid: GpuA.Uuid); // max=350

        vm.PowerLimitWatts = 9999;

        vm.PowerLimitWatts.Should().Be(GpuA.PowerLimitMax);
    }

    // 下限未満の値を設定すると min にクランプされる
    [Fact]
    public void PowerLimitWatts_BelowMin_ClampsToMin()
    {
        var vm = CreateViewModel(selectedUuid: GpuA.Uuid); // min=100

        vm.PowerLimitWatts = 1;

        vm.PowerLimitWatts.Should().Be(GpuA.PowerLimitMin);
    }

    // ─────────────────────────────────────────────────────────
    // SelectedGpuIndex 変更時の再クランプ
    // ─────────────────────────────────────────────────────────

    // GPU を切り替えると PowerLimitWatts が新 GPU の min/max に再クランプされる
    [Fact]
    public void SelectedGpuIndex_Change_ReclampsPowerLimitWatts()
    {
        // GpuA: min=100, max=350 / GpuB: min=150, max=450
        var config = new AppConfig { PowerLimitWatts = 120 }; // GpuA 範囲内だが GpuB.min(150) 未満
        var vm = CreateViewModel(config, selectedUuid: GpuA.Uuid);

        vm.SelectedGpuIndex = 1; // → GpuB に切り替え

        vm.PowerLimitWatts.Should().Be(GpuB.PowerLimitMin); // 150 にクランプ
    }

    // ─────────────────────────────────────────────────────────
    // LoadMinWatts / LoadDefaultWatts / LoadMaxWatts コマンド
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void LoadMinWattsCommand_SetsPowerLimitToGpuMin()
    {
        var vm = CreateViewModel(selectedUuid: GpuA.Uuid);
        vm.LoadMinWattsCommand.Execute(null);

        vm.PowerLimitWatts.Should().Be(GpuA.PowerLimitMin);
    }

    [Fact]
    public void LoadDefaultWattsCommand_SetsPowerLimitToGpuDefault()
    {
        var vm = CreateViewModel(selectedUuid: GpuA.Uuid);
        vm.LoadDefaultWattsCommand.Execute(null);

        vm.PowerLimitWatts.Should().Be(GpuA.PowerLimitDefault);
    }

    [Fact]
    public void LoadMaxWattsCommand_SetsPowerLimitToGpuMax()
    {
        var vm = CreateViewModel(selectedUuid: GpuA.Uuid);
        vm.LoadMaxWattsCommand.Execute(null);

        vm.PowerLimitWatts.Should().Be(GpuA.PowerLimitMax);
    }

    // ─────────────────────────────────────────────────────────
    // コンストラクタ: RestoreToWatts クランプ
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_RestoreToWattsBelowMin_ClampsToMin()
    {
        var config = new AppConfig { RestoreToWatts = 50 }; // GpuA.min = 100
        var vm = CreateViewModel(config, GpuA.Uuid);

        vm.RestoreToWatts.Should().Be(GpuA.PowerLimitMin);
    }

    [Fact]
    public void Constructor_RestoreToWattsAboveMax_ClampsToMax()
    {
        var config = new AppConfig { RestoreToWatts = 999 }; // GpuA.max = 350
        var vm = CreateViewModel(config, GpuA.Uuid);

        vm.RestoreToWatts.Should().Be(GpuA.PowerLimitMax);
    }

    // ─────────────────────────────────────────────────────────
    // RestoreToWatts の動的クランプ
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void RestoreToWatts_AboveMax_ClampsToMax()
    {
        var vm = CreateViewModel(selectedUuid: GpuA.Uuid); // max=350

        vm.RestoreToWatts = 9999;

        vm.RestoreToWatts.Should().Be(GpuA.PowerLimitMax);
    }

    [Fact]
    public void RestoreToWatts_BelowMin_ClampsToMin()
    {
        var vm = CreateViewModel(selectedUuid: GpuA.Uuid); // min=100

        vm.RestoreToWatts = 1;

        vm.RestoreToWatts.Should().Be(GpuA.PowerLimitMin);
    }

    // ─────────────────────────────────────────────────────────
    // SelectedGpuIndex 変更時の RestoreToWatts 再クランプ
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void SelectedGpuIndex_Change_ReclampsRestoreToWatts()
    {
        // GpuA: min=100, max=350 / GpuB: min=150, max=450
        var config = new AppConfig { RestoreToWatts = 120 }; // GpuA 範囲内だが GpuB.min(150) 未満
        var vm = CreateViewModel(config, selectedUuid: GpuA.Uuid);

        vm.SelectedGpuIndex = 1; // → GpuB に切り替え

        vm.RestoreToWatts.Should().Be(GpuB.PowerLimitMin); // 150 にクランプ
    }

    // ─────────────────────────────────────────────────────────
    // ApplyTo
    // ─────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────
    // SaveCommand — バリデーション
    // ─────────────────────────────────────────────────────────

    // AutoLimitEnabled=true かつ Threshold が下限(0)のとき ShowWarning が呼ばれる
    [Fact]
    public void SaveCommand_ThresholdTooLow_CallsShowWarning()
    {
        var dialog = new Mock<IDialogService>();
        var config = new AppConfig { AutoLimitEnabled = true, AutoLimitThreshold = 0 };
        var vm = CreateViewModel(config, dialogMock: dialog);

        vm.SaveCommand.Execute(null!);

        dialog.Verify(d => d.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    // AutoLimitEnabled=true かつ Threshold が上限(100)のとき ShowWarning が呼ばれる
    [Fact]
    public void SaveCommand_ThresholdTooHigh_CallsShowWarning()
    {
        var dialog = new Mock<IDialogService>();
        var config = new AppConfig { AutoLimitEnabled = true, AutoLimitThreshold = 100 };
        var vm = CreateViewModel(config, dialogMock: dialog);

        vm.SaveCommand.Execute(null!);

        dialog.Verify(d => d.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    // AutoLimitEnabled=true かつ Threshold=1（下限境界値）のとき ShowWarning は呼ばれない
    // バリデーションを通過するため window.Close() で NPE が発生するが、その前に ShowWarning が
    // 呼ばれないことを検証する（Window は WPF クラスのためテスト環境では null を渡す）
    [Fact]
    public void SaveCommand_ThresholdAtLowerBound_DoesNotCallShowWarning()
    {
        var dialog = new Mock<IDialogService>();
        var config = new AppConfig { AutoLimitEnabled = true, AutoLimitThreshold = 1 };
        var vm = CreateViewModel(config, selectedUuid: GpuA.Uuid, dialogMock: dialog);

        try { vm.SaveCommand.Execute(null!); } catch { /* window が null のため NPE は想定内 */ }

        dialog.Verify(d => d.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // AutoLimitEnabled=true かつ Threshold=99（上限境界値）のとき ShowWarning は呼ばれない
    [Fact]
    public void SaveCommand_ThresholdAtUpperBound_DoesNotCallShowWarning()
    {
        var dialog = new Mock<IDialogService>();
        var config = new AppConfig { AutoLimitEnabled = true, AutoLimitThreshold = 99 };
        var vm = CreateViewModel(config, selectedUuid: GpuA.Uuid, dialogMock: dialog);

        try { vm.SaveCommand.Execute(null!); } catch { /* window が null のため NPE は想定内 */ }

        dialog.Verify(d => d.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // AutoLimitEnabled=false のとき Threshold が範囲外でも ShowWarning は呼ばれない
    [Fact]
    public void SaveCommand_AutoLimitDisabled_InvalidThreshold_DoesNotCallShowWarning()
    {
        var dialog = new Mock<IDialogService>();
        var config = new AppConfig { AutoLimitEnabled = false, AutoLimitThreshold = 0 };
        var vm = CreateViewModel(config, selectedUuid: GpuA.Uuid, dialogMock: dialog);

        try { vm.SaveCommand.Execute(null!); } catch { /* window が null のため NPE は想定内 */ }

        dialog.Verify(d => d.ShowWarning(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────
    // ApplyTo
    // ─────────────────────────────────────────────────────────

    // ApplyTo は ViewModel の現在値をすべて AppConfig に書き戻す
    [Fact]
    public void ApplyTo_WritesAllValuesToConfig()
    {
        var vm = CreateViewModel(selectedUuid: GpuA.Uuid);
        vm.AutoLimitEnabled = true;
        vm.AutoLimitThreshold = 20;
        vm.CoreClockLimitEnabled = true;
        vm.CoreClockMaxMhz = 1800;
        vm.RestoreDefaultOnUnlimit = false;
        vm.PowerLimitWatts = 250;
        vm.RestoreToWatts = 200;

        var config = new AppConfig();
        vm.ApplyTo(config);

        config.SelectedGpuUuid.Should().Be(GpuA.Uuid);
        config.AutoLimitEnabled.Should().BeTrue();
        config.AutoLimitThreshold.Should().Be(20);
        config.CoreClockLimitEnabled.Should().BeTrue();
        config.CoreClockMaxMhz.Should().Be(1800);
        config.RestoreDefaultOnUnlimit.Should().BeFalse();
        config.PowerLimitWatts.Should().Be(250);
        config.RestoreToWatts.Should().Be(200);
    }
}
