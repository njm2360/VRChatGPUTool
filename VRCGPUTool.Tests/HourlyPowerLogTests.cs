using FluentAssertions;
using VRCGPUTool.Models;
using Xunit;

namespace VRCGPUTool.Tests;

public class HourlyPowerLogTests
{
    // ─────────────────────────────────────────────────────────
    // Accumulate
    // ─────────────────────────────────────────────────────────

    // 指定した時間のバケツに値が加算される
    [Fact]
    public void Accumulate_AddsWattsToCorrectHour()
    {
        var log = new HourlyPowerLog();

        log.Accumulate(10, 300);

        log.HourlyWatts[10].Should().Be(300);
    }

    // 同じ時間に複数回呼ぶと累積される
    [Fact]
    public void Accumulate_AccumulatesMultipleCalls()
    {
        var log = new HourlyPowerLog();

        log.Accumulate(5, 100);
        log.Accumulate(5, 200);

        log.HourlyWatts[5].Should().Be(300);
    }

    // 他の時間帯には影響しない
    [Fact]
    public void Accumulate_DoesNotAffectOtherHours()
    {
        var log = new HourlyPowerLog();

        log.Accumulate(12, 500);

        for (int h = 0; h < 24; h++)
        {
            if (h == 12) continue;
            log.HourlyWatts[h].Should().Be(0, $"hour {h} should be unaffected");
        }
    }

    // 境界値: hour=0 と hour=23 は有効
    [Theory]
    [InlineData(0)]
    [InlineData(23)]
    public void Accumulate_BoundaryHours_AreValid(int hour)
    {
        var log = new HourlyPowerLog();

        log.Accumulate(hour, 100);

        log.HourlyWatts[hour].Should().Be(100);
    }

    // 範囲外の hour は無視される（例外も発生しない）
    [Theory]
    [InlineData(-1)]
    [InlineData(24)]
    [InlineData(100)]
    public void Accumulate_OutOfRangeHour_IsIgnored(int hour)
    {
        var log = new HourlyPowerLog();

        var act = () => log.Accumulate(hour, 999);

        act.Should().NotThrow();
        log.HourlyWatts.Should().AllBeEquivalentTo(0);
    }

    // ─────────────────────────────────────────────────────────
    // Clear
    // ─────────────────────────────────────────────────────────

    [Fact]
    public void Clear_ResetsAllHoursToZero()
    {
        var log = new HourlyPowerLog();
        for (int h = 0; h < 24; h++)
            log.Accumulate(h, (h + 1) * 100);

        log.Clear();

        log.HourlyWatts.Should().AllBeEquivalentTo(0);
    }
}
