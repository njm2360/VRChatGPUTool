namespace VRCGPUTool.Models;

/// <summary>
/// 1日分の時間別消費電力の累積ログ。
/// </summary>
public sealed class HourlyPowerLog
{
    public DateOnly Date { get; init; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>インデックス = 時 (0-23)、値 = 1秒ポーリングの累積消費電力量 (W·s)</summary>
    public int[] HourlyWatts { get; } = new int[24];

    public void Accumulate(int hour, int watts)
    {
        if ((uint)hour < 24u)
            HourlyWatts[hour] += watts;
    }

    public void Clear() => Array.Clear(HourlyWatts);
}
