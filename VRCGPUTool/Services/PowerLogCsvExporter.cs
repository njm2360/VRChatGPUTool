using System.Globalization;
using System.Text;
using VRCGPUTool.Infrastructure;
using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public sealed class PowerLogCsvExporter(IPowerLogService powerLogService)
{
    public async Task ExportDayToCsvAsync(HourlyPowerLog log, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("日時,使用量(Wh)");
        for (int h = 0; h < 24; h++)
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"{log.Date:yyyy-MM-dd}T{h:D2}:00,{log.HourlyWatts[h] / 3600.0:f3}")); // Ws → Wh

        await AtomicFile.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8).ConfigureAwait(false);
    }

    public async Task ExportMonthToCsvAsync(DateOnly month, string filePath)
    {
        int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);

        var tasks = Enumerable.Range(1, daysInMonth)
            .Select(d => powerLogService.LoadForDateAsync(new DateOnly(month.Year, month.Month, d)));
        HourlyPowerLog[] logs = await Task.WhenAll(tasks).ConfigureAwait(false);

        var sb = new StringBuilder();
        sb.AppendLine("日時,使用量(Wh)");
        for (int d = 0; d < daysInMonth; d++)
        {
            double totalWh = logs[d].HourlyWatts.Sum(w => (long)w) / 3600.0; // Ws → Wh
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"{new DateOnly(month.Year, month.Month, d + 1):yyyy-MM-dd}T00:00,{totalWh:f3}"));
        }

        await AtomicFile.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8).ConfigureAwait(false);
    }
}
