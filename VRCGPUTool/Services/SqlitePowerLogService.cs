using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using VRCGPUTool.Infrastructure;
using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public sealed class SqlitePowerLogService : IPowerLogService
{
    private readonly Lazy<Task> _initTask = new(
        () => Task.Run(Initialize),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public async Task<HourlyPowerLog> LoadForDateAsync(DateOnly date)
    {
        await _initTask.Value.ConfigureAwait(false);
        return await Task.Run(() => LoadForDate(date)).ConfigureAwait(false);
    }

    public async Task SaveAsync(HourlyPowerLog log)
    {
        await _initTask.Value.ConfigureAwait(false);
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

        // V1: powerlog_YYYYMMDD.json
        foreach (string file in v1Files)
        {
            string datePart = Path.GetFileNameWithoutExtension(file)["powerlog_".Length..];
            if (!DateOnly.TryParseExact(datePart, "yyyyMMdd", null,
                    System.Globalization.DateTimeStyles.None, out DateOnly date))
                continue;
            try
            {
                var v1 = JsonSerializer.Deserialize<V1RawData>(File.ReadAllText(file));
                if (v1?.HourPowerLog is not { Length: 24 }) continue;
                UpsertRow(conn, tx, date, v1.HourPowerLog);
                migratedFiles.Add(file);
            }
            catch { /* 壊れたファイルはスキップ（削除もしない） */ }
        }

        // V2: YYYY-MM-DD.json
        foreach (string file in v2Files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            if (!DateOnly.TryParseExact(name, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out DateOnly date))
                continue;
            try
            {
                int[]? data = JsonSerializer.Deserialize<int[]>(File.ReadAllText(file));
                if (data is not { Length: 24 }) continue;
                UpsertRow(conn, tx, date, data);
                migratedFiles.Add(file);
            }
            catch { /* 壊れたファイルはスキップ（削除もしない） */ }
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

    private sealed class V1RawData
    {
        [System.Text.Json.Serialization.JsonPropertyName("hourPowerLog")]
        public int[]? HourPowerLog { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("logdate")]
        public DateTime LogDate { get; set; }
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
        {
            var blob = (byte[])reader["watts"];
            MemoryMarshal.Cast<byte, int>(blob).CopyTo(log.HourlyWatts);
        }
        return log;
    }

    private static void Save(HourlyPowerLog log)
    {
        using var conn = OpenConnection();
        UpsertRow(conn, null, log.Date, log.HourlyWatts);
    }

    private static void UpsertRow(SqliteConnection conn, SqliteTransaction? tx,
                                   DateOnly date, int[] watts)
    {
        byte[] blob = MemoryMarshal.AsBytes(watts.AsSpan()).ToArray(); // int[24] → byte[96]

        using var cmd = conn.CreateCommand();
        if (tx is not null) cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO power_log (date, watts) VALUES (@date, @watts)
            ON CONFLICT(date) DO UPDATE SET watts = excluded.watts
            """;
        cmd.Parameters.AddWithValue("@date", DateKey(date));
        cmd.Parameters.AddWithValue("@watts", blob);
        cmd.ExecuteNonQuery();
    }

    // WAL モードは DB ファイルに永続化されるため、初回のみ設定すれば十分。
    private static SqliteConnection OpenConnection(bool setWal = false)
    {
        var conn = new SqliteConnection($"Data Source={AppPaths.PowerLogDb}");
        conn.Open();
        if (setWal)
        {
            using var pragma = conn.CreateCommand();
            pragma.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
            pragma.ExecuteNonQuery();
        }
        return conn;
    }

    private static string DateKey(DateOnly date) => date.ToString("yyyy-MM-dd");
}
