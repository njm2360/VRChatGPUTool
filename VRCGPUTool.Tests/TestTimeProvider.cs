namespace VRCGPUTool.Tests;

/// <summary>
/// テスト用 TimeProvider。UTC = Local として扱うことで
/// GetLocalNow().DateTime == 設定した DateTimeOffset.UtcDateTime となる。
/// </summary>
internal sealed class TestTimeProvider(DateTimeOffset startTime) : TimeProvider
{
    private DateTimeOffset _now = startTime;

    public void Set(DateTimeOffset now) => _now = now;

    public override DateTimeOffset GetUtcNow() => _now.ToUniversalTime();

    // UTC をローカル時刻として返すことでタイムゾーン変換を排除する
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
