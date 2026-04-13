using FluentAssertions;
using Moq;
using VRCGPUTool.Models;
using VRCGPUTool.Services;
using VRCGPUTool.ViewModels.PowerHistory;
using Xunit;

namespace VRCGPUTool.Tests;

public class DailyHistoryViewModelTests
{
    // ─────────────────────────────────────────────────────────
    // ヘルパー
    // ─────────────────────────────────────────────────────────

    private static Mock<IPowerLogService> DefaultPowerLogMock()
    {
        var mock = new Mock<IPowerLogService>();
        mock.Setup(s => s.LoadForDateAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(new HourlyPowerLog());
        return mock;
    }

    private static DailyHistoryViewModel CreateViewModel(
        HourlyPowerLog? log = null,
        ElectricityProfile? profile = null,
        Mock<IPowerLogService>? powerLogMock = null)
    {
        log ??= new HourlyPowerLog();
        profile ??= new ElectricityProfile();
        powerLogMock ??= DefaultPowerLogMock();

        return new DailyHistoryViewModel(
            powerLogMock.Object,
            () => log,
            profile);
    }

    // ─────────────────────────────────────────────────────────
    // DateText
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void DateText_FormatsAsJapaneseDate()
    {
        var log = new HourlyPowerLog { Date = new DateOnly(2025, 6, 9) };
        var vm = CreateViewModel(log);

        vm.DateText.Should().Be("2025年6月9日(月)");
    }

    // ─────────────────────────────────────────────────────────
    // LoadBars — Bars / MaxWatts
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void LoadBars_Bars_AlwaysHas24Entries()
    {
        var vm = CreateViewModel();

        vm.Bars.Should().HaveCount(24);
    }

    [Fact]
    public void LoadBars_MaxWatts_IsMaxOfHourlyWatts()
    {
        var log = new HourlyPowerLog();
        log.Accumulate(5, 300);
        log.Accumulate(10, 500);
        var vm = CreateViewModel(log);

        vm.MaxWatts.Should().Be(500);
    }

    [Fact]
    public void LoadBars_BarRatio_IsRelativeToMax()
    {
        var log = new HourlyPowerLog();
        log.Accumulate(0, 200); // max
        log.Accumulate(1, 100); // half of max
        var vm = CreateViewModel(log);

        vm.Bars[0].BarRatio.Should().BeApproximately(1.0, 0.001);
        vm.Bars[1].BarRatio.Should().BeApproximately(0.5, 0.001);
    }

    [Fact]
    public void LoadBars_AllZero_BarsHaveZeroRatioAndMaxWattsIsZero()
    {
        var vm = CreateViewModel();

        vm.MaxWatts.Should().Be(0);
        vm.Bars.Should().AllSatisfy(b => b.BarRatio.Should().Be(0));
    }

    // ─────────────────────────────────────────────────────────
    // LoadBars — YTicks
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void LoadBars_YTicks_AlwaysHas4Entries()
    {
        var vm = CreateViewModel();

        vm.YTicks.Should().HaveCount(4);
    }

    [Fact]
    public void LoadBars_YTicks_TopRatiosAreCorrect()
    {
        var vm = CreateViewModel();

        vm.YTicks[0].TopRatio.Should().BeApproximately(0.0, 0.001);
        vm.YTicks[1].TopRatio.Should().BeApproximately(0.25, 0.001);
        vm.YTicks[2].TopRatio.Should().BeApproximately(0.5, 0.001);
        vm.YTicks[3].TopRatio.Should().BeApproximately(0.75, 0.001);
    }

    [Fact]
    public void LoadBars_YTicks_LabelsAt100_75_50_25Percent()
    {
        // max = 7200 W·s → 2.0 / 1.5 / 1.0 / 0.5 Wh
        var log = new HourlyPowerLog();
        log.Accumulate(0, 7200);
        var vm = CreateViewModel(log);

        vm.YTicks[0].Label.Should().Be("2.0 Wh");
        vm.YTicks[1].Label.Should().Be("1.5 Wh");
        vm.YTicks[2].Label.Should().Be("1.0 Wh");
        vm.YTicks[3].Label.Should().Be("0.5 Wh");
    }

    [Fact]
    public void LoadBars_AllZero_YTicksAllShowZeroLabel()
    {
        var vm = CreateViewModel();

        vm.YTicks.Should().AllSatisfy(t => t.Label.Should().Be("0.0 Wh"));
    }

    // ─────────────────────────────────────────────────────────
    // LoadBars — TotalText
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void LoadBars_TotalText_NoSlots_ShowsUnsettledMessage()
    {
        var profile = new ElectricityProfile { WeekdaySlots = [] };
        var vm = CreateViewModel(profile: profile);

        vm.TotalText.Should().Contain("kWh");
        vm.TotalText.Should().Contain("(単価未設定)");
    }

    [Fact]
    public void LoadBars_TotalText_WithSlots_ShowsPriceAndKwh()
    {
        var log = new HourlyPowerLog();
        for (int h = 0; h < 24; h++) log.Accumulate(h, 3600);

        var profile = new ElectricityProfile
        {
            WeekdaySlots = [new PriceSlot { Hour = 0, UnitPrice = 10.0 }],
        };

        var vm = CreateViewModel(log, profile);

        vm.TotalText.Should().Contain("0.024 kWh");
        vm.TotalText.Should().Contain("円");
        vm.TotalText.Should().NotContain("(単価未設定)");
    }

    // ─────────────────────────────────────────────────────────
    // LoadBars — isWeekday の切り替え
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void LoadBars_Saturday_UsesHolidayPrices()
    {
        // 土曜日: UseDayOfWeek=true なら HolidaySlots (20円) が使われる
        var log = new HourlyPowerLog { Date = new DateOnly(2025, 6, 7) }; // 土曜日
        for (int h = 0; h < 24; h++) log.Accumulate(h, 3_600_000); // 24h × 1kWh = 24kWh
        var profile = new ElectricityProfile
        {
            UseDayOfWeek = true,
            WeekdaySlots = [new PriceSlot { Hour = 0, UnitPrice = 30.0 }],
            HolidaySlots = [new PriceSlot { Hour = 0, UnitPrice = 20.0 }],
        };
        var vm = CreateViewModel(log, profile);

        vm.TotalText.Should().Contain("電気代: 480.0 円"); // 24kWh × 20円
    }

    // UseDayOfWeek=true・HolidaySlots 未設定の土曜日 → 「単価未設定」
    [Fact]
    public void LoadBars_Saturday_UseDayOfWeekTrue_EmptyHolidaySlots_ShowsUnsettledMessage()
    {
        var log = new HourlyPowerLog { Date = new DateOnly(2025, 6, 7) }; // 土曜日
        var profile = new ElectricityProfile
        {
            UseDayOfWeek = true,
            WeekdaySlots = [new PriceSlot { Hour = 0, UnitPrice = 30.0 }],
            HolidaySlots = [],
        };
        var vm = CreateViewModel(log, profile);

        vm.TotalText.Should().Contain("(単価未設定)");
        vm.TotalText.Should().NotContain("電気代");
    }

    [Fact]
    public void LoadBars_Monday_UsesWeekdayPrices()
    {
        // 月曜日: UseDayOfWeek=true なら WeekdaySlots (30円) が使われる
        var log = new HourlyPowerLog { Date = new DateOnly(2025, 6, 9) }; // 月曜日
        for (int h = 0; h < 24; h++) log.Accumulate(h, 3_600_000); // 24h × 1kWh = 24kWh
        var profile = new ElectricityProfile
        {
            UseDayOfWeek = true,
            WeekdaySlots = [new PriceSlot { Hour = 0, UnitPrice = 30.0 }],
            HolidaySlots = [new PriceSlot { Hour = 0, UnitPrice = 20.0 }],
        };
        var vm = CreateViewModel(log, profile);

        vm.TotalText.Should().Contain("電気代: 720.0 円"); // 24kWh × 30円
    }

    // ─────────────────────────────────────────────────────────
    // NextDayAsync — 明日以降に進めない上限ガード
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void NextDayCommand_AtToday_CanExecuteIsFalse()
    {
        var vm = CreateViewModel();
        var today = DateOnly.FromDateTime(DateTime.Today);
        vm.SelectedDate = today;

        vm.NextDayCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void NextDayCommand_AtTomorrow_CanExecuteIsFalse()
    {
        var vm = CreateViewModel();
        var tomorrow = DateOnly.FromDateTime(DateTime.Today).AddDays(1);
        vm.SelectedDate = tomorrow;

        vm.NextDayCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void NextDayCommand_BeforeToday_CanExecuteIsTrue()
    {
        var vm = CreateViewModel();
        var yesterday = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
        vm.SelectedDate = yesterday;

        vm.NextDayCommand.CanExecute(null).Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────
    // PreviousDayAsync
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task PreviousDayCommand_DecrementsDateByOne()
    {
        var log = new HourlyPowerLog { Date = new DateOnly(2025, 6, 9) };
        var vm = CreateViewModel(log);

        await vm.PreviousDayCommand.ExecuteAsync(null);

        vm.SelectedDate.Should().Be(new DateOnly(2025, 6, 8));
    }

    // ─────────────────────────────────────────────────────────
    // ReloadAsync — ライブログ vs. ファイル
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task PreviousDayAsync_UsesFileLog_WhenDateDiffersFromLiveLog()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var liveLog = new HourlyPowerLog { Date = today };

        var fileLog = new HourlyPowerLog();
        fileLog.Accumulate(0, 9999);

        var powerLogMock = DefaultPowerLogMock();
        powerLogMock
            .Setup(s => s.LoadForDateAsync(today.AddDays(-1)))
            .ReturnsAsync(fileLog);

        var vm = CreateViewModel(liveLog, powerLogMock: powerLogMock);
        await vm.PreviousDayCommand.ExecuteAsync(null);

        vm.Bars[0].Ws.Should().Be(9999);
    }

    [Fact]
    public async Task NextDayAsync_UsesLiveLog_WhenDateMatchesLiveLog()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var liveLog = new HourlyPowerLog { Date = today };
        liveLog.Accumulate(0, 1234);

        var powerLogMock = DefaultPowerLogMock();
        var vm = CreateViewModel(liveLog, powerLogMock: powerLogMock);

        await vm.PreviousDayCommand.ExecuteAsync(null);
        await vm.NextDayCommand.ExecuteAsync(null);

        vm.Bars[0].Ws.Should().Be(1234);
        powerLogMock.Verify(s => s.LoadForDateAsync(today), Times.Never);
    }
}
