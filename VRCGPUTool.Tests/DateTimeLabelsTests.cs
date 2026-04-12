using FluentAssertions;
using VRCGPUTool.Services;
using Xunit;

namespace VRCGPUTool.Tests;

public class DateTimeLabelsTests
{
    [Theory]
    [InlineData(DayOfWeek.Monday,    "月")]
    [InlineData(DayOfWeek.Tuesday,   "火")]
    [InlineData(DayOfWeek.Wednesday, "水")]
    [InlineData(DayOfWeek.Thursday,  "木")]
    [InlineData(DayOfWeek.Friday,    "金")]
    [InlineData(DayOfWeek.Saturday,  "土")]
    [InlineData(DayOfWeek.Sunday,    "日")]
    public void DayLabel_ReturnsCorrectLabel(DayOfWeek dow, string expected)
    {
        DateTimeLabels.DayLabel(dow).Should().Be(expected);
    }

    [Fact]
    public void DayLabel_UnknownValue_ReturnsEmpty()
    {
        DateTimeLabels.DayLabel((DayOfWeek)99).Should().Be("");
    }
}
