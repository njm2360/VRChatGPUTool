using FluentAssertions;
using VRCGPUTool.Models;
using Xunit;

namespace VRCGPUTool.Tests;

public class ElectricityProfileTests
{
    // スロットなし → 全時間帯 0 円
    [Fact]
    public void ComputeHourlyPrices_EmptySlots_ReturnsAllZeros()
    {
        var profile = new ElectricityProfile { WeekdaySlots = [] };

        var prices = profile.ComputeHourlyPrices(isWeekday: true);

        prices.Should().HaveCount(24);
        prices.Should().AllBeEquivalentTo(0.0);
    }

    // 0 時始まりのスロット 1 つ → 全時間帯がその単価
    [Fact]
    public void ComputeHourlyPrices_SingleSlotAtHour0_AllHoursGetThatPrice()
    {
        var profile = new ElectricityProfile
        {
            WeekdaySlots = [new PriceSlot { Hour = 0, UnitPrice = 30.0 }]
        };

        var prices = profile.ComputeHourlyPrices(isWeekday: true);

        prices.Should().AllBeEquivalentTo(30.0);
    }

    // スロット開始前の時間帯は 0 円のまま
    [Fact]
    public void ComputeHourlyPrices_SlotStartsAfterHour0_HoursBeforeSlotAreZero()
    {
        var profile = new ElectricityProfile
        {
            WeekdaySlots = [new PriceSlot { Hour = 6, UnitPrice = 25.0 }]
        };

        var prices = profile.ComputeHourlyPrices(isWeekday: true);

        prices[0].Should().Be(0.0);
        prices[5].Should().Be(0.0);
        prices[6].Should().Be(25.0);
        prices[23].Should().Be(25.0);
    }

    // 複数スロット → 各時間帯に正しい単価が割り当てられる
    [Fact]
    public void ComputeHourlyPrices_MultipleSlots_CorrectPricePerHour()
    {
        var profile = new ElectricityProfile
        {
            WeekdaySlots =
            [
                new PriceSlot { Hour = 0,  UnitPrice = 20.0 },
                new PriceSlot { Hour = 8,  UnitPrice = 30.0 },
                new PriceSlot { Hour = 22, UnitPrice = 20.0 },
            ]
        };

        var prices = profile.ComputeHourlyPrices(isWeekday: true);

        prices[0].Should().Be(20.0);   // hour  0: 深夜料金
        prices[7].Should().Be(20.0);   // hour  7: まだ深夜料金
        prices[8].Should().Be(30.0);   // hour  8: 昼間料金に切り替え
        prices[21].Should().Be(30.0);  // hour 21: まだ昼間料金
        prices[22].Should().Be(20.0);  // hour 22: 深夜料金に戻る
        prices[23].Should().Be(20.0);  // hour 23: 深夜料金
    }

    // UseDayOfWeek=false のとき isWeekday の値に関わらず WeekdaySlots を使う
    [Fact]
    public void ComputeHourlyPrices_UseDayOfWeekFalse_AlwaysUsesWeekdaySlots()
    {
        var profile = new ElectricityProfile
        {
            UseDayOfWeek = false,
            WeekdaySlots = [new PriceSlot { Hour = 0, UnitPrice = 30.0 }],
            HolidaySlots = [new PriceSlot { Hour = 0, UnitPrice = 20.0 }],
        };

        var weekday = profile.ComputeHourlyPrices(isWeekday: true);
        var holiday = profile.ComputeHourlyPrices(isWeekday: false);

        weekday[0].Should().Be(30.0);
        holiday[0].Should().Be(30.0); // WeekdaySlots が使われるので 30 円
    }

    // UseDayOfWeek=true かつ isWeekday=false → HolidaySlots を使う
    [Fact]
    public void ComputeHourlyPrices_UseDayOfWeekTrue_UsesHolidaySlotsOnHoliday()
    {
        var profile = new ElectricityProfile
        {
            UseDayOfWeek = true,
            WeekdaySlots = [new PriceSlot { Hour = 0, UnitPrice = 30.0 }],
            HolidaySlots = [new PriceSlot { Hour = 0, UnitPrice = 20.0 }],
        };

        var weekday = profile.ComputeHourlyPrices(isWeekday: true);
        var holiday = profile.ComputeHourlyPrices(isWeekday: false);

        weekday[0].Should().Be(30.0);
        holiday[0].Should().Be(20.0);
    }

    // スロットが Hour 昇順でなくても正しく動作する
    [Fact]
    public void ComputeHourlyPrices_UnsortedSlots_SortsCorrectly()
    {
        var profile = new ElectricityProfile
        {
            WeekdaySlots =
            [
                new PriceSlot { Hour = 22, UnitPrice = 20.0 },
                new PriceSlot { Hour = 0,  UnitPrice = 15.0 },
                new PriceSlot { Hour = 8,  UnitPrice = 30.0 },
            ]
        };

        var prices = profile.ComputeHourlyPrices(isWeekday: true);

        prices[0].Should().Be(15.0);
        prices[8].Should().Be(30.0);
        prices[22].Should().Be(20.0);
    }
}
