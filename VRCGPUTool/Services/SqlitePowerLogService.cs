using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using VRCGPUTool.Infrastructure;
using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public sealed class SqlitePowerLogService : IPowerLogService
{
    private readonly object _initLock = new();
    private Task? _initTask;

    private Task EnsureInitializedAsync()
    {
        var task = _initTask;
        if (task is null || task.IsFaulted || task.IsCanceled)
        {
            lock (_initLock)
            {
                task = _initTask;
                if (task is null || task.IsFaulted || task.IsCanceled)
                    task = _initTask = Task.Run(Initialize);
            }
        }
        return task;
    }

    public async Task<HourlyPowerLog> LoadForDateAsync(DateOnly date)
    {
        try
        {
            await EnsureInitializedAsync().ConfigureAwait(false);
            return await Task.Run(() => LoadForDate(date)).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is SqliteException or IOException or UnauthorizedAccessException)
        {
            return new HourlyPowerLog { Date = date };
        }
    }

    public async Task<IReadOnlyList<HourlyPowerLog>> LoadMonthAsync(DateOnly month)
    {
        try
        {
            await EnsureInitializedAsync().ConfigureAwait(false);
            return await Task.Run(() => LoadMonth(month)).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is SqliteException or IOException or UnauthorizedAccessException)
        {
            // DB・ファイル起因の失敗のみ空ログにフォールバック (バグは表面化させる)
            return CreateEmptyMonth(month);
        }
    }

    public async Task SaveAsync(HourlyPowerLog log)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);
        await Task.Run(() => Save(log)).ConfigureAwait(false);
    }

    private static void Initialize()
    {
        Directory.CreateDirectory(AppPaths.DataDir);

        using var conn = OpenConnection(setWal: true);
        CreateTable(conn);
        MigrateFromJson(conn);
    }

    private static void CreateTable(SqliteConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS power_log (
                date  TEXT NOT NULL PRIMARY KEY,
                watts BLOB NOT NULL
            )
            """;
        cmd.ExecuteNonQuery();
    }

    /// <summary>既存の JSON per-day ファイルを SQLite へ移行し、完了後にファイルを削除する。</summary>
    /// <remarks>
    /// V1: powerlog_YYYYMMDD.json — {"hourPowerLog":[...],"logdate":"..."}
    /// V2: YYYY-MM-DD.json        — [0,0,...] (int[24] 配列)
    /// </remarks>
    private static void MigrateFromJson(SqliteConnection conn)
    {
        string jsonDir = AppPaths.PowerLogDir;
        if (!Directory.Exists(jsonDir)) return;

        string[] v1Files = Directory.GetFiles(jsonDir, "powerlog_????????.json");
        string[] v2Files = Directory.GetFiles(jsonDir, "????-??-??.json");
        if (v1Files.Length == 0 && v2Files.Length == 0) return;

        using var tx = conn.BeginTransaction();
        var migratedFiles = new List<string>(v1Files.Length + v2Files.Length);

        // 同一日付に両形式がある場合は (優先順位: DB > V2 > V1)

        // V2: YYYY-MM-DD.json
        foreach (string file in v2Files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            if (!DateOnly.TryParseExact(name, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateOnly date))
                continue;
            if (TryReadFile(file) is not { } json)
                continue;
            try
            {
                int[]? data = JsonSerializer.Deserialize<int[]>(json);
                if (data is not { Length: 24 }) { MarkCorrupt(file); continue; }
                InsertRowIfAbsent(conn, tx, date, data);
                migratedFiles.Add(file);
            }
            catch (JsonException) { MarkCorrupt(file); }
        }

        // V1: powerlog_YYYYMMDD.json
        foreach (string file in v1Files)
        {
            string datePart = Path.GetFileNameWithoutExtension(file)["powerlog_".Length..];
            if (!DateOnly.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateOnly date))
                continue;
            if (TryReadFile(file) is not { } json)
                continue;
            try
            {
                var v1 = JsonSerializer.Deserialize<V1RawData>(json);
                if (v1?.HourPowerLog is not { Length: 24 }) { MarkCorrupt(file); continue; }
                InsertRowIfAbsent(conn, tx, date, v1.HourPowerLog);
                migratedFiles.Add(file);
            }
            catch (JsonException) { MarkCorrupt(file); }
        }

        tx.Commit();

        foreach (string file in migratedFiles)
            try { File.Delete(file); } catch { }

        try
        {
            if (!Directory.EnumerateFileSystemEntries(jsonDir).Any())
                Directory.Delete(jsonDir);
        }
        catch { }
    }

    private static string? TryReadFile(string file)
    {
        try
        {
            return File.ReadAllText(file);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static void MarkCorrupt(string file)
    {
        try { File.Move(file, file + ".corrupt", overwrite: true); } catch { }
    }

    private sealed class V1RawData
    {
        [System.Text.Json.Serialization.JsonPropertyName("hourPowerLog")]
        public int[]? HourPowerLog { get; set; }
    }

    // ────────────────────────────────────────────────
    // 内部 DB 操作
    // ────────────────────────────────────────────────

    private static HourlyPowerLog LoadForDate(DateOnly date)
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT watts FROM power_log WHERE date = @date";
        cmd.Parameters.AddWithValue("@date", DateKey(date));

        using var reader = cmd.ExecuteReader();
        var log = new HourlyPowerLog { Date = date };
        if (reader.Read())
            CopyBlob((byte[])reader["watts"], log);
        return log;
    }

    private static HourlyPowerLog[] LoadMonth(DateOnly month)
    {
        var logs = CreateEmptyMonth(month);

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT date, watts FROM power_log WHERE date BETWEEN @start AND @end";
        cmd.Parameters.AddWithValue("@start", DateKey(logs[0].Date));
        cmd.Parameters.AddWithValue("@end", DateKey(logs[^1].Date));

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            if (DateOnly.TryParseExact((string)reader["date"], "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
                CopyBlob((byte[])reader["watts"], logs[date.Day - 1]);
        }
        return logs;
    }

    private static HourlyPowerLog[] CreateEmptyMonth(DateOnly month)
    {
        int days = DateTime.DaysInMonth(month.Year, month.Month);
        var logs = new HourlyPowerLog[days];
        for (int d = 0; d < days; d++)
            logs[d] = new HourlyPowerLog { Date = new DateOnly(month.Year, month.Month, d + 1) };
        return logs;
    }

    private static void CopyBlob(byte[] blob, HourlyPowerLog log)
    {
        // 長さ不正 (破損行) は空ログ扱いにする
        if (blob.Length == log.HourlyWatts.Length * sizeof(int))
            MemoryMarshal.Cast<byte, int>(blob).CopyTo(log.HourlyWatts);
    }

    private static void Save(HourlyPowerLog log)
    {
        using var conn = OpenConnection();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO power_log (date, watts) VALUES (@date, @watts)
            ON CONFLICT(date) DO UPDATE SET watts = excluded.watts
            """;
        cmd.Parameters.AddWithValue("@date", DateKey(log.Date));
        cmd.Parameters.AddWithValue("@watts", ToBlob(log.HourlyWatts));
        cmd.ExecuteNonQuery();
    }

    // 移行用: 既にDBに行がある日はDB側を正とし古いJSONで上書きしない
    private static void InsertRowIfAbsent(SqliteConnection conn, SqliteTransaction tx,
                                          DateOnly date, int[] watts)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO power_log (date, watts) VALUES (@date, @watts)
            ON CONFLICT(date) DO NOTHING
            """;
        cmd.Parameters.AddWithValue("@date", DateKey(date));
        cmd.Parameters.AddWithValue("@watts", ToBlob(watts));
        cmd.ExecuteNonQuery();
    }

    private static byte[] ToBlob(int[] watts)
        => MemoryMarshal.AsBytes(watts.AsSpan()).ToArray(); // int[24] → byte[96]

    // WAL モードは DB ファイルに永続化されるが、synchronous は接続ごとの設定のため毎回設定する。
    private static SqliteConnection OpenConnection(bool setWal = false)
    {
        var conn = new SqliteConnection($"Data Source={AppPaths.PowerLogDb}");
        conn.Open();
        using var pragma = conn.CreateCommand();
        pragma.CommandText = setWal
            ? "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;"
            : "PRAGMA synchronous=NORMAL;";
        pragma.ExecuteNonQuery();
        return conn;
    }

    private static string DateKey(DateOnly date)
        => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
