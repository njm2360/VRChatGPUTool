using FluentAssertions;
using VRCGPUTool.Services;
using Xunit;

namespace VRCGPUTool.Tests;

public class AutoLimitDetectorTests
{
    // バッファサイズは 300。満杯になる前は常に false を返す。
    [Fact]
    public void Update_ReturnsFalse_BeforeBufferIsFull()
    {
        var detector = new AutoLimitDetector();

        bool result = false;
        for (int i = 0; i < 299; i++)
            result = detector.Update(50, 10);

        result.Should().BeFalse();
    }

    // 300 サンプル全て同じ値 → セグメント平均の差はゼロ → 安定
    [Fact]
    public void Update_ReturnsTrue_WhenBufferFullAndAllSameValue()
    {
        var detector = new AutoLimitDetector();

        bool result = false;
        for (int i = 0; i < 300; i++)
            result = detector.Update(50, 10);

        result.Should().BeTrue();
    }

    // 前半 0% / 後半 50% → セグメント平均の差が閾値を超える → 不安定
    [Fact]
    public void Update_ReturnsFalse_WhenValuesAreUnstable()
    {
        var detector = new AutoLimitDetector();

        for (int i = 0; i < 150; i++)
            detector.Update(0, 10);

        bool result = false;
        for (int i = 0; i < 150; i++)
            result = detector.Update(50, 10);

        result.Should().BeFalse();
    }

    // Reset 後はバッファが空になるため、再び 300 サンプル溜まるまで false
    [Fact]
    public void Reset_CausesBufferToRequireRefilling()
    {
        var detector = new AutoLimitDetector();

        for (int i = 0; i < 300; i++)
            detector.Update(50, 10);

        detector.Reset();

        detector.Update(50, 10).Should().BeFalse();
    }

    // 閾値を変えても安定判定は変わらない（全て同値なら差はゼロ）
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Update_ReturnsTrue_WithVariousThresholds_WhenStable(int threshold)
    {
        var detector = new AutoLimitDetector();

        bool result = false;
        for (int i = 0; i < 300; i++)
            result = detector.Update(50, threshold);

        result.Should().BeTrue();
    }

    // セグメント平均の差 == threshold のとき false（< は厳密な不等号）
    [Fact]
    public void Update_ReturnsFalse_WhenDiffEqualsThreshold()
    {
        var detector = new AutoLimitDetector();

        // 前半 150 サンプル = 0、後半 150 サンプル = 20
        // セグメント平均: 前半 7 セグメントは 0、後半 7 セグメントは 20、
        // 境界の 1 セグメント(index=7, サンプル 140-159)は 0×10 + 20×10 = 10 平均
        // → maxAvg=20, minAvg=0, 差=20
        // threshold=20 → 20 < 20 は false
        for (int i = 0; i < 150; i++)
            detector.Update(0, 20);
        bool result = false;
        for (int i = 0; i < 150; i++)
            result = detector.Update(20, 20);

        result.Should().BeFalse();
    }

    // セグメント平均の差 == threshold-1 のとき true
    [Fact]
    public void Update_ReturnsTrue_WhenDiffIsBelowThreshold()
    {
        var detector = new AutoLimitDetector();

        // 前半 150 サンプル = 0、後半 150 サンプル = 20、threshold=21
        // 差=20 < 21 → true
        for (int i = 0; i < 150; i++)
            detector.Update(0, 21);
        bool result = false;
        for (int i = 0; i < 150; i++)
            result = detector.Update(20, 21);

        result.Should().BeTrue();
    }

    // 不安定な 300 サンプルの後、安定した 300 サンプルを追加すると true になる
    [Fact]
    public void Update_ReturnsTrue_AfterUnstableDataIsOverwritten()
    {
        var detector = new AutoLimitDetector();

        // 不安定フェーズ: 前半 0% / 後半 50%
        for (int i = 0; i < 150; i++)
            detector.Update(0, 10);
        for (int i = 0; i < 150; i++)
            detector.Update(50, 10);

        // 安定フェーズ: 300 サンプル全て 50%（古いデータを全て上書き）
        bool result = false;
        for (int i = 0; i < 300; i++)
            result = detector.Update(50, 10);

        result.Should().BeTrue();
    }

    // 安定 → 不安定に変化したとき、古い安定データが上書きされて false になる
    [Fact]
    public void Update_ReturnsFalse_AfterStableDataIsOverwrittenWithUnstable()
    {
        var detector = new AutoLimitDetector();

        // 安定フェーズ
        for (int i = 0; i < 300; i++)
            detector.Update(50, 10);

        // 不安定フェーズ: 前半 0% / 後半 100% で古いデータを全て上書き
        for (int i = 0; i < 150; i++)
            detector.Update(0, 10);
        bool result = false;
        for (int i = 0; i < 150; i++)
            result = detector.Update(100, 10);

        result.Should().BeFalse();
    }

    // threshold=0 のとき差が 0 でも false（0 < 0 は偽）
    [Fact]
    public void Update_ReturnsFalse_WhenThresholdIsZero_EvenWithStableData()
    {
        var detector = new AutoLimitDetector();

        // 全サンプル同値 → maxAvg - minAvg = 0 だが 0 < 0 は false
        bool result = false;
        for (int i = 0; i < 300; i++)
            result = detector.Update(50, threshold: 0);

        result.Should().BeFalse();
    }
}
