using FluentAssertions;
using VRCGPUTool.ViewModels.PowerHistory;
using Xunit;

namespace VRCGPUTool.Tests;

public class FormatWhsTests
{
    // 1 W·s 未満の小さな値 → Wh 表示
    [Theory]
    [InlineData(0, "0.0 Wh")]
    [InlineData(3600, "1.0 Wh")]    // 3600 W·s = 1 Wh
    [InlineData(1800, "0.5 Wh")]    // 0.5 Wh
    [InlineData(3_596_400, "999.0 Wh")] // 999 Wh (< 1000 Wh)
    public void FormatWhs_BelowOneKwh_ReturnsWhFormat(int ws, string expected)
    {
        DailyHistoryViewModel.FormatWhs(ws).Should().Be(expected);
    }

    // 3,600,000 W·s = 1000 Wh = 1.00 kWh → kWh 表示に切り替わる
    [Theory]
    [InlineData(3_600_000, "1.00 kWh")]  // 1000 Wh = 1.00 kWh
    [InlineData(7_200_000, "2.00 kWh")]  // 2000 Wh = 2.00 kWh
    [InlineData(5_400_000, "1.50 kWh")]  // 1500 Wh = 1.50 kWh
    public void FormatWhs_AtOrAboveOneKwh_ReturnsKwhFormat(int ws, string expected)
    {
        DailyHistoryViewModel.FormatWhs(ws).Should().Be(expected);
    }

    // 境界直前: 3,599,640 W·s = 999.9 Wh（kWh 切り替え直前の最大 Wh 値）
    [Fact]
    public void FormatWhs_JustBelowOneKwh_ReturnsWhFormat()
    {
        // 3,599,640 / 3600.0 = 999.9 Wh → wh(999.9) < 1000 なので Wh 表示
        DailyHistoryViewModel.FormatWhs(3_599_640).Should().Be("999.9 Wh");
    }

    // 負の値 → 現在の実装は負の Wh をそのまま表示
    [Fact]
    public void FormatWhs_NegativeValue_ReturnsWhFormat()
    {
        // -3600 W·s → -1.0 Wh（wh < 1000 なので Wh 表示）
        DailyHistoryViewModel.FormatWhs(-3600).Should().Be("-1.0 Wh");
    }
}
