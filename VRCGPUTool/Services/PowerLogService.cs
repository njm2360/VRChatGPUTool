using System.IO;
using System.Text;
using System.Text.Json;
using VRCGPUTool.Infrastructure;
using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public sealed class PowerLogService : IPowerLogService
{
    private static readonly string LogDirectory = AppPaths.PowerLogDir;

    public async Task<HourlyPowerLog> LoadForDateAsync(DateOnly date)
    {
        string path = GetLogPath(date);
        if (!File.Exists(path))
            return new HourlyPowerLog { Date = date };

        try
        {
            await using var fs = File.OpenRead(path);
            var data = await JsonSerializer.DeserializeAsync<int[]>(fs).ConfigureAwait(false);

            var log = new HourlyPowerLog { Date = date };
            if (data is { Length: 24 })
                Array.Copy(data, log.HourlyWatts, 24);
            return log;
        }
        catch
        {
            return new HourlyPowerLog { Date = date };
        }
    }

    public async Task SaveAsync(HourlyPowerLog log)
    {
        Directory.CreateDirectory(LogDirectory);
        string json = JsonSerializer.Serialize(log.HourlyWatts);
        await File.WriteAllTextAsync(GetLogPath(log.Date), json, Encoding.UTF8)
            .ConfigureAwait(false);
    }

    public async Task ExportToCsvAsync(HourlyPowerLog log, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{log.Date.Year}年{log.Date.Month}月{log.Date.Day}日");
        sb.AppendLine("時,使用量(Wh)");
        for (int h = 0; h < 24; h++)
            sb.AppendLine($"{h},{log.HourlyWatts[h] / 3600.0:f2}");

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8).ConfigureAwait(false);
    }

    public async Task ExportMonthToCsvAsync(DateOnly month, string filePath)
    {
        int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
        var sb = new StringBuilder();
        sb.AppendLine($"{month.Year}年{month.Month}月");
        sb.AppendLine("日,使用量(Wh)");
        for (int d = 1; d <= daysInMonth; d++)
        {
            var log = await LoadForDateAsync(new DateOnly(month.Year, month.Month, d)).ConfigureAwait(false);
            double totalWh = log.HourlyWatts.Sum() / 3600.0;
            sb.AppendLine($"{d},{totalWh:f2}");
        }
        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8).ConfigureAwait(false);
    }

    private static string GetLogPath(DateOnly date)
        => Path.Combine(LogDirectory, $"{date:yyyy-MM-dd}.json");
}
