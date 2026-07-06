using FluentAssertions;
using Moq;
using VRCGPUTool.Models;
using VRCGPUTool.Services;
using VRCGPUTool.ViewModels.PowerHistory;
using Xunit;

namespace VRCGPUTool.Tests;

public class MonthlyHistoryViewModelTests
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

    private static MonthlyHistoryViewModel CreateViewModel(
        HourlyPowerLog? liveLog = null,
        Mock<IPowerLogService>? powerLogMock = null,
        ElectricityProfile? profile = null)
    {
        liveLog ??= new HourlyPowerLog();
        powerLogMock ??= DefaultPowerLogMock();
        profile ??= new ElectricityProfile();
        return new MonthlyHistoryViewModel(
            powerLogMock.Object,
            new PowerLogCsvExporter(powerLogMock.Object),
            () => liveLog,
            profile);
    }

    // ─────────────────────────────────────────────────────────
    // MonthText
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task MonthText_FormatsAsJapaneseYearMonth()
    {
        var vm = CreateViewModel();
        vm.SelectedMonth = new DateOnly(2025, 6, 1);
        await vm.ReloadAsync();

        vm.MonthText.Should().Be("2025年6月");
    }

    // ─────────────────────────────────────────────────────────
    // DayBars の件数とラベル
    // ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(2025, 6, 30)]  // 6月
    [InlineData(2025, 1, 31)]  // 1月
    [InlineData(2025, 2, 28)]  // 平年2月
    [InlineData(2024, 2, 29)]  // 閏年2月
    public async Task LoadDayBars_DayBarsCount_EqualsDaysInMonth(int year, int month, int expected)
    {
        var vm = CreateViewModel();
        vm.SelectedMonth = new DateOnly(year, month, 1);
        await vm.ReloadAsync();

        vm.DayBars.Should().HaveCount(expected);
    }

    [Fact]
    public async Task LoadDayBars_DayBarLabels_Are1ToLastDay()
    {
        var vm = CreateViewModel();
        vm.SelectedMonth = new DateOnly(2025, 6, 1); // 30日
        await vm.ReloadAsync();

        vm.DayBars.First().Label.Should().Be("1");
        vm.DayBars.Last().Label.Should().Be("30");
    }

    // ─────────────────────────────────────────────────────────
    // DayYTicks
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadDayBars_DayYTicks_AlwaysHas4Entries()
    {
        var vm = CreateViewModel();
        vm.SelectedMonth = new DateOnly(2025, 1, 1);
        await vm.ReloadAsync();

        vm.DayYTicks.Should().HaveCount(4);
    }

    [Fact]
    public async Task LoadDayBars_DayYTicks_TopRatiosAreCorrect()
    {
        var vm = CreateViewModel();
        vm.SelectedMonth = new DateOnly(2025, 1, 1);
        await vm.ReloadAsync();

        vm.DayYTicks[0].TopRatio.Should().BeApproximately(0.0, 0.001);
        vm.DayYTicks[1].TopRatio.Should().BeApproximately(0.25, 0.001);
        vm.DayYTicks[2].TopRatio.Should().BeApproximately(0.5, 0.001);
        vm.DayYTicks[3].TopRatio.Should().BeApproximately(0.75, 0.001);
    }

    [Fact]
    public async Task LoadDayBars_AllZero_DayYTicksAllShowZeroLabel()
    {
        var vm = CreateViewModel();
        vm.SelectedMonth = new DateOnly(2025, 1, 1);
        await vm.ReloadAsync();

        vm.DayYTicks.Should().AllSatisfy(t => t.Label.Should().Be("0.000 kWh"));
    }

    [Fact]
    public async Task LoadDayBars_YTickLabels_At100_75_50_25Percent()
    {
        // 1月1日に 24h × 300000 W·s = 7200000 W·s = 2 kWh → max
        var powerLogMock = DefaultPowerLogMock();
        var jan1 = new DateOnly(2025, 1, 1);
        var fileLog = new HourlyPowerLog();
        for (int h = 0; h < 24; h++) fileLog.Accumulate(h, 300000);
        powerLogMock.Setup(s => s.LoadForDateAsync(jan1)).ReturnsAsync(fileLog);

        var vm = CreateViewModel(powerLogMock: powerLogMock);
        vm.SelectedMonth = new DateOnly(2025, 1, 1);
        await vm.ReloadAsync();

        vm.DayYTicks[0].Label.Should().Be("2.000 kWh");
        vm.DayYTicks[1].Label.Should().Be("1.500 kWh");
        vm.DayYTicks[2].Label.Should().Be("1.000 kWh");
        vm.DayYTicks[3].Label.Should().Be("0.500 kWh");
    }

    // ─────────────────────────────────────────────────────────
    // MonthTotalText
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadDayBars_MonthTotalText_AllZero_ShowsZeroKwh()
    {
        var vm = CreateViewModel();
        vm.SelectedMonth = new DateOnly(2025, 1, 1);
        await vm.ReloadAsync();

        vm.MonthTotalText.Should().Be("合計: 0.000 kWh  (単価未設定)");
    }

    [Fact]
    public async Task LoadDayBars_MonthTotalText_CorrectKwh()
    {
        // 1月1日に 24h × 150000 W·s = 3600000 W·s = 1 kWh
        var powerLogMock = DefaultPowerLogMock();
        var jan1 = new DateOnly(2025, 1, 1);
        var fileLog = new HourlyPowerLog();
        for (int h = 0; h < 24; h++) fileLog.Accumulate(h, 150000);
        powerLogMock.Setup(s => s.LoadForDateAsync(jan1)).ReturnsAsync(fileLog);

        var vm = CreateViewModel(powerLogMock: powerLogMock);
        vm.SelectedMonth = new DateOnly(2025, 1, 1);
        await vm.ReloadAsync();

        vm.MonthTotalText.Should().Be("合計: 1.000 kWh  (単価未設定)");
    }

    // UseDayOfWeek=true・HolidaySlots 未設定 → 「単価未設定」
    [Fact]
    public async Task LoadDayBars_MonthTotalText_UseDayOfWeekTrue_EmptyHolidaySlots_ShowsUnsettledMessage()
    {
        var profile = new ElectricityProfile
        {
            UseDayOfWeek = true,
            WeekdaySlots = [new PriceSlot { Hour = 0, UnitPrice = 30.0 }],
            HolidaySlots = [],
        };
        var vm = CreateViewModel(profile: profile);
        vm.SelectedMonth = new DateOnly(2025, 6, 1);
        await vm.ReloadAsync();

        vm.MonthTotalText.Should().Contain("(単価未設定)");
        vm.MonthTotalText.Should().NotContain("電気代");
    }

    // ─────────────────────────────────────────────────────────
    // 過去日付 → LoadForDateAsync を使う
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadDayBars_PastDate_UsesLoadForDateAsync()
    {
        var powerLogMock = DefaultPowerLogMock();
        var jan1 = new DateOnly(2025, 1, 1);
        var fileLog = new HourlyPowerLog();
        fileLog.Accumulate(0, 7200);
        powerLogMock.Setup(s => s.LoadForDateAsync(jan1)).ReturnsAsync(fileLog);

        var vm = CreateViewModel(powerLogMock: powerLogMock);
        vm.SelectedMonth = new DateOnly(2025, 1, 1);
        await vm.ReloadAsync();

        vm.DayBars[0].TotalWs.Should().Be(7200);
    }

    // ─────────────────────────────────────────────────────────
    // 今日 → ライブログを使う
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadDayBars_Today_UsesLiveLog()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var liveLog = new HourlyPowerLog { Date = today };
        liveLog.Accumulate(0, 3600);

        var powerLogMock = DefaultPowerLogMock();
        var vm = CreateViewModel(liveLog, powerLogMock);
        await vm.ReloadAsync();

        vm.DayBars[today.Day - 1].TotalWs.Should().Be(3600);
        powerLogMock.Verify(s => s.LoadForDateAsync(today), Times.Never);
    }

    // ─────────────────────────────────────────────────────────
    // BarRatio
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadDayBars_BarRatio_IsRelativeToMax()
    {
        var powerLogMock = DefaultPowerLogMock();
        var jan1 = new DateOnly(2025, 1, 1);
        var jan2 = new DateOnly(2025, 1, 2);

        var logMax = new HourlyPowerLog(); logMax.Accumulate(0, 200);
        var logHalf = new HourlyPowerLog(); logHalf.Accumulate(0, 100);
        powerLogMock.Setup(s => s.LoadForDateAsync(jan1)).ReturnsAsync(logMax);
        powerLogMock.Setup(s => s.LoadForDateAsync(jan2)).ReturnsAsync(logHalf);

        var vm = CreateViewModel(powerLogMock: powerLogMock);
        vm.SelectedMonth = new DateOnly(2025, 1, 1);
        await vm.ReloadAsync();

        vm.DayBars[0].BarRatio.Should().BeApproximately(1.0, 0.001);
        vm.DayBars[1].BarRatio.Should().BeApproximately(0.5, 0.001);
    }

    // ─────────────────────────────────────────────────────────
    // NextMonthAsync — 今月以降に進めない
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void NextMonthCommand_AtCurrentMonth_CanExecuteIsFalse()
    {
        var vm = CreateViewModel();
        var thisMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        vm.SelectedMonth = thisMonth;

        vm.NextMonthCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void NextMonthCommand_BeforeCurrentMonth_CanExecuteIsTrue()
    {
        var vm = CreateViewModel();
        var lastMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
        vm.SelectedMonth = lastMonth;

        vm.NextMonthCommand.CanExecute(null).Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────
    // PreviousMonthAsync
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task PreviousMonthCommand_DecrementsMonthByOne()
    {
        var vm = CreateViewModel();
        vm.SelectedMonth = new DateOnly(2025, 6, 1);

        await vm.PreviousMonthCommand.ExecuteAsync(null);

        vm.SelectedMonth.Should().Be(new DateOnly(2025, 5, 1));
    }

    // 1月から前月に遡ると前年12月になる
    [Fact]
    public async Task PreviousMonthCommand_FromJanuary_GoesToPreviousYearDecember()
    {
        var vm = CreateViewModel();
        vm.SelectedMonth = new DateOnly(2025, 1, 1);

        await vm.PreviousMonthCommand.ExecuteAsync(null);

        vm.SelectedMonth.Should().Be(new DateOnly(2024, 12, 1));
    }
}
