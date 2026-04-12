using FluentAssertions;
using VRCGPUTool.Service;
using Xunit;

namespace VRCGPUTool.Tests;

public class NvidiaSmiExecutorTests
{
    // nvidia-smi が出力するフォーマットに合わせた正常行
    // name, uuid, power.limit, power.min_limit, power.max_limit, power.default_limit,
    // utilization.gpu, temperature.gpu, power.draw, clocks.gr, clocks.mem
    private const string ValidLine =
        "NVIDIA GeForce RTX 3080, GPU-12345678-1234-1234-1234-123456789012, " +
        "320.00, 100.00, 350.00, 320.00, 45, 65, 180, 1800, 9501";

    [Fact]
    public void TryParseLine_ValidLine_ReturnsTrueAndParsesAllFields()
    {
        bool ok = NvidiaSmiExecutor.TryParseLine(ValidLine, out var dto);

        ok.Should().BeTrue();
        dto.Name.Should().Be("NVIDIA GeForce RTX 3080");
        dto.Uuid.Should().Be("GPU-12345678-1234-1234-1234-123456789012");
        dto.PowerLimit.Should().Be(320);
        dto.PowerLimitMin.Should().Be(100);
        dto.PowerLimitMax.Should().Be(350);
        dto.PowerLimitDefault.Should().Be(320);
        dto.GpuUtilization.Should().Be(45);
        dto.CoreTemperature.Should().Be(65);
        dto.PowerDraw.Should().Be(180);
        dto.CoreClock.Should().Be(1800);
        dto.MemoryClock.Should().Be(9501);
    }

    // 列数が足りない行は false
    [Fact]
    public void TryParseLine_TooFewColumns_ReturnsFalse()
    {
        string line = "NVIDIA GeForce RTX 3080, GPU-00000000-0000-0000-0000-000000000000, 320.00";

        bool ok = NvidiaSmiExecutor.TryParseLine(line, out _);

        ok.Should().BeFalse();
    }

    // 数値フィールドに "N/A" などが入ると false
    [Fact]
    public void TryParseLine_NonNumericValueInNumericField_ReturnsFalse()
    {
        string line =
            "NVIDIA GeForce RTX 3080, GPU-12345678-1234-1234-1234-123456789012, " +
            "N/A, 100.00, 350.00, 320.00, 45, 65, 180, 1800, 9501";

        bool ok = NvidiaSmiExecutor.TryParseLine(line, out _);

        ok.Should().BeFalse();
    }

    // 空行は false
    [Fact]
    public void TryParseLine_EmptyLine_ReturnsFalse()
    {
        bool ok = NvidiaSmiExecutor.TryParseLine(string.Empty, out _);

        ok.Should().BeFalse();
    }

    // TryParseDouble: 正常な小数文字列をパース
    [Theory]
    [InlineData("320.00", 320.0)]
    [InlineData("  100.5  ", 100.5)]
    [InlineData("0", 0.0)]
    public void TryParseDouble_ValidInput_ReturnsTrueWithCorrectValue(string input, double expected)
    {
        bool ok = NvidiaSmiExecutor.TryParseDouble(input, out double value);

        ok.Should().BeTrue();
        value.Should().Be(expected);
    }

    // PowerLimit 系は Math.Round で丸められる
    [Theory]
    [InlineData("199.90", 200)]  // 切り捨てではなく四捨五入
    [InlineData("200.10", 200)]
    [InlineData("199.50", 200)]  // 銀行丸めではなくMidpointRounding.AwayFromZero
    [InlineData("200.00", 200)]
    public void TryParseLine_PowerLimitNearInteger_IsRoundedNotTruncated(string rawWatts, int expected)
    {
        string line =
            $"NVIDIA GeForce RTX 3080, GPU-12345678-1234-1234-1234-123456789012, " +
            $"{rawWatts}, 100.00, 350.00, 320.00, 45, 65, 180, 1800, 9501";

        NvidiaSmiExecutor.TryParseLine(line, out var dto);

        dto.PowerLimit.Should().Be(expected);
    }

    // TryParseDouble: 数値でない文字列は false
    [Theory]
    [InlineData("N/A")]
    [InlineData("")]
    [InlineData("abc")]
    public void TryParseDouble_InvalidInput_ReturnsFalse(string input)
    {
        bool ok = NvidiaSmiExecutor.TryParseDouble(input, out _);

        ok.Should().BeFalse();
    }
}
