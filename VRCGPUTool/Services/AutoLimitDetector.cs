namespace VRCGPUTool.Services;

/// <summary>
/// 直近 N サンプルの GPU 使用率を分析し、
/// 使用率が安定している (= 自動制限をかけるべき状態) かどうかを判定する。
///
/// アルゴリズム:
///   300 サンプルをリングバッファに貯め、20 サンプル単位で平均化した
///   セグメント群の最大値と最小値の差が閾値未満なら「安定」と判定する。
/// </summary>
public sealed class AutoLimitDetector
{
    private const int BufferSize = 300;
    private const int SegmentSize = 20;
    private const int SegmentCount = BufferSize / SegmentSize; // 15

    private readonly int[] _buffer = new int[BufferSize];
    private int _writeIndex;
    private bool _bufferFull;

    public void Reset()
    {
        Array.Fill(_buffer, 0);
        _writeIndex = 0;
        _bufferFull = false;
    }

    /// <summary>
    /// 新しい GPU 使用率を追加し、安定しているかどうかを返す。
    /// バッファが満杯になるまでは常に false を返す。
    /// </summary>
    public bool Update(int utilization, int threshold)
    {
        _buffer[_writeIndex] = utilization;
        _writeIndex = (_writeIndex + 1) % BufferSize;
        if (_writeIndex == 0) _bufferFull = true;

        if (!_bufferFull) return false;

        int maxAvg = int.MinValue;
        int minAvg = int.MaxValue;

        for (int seg = 0; seg < SegmentCount; seg++)
        {
            int sum = 0;
            int baseIdx = seg * SegmentSize;
            for (int i = 0; i < SegmentSize; i++)
                sum += _buffer[baseIdx + i];

            int avg = sum / SegmentSize;
            if (avg > maxAvg) maxAvg = avg;
            if (avg < minAvg) minAvg = avg;
        }

        return (maxAvg - minAvg) < threshold;
    }
}
