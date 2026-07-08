using System.IO;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using VRCGPUTool.Models;
using VRCGPUTool.Services;
using Xunit;

namespace VRCGPUTool.Tests;

public class SqlitePowerLogServiceTests : IDisposable
{
    private readonly string _dir;
    private readonly string _dbPath;
    private readonly string _jsonDir;

    public SqlitePowerLogServiceTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "vgt-sqlite-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        _dbPath = Path.Combine(_dir, "powerlog.db");
        _jsonDir = Path.Combine(_dir, "powerlog");
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try
        {
            if (Directory.Exists(_dir))
                Directory.Delete(_dir, recursive: true);
        }
        catch
        {
            // クリーンアップ失敗は無視
        }
    }

    private SqlitePowerLogService CreateService() => new(_dbPath, _jsonDir);

    private static HourlyPowerLog MakeLog(DateOnly date, Func<int, int> wattsByHour)
    {
        var log = new HourlyPowerLog { Date = date };
        for (int h = 0; h < 24; h++)
            log.Accumulate(h, wattsByHour(h));
        return log;
    }

    private void WriteV2File(DateOnly date, int[] watts)
    {
        Directory.CreateDirectory(_jsonDir);
        File.WriteAllText(Path.Combine(_jsonDir, $"{date:yyyy-MM-dd}.json"),
            JsonSerializer.Serialize(watts));
    }

    private void WriteV1File(DateOnly date, int[] watts)
    {
        Directory.CreateDirectory(_jsonDir);
        string json = JsonSerializer.Serialize(new { hourPowerLog = watts, logdate = $"{date:yyyy-MM-dd}T00:00:00" });
        File.WriteAllText(Path.Combine(_jsonDir, $"powerlog_{date:yyyyMMdd}.json"), json);
    }

    // ─────────────────────────────────────────────────────────
    // Save / Load — 基本動作
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_ThenLoadForDateAsync_RoundTripsAllHours()
    {
        var service = CreateService();
        var date = new DateOnly(2025, 6, 15);
        var log = MakeLog(date, h => (h + 1) * 1000);

        await service.SaveAsync(log);
        var loaded = await service.LoadForDateAsync(date);

        loaded.Date.Should().Be(date);
        loaded.HourlyWatts.Should().Equal(log.HourlyWatts);
    }

    [Fact]
    public async Task LoadForDateAsync_NoData_ReturnsEmptyLogWithRequestedDate()
    {
        var service = CreateService();
        var date = new DateOnly(2025, 6, 15);

        var loaded = await service.LoadForDateAsync(date);

        loaded.Date.Should().Be(date);
        loaded.HourlyWatts.Should().AllBeEquivalentTo(0);
    }

    [Fact]
    public async Task SaveAsync_SameDateTwice_LastWriteWins()
    {
        var service = CreateService();
        var date = new DateOnly(2025, 6, 15);

        await service.SaveAsync(MakeLog(date, _ => 111));
        await service.SaveAsync(MakeLog(date, _ => 222));
        var loaded = await service.LoadForDateAsync(date);

        loaded.HourlyWatts.Should().AllBeEquivalentTo(222);
    }

    [Fact]
    public async Task LoadMonthAsync_ReturnsAllDaysWithDataAtCorrectIndex()
    {
        var service = CreateService();
        var month = new DateOnly(2025, 6, 1);
        await service.SaveAsync(MakeLog(new DateOnly(2025, 6, 5), _ => 500));
        await service.SaveAsync(MakeLog(new DateOnly(2025, 6, 20), _ => 2000));
        // 隣接月のデータは含まれない
        await service.SaveAsync(MakeLog(new DateOnly(2025, 5, 31), _ => 999));
        await service.SaveAsync(MakeLog(new DateOnly(2025, 7, 1), _ => 999));

        var logs = await service.LoadMonthAsync(month);

        logs.Should().HaveCount(30); // 2025年6月 = 30日
        for (int d = 0; d < 30; d++)
            logs[d].Date.Should().Be(new DateOnly(2025, 6, d + 1));
        logs[4].HourlyWatts.Should().AllBeEquivalentTo(500);   // 6/5
        logs[19].HourlyWatts.Should().AllBeEquivalentTo(2000); // 6/20
        logs.Where((_, i) => i is not (4 or 19))
            .Should().OnlyContain(l => l.HourlyWatts.All(w => w == 0));
    }

    // ─────────────────────────────────────────────────────────
    // JSON マイグレーション
    // ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Migration_V2File_ImportsDataAndDeletesFile()
    {
        var date = new DateOnly(2025, 1, 15);
        int[] watts = [.. Enumerable.Range(0, 24).Select(h => h * 100)];
        WriteV2File(date, watts);

        var loaded = await CreateService().LoadForDateAsync(date);

        loaded.HourlyWatts.Should().Equal(watts);
        // 全ファイル移行済み → ファイルもディレクトリも消える
        Directory.Exists(_jsonDir).Should().BeFalse();
    }

    [Fact]
    public async Task Migration_V1File_ImportsDataAndDeletesFile()
    {
        var date = new DateOnly(2024, 12, 31);
        int[] watts = [.. Enumerable.Range(0, 24).Select(h => h + 1)];
        WriteV1File(date, watts);

        var loaded = await CreateService().LoadForDateAsync(date);

        loaded.HourlyWatts.Should().Equal(watts);
        Directory.Exists(_jsonDir).Should().BeFalse();
    }

    [Fact]
    public async Task Migration_SameDateInBothFormats_V2Wins()
    {
        // 同一日付にV1とV2が併存する場合、V2を採用する
        var date = new DateOnly(2025, 1, 15);
        WriteV1File(date, [.. Enumerable.Repeat(111, 24)]);
        WriteV2File(date, [.. Enumerable.Repeat(222, 24)]);

        var loaded = await CreateService().LoadForDateAsync(date);

        loaded.HourlyWatts.Should().AllBeEquivalentTo(222);
        Directory.Exists(_jsonDir).Should().BeFalse();
    }

    [Fact]
    public async Task Migration_DbRowAlreadyExists_DbWinsAndFileIsDeleted()
    {
        // 一度移行済み(DBに行がある)の日付は古いJSONで上書きしない
        var date = new DateOnly(2025, 1, 15);
        await CreateService().SaveAsync(MakeLog(date, _ => 333));
        WriteV2File(date, [.. Enumerable.Repeat(111, 24)]);

        // 新しいインスタンスで初期化(マイグレーション)をやり直す
        var loaded = await CreateService().LoadForDateAsync(date);

        loaded.HourlyWatts.Should().AllBeEquivalentTo(333);
        Directory.Exists(_jsonDir).Should().BeFalse();
    }

    [Fact]
    public async Task Migration_BrokenJson_RenamesToCorruptAndKeepsDirectory()
    {
        Directory.CreateDirectory(_jsonDir);
        string file = Path.Combine(_jsonDir, "2025-01-15.json");
        await File.WriteAllTextAsync(file, "{ broken json");

        var loaded = await CreateService().LoadForDateAsync(new DateOnly(2025, 1, 15));

        loaded.HourlyWatts.Should().AllBeEquivalentTo(0);
        File.Exists(file).Should().BeFalse();
        File.Exists(file + ".corrupt").Should().BeTrue();
        Directory.Exists(_jsonDir).Should().BeTrue();
    }

    [Fact]
    public async Task Migration_WrongArrayLength_RenamesToCorrupt()
    {
        // 24要素以外 (最初期V1の31要素など) は破損扱い
        var date = new DateOnly(2025, 1, 15);
        WriteV2File(date, [.. Enumerable.Repeat(100, 23)]);
        string file = Path.Combine(_jsonDir, $"{date:yyyy-MM-dd}.json");

        var loaded = await CreateService().LoadForDateAsync(date);

        loaded.HourlyWatts.Should().AllBeEquivalentTo(0);
        File.Exists(file + ".corrupt").Should().BeTrue();
    }

    [Fact]
    public async Task Migration_RunTwice_IsIdempotent()
    {
        var date = new DateOnly(2025, 1, 15);
        WriteV2File(date, [.. Enumerable.Repeat(500, 24)]);

        // 1回目の初期化で移行、2回目は移行対象なしでもエラーにならない
        (await CreateService().LoadForDateAsync(date)).HourlyWatts.Should().AllBeEquivalentTo(500);
        (await CreateService().LoadForDateAsync(date)).HourlyWatts.Should().AllBeEquivalentTo(500);
    }
}
