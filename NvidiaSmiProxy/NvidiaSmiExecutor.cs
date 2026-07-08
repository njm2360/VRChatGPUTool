using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using VRCGPUTool.Shared;

namespace VRCGPUTool.Service;

public sealed partial class NvidiaSmiExecutor
{
    [GeneratedRegex(@"^(GPU|MIG)-[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$")]
    private static partial Regex UuidPattern();

    private static void ValidateUuid(string uuid)
    {
        if (!UuidPattern().IsMatch(uuid))
            throw new ArgumentException($"Invalid GPU UUID format: '{uuid}'", nameof(uuid));
    }

    private static readonly string NvidiaSmiPath =
        Path.Combine(Environment.SystemDirectory, "nvidia-smi.exe");

    private static readonly string[] QueryColumns =
    [
        "name", "uuid",
        "power.limit", "power.min_limit", "power.max_limit", "power.default_limit",
        "utilization.gpu", "temperature.gpu",
        "power.draw", "clocks.gr", "clocks.mem",
    ];

    public static bool IsAvailable() => File.Exists(NvidiaSmiPath);

    public static async Task<IReadOnlyList<GpuStatusDto>> QueryAllGpusAsync(CancellationToken ct = default)
    {
        string query = string.Join(",", QueryColumns);
        string output = await RunAsync($"--query-gpu={query} --format=csv,noheader,nounits", ct)
            .ConfigureAwait(false);
        return ParseOutput(output);
    }

    public static Task SetPowerLimitAsync(string uuid, int watts, CancellationToken ct = default)
    {
        ValidateUuid(uuid);
        return RunAsync($"-pl {watts} --id={uuid}", ct);
    }

    public static Task SetCoreClockLimitAsync(string uuid, int minCoreClock, int maxCoreClock, CancellationToken ct = default)
    {
        ValidateUuid(uuid);
        return RunAsync($"-lgc {minCoreClock},{maxCoreClock} --id={uuid}", ct);
    }

    public static Task ResetCoreClockLimitAsync(string uuid, CancellationToken ct = default)
    {
        ValidateUuid(uuid);
        return RunAsync($"-rgc --id={uuid}", ct);
    }

    private static async Task<string> RunAsync(string arguments, CancellationToken ct)
    {
        var psi = new ProcessStartInfo(NvidiaSmiPath)
        {
            Arguments = arguments,
            WorkingDirectory = Environment.SystemDirectory,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8,
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linked.CancelAfter(TimeSpan.FromSeconds(10));

        var stderrTask = process.StandardError.ReadToEndAsync(CancellationToken.None);
        string output;
        try
        {
            output = await process.StandardOutput.ReadToEndAsync(linked.Token).ConfigureAwait(false);
            await process.WaitForExitAsync(linked.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
            throw;
        }

        string stderr = await stderrTask.ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            string detail = string.Join(" ",
                new[] { output.Trim(), stderr.Trim() }.Where(s => s.Length > 0));
            throw new InvalidOperationException(
                $"nvidia-smi exited with code {process.ExitCode}. {detail}".TrimEnd());
        }

        return output;
    }

    private static List<GpuStatusDto> ParseOutput(string output)
    {
        var result = new List<GpuStatusDto>();
        using var reader = new StringReader(output);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (TryParseLine(line, out var dto))
                result.Add(dto);
        }
        return result;
    }

    internal static bool TryParseLine(string line, out GpuStatusDto dto)
    {
        dto = null!;
        var v = line.Split(',');
        if (v.Length != QueryColumns.Length) return false;

        if (!TryParseDouble(v[2], out var powerLimit) ||
            !TryParseDouble(v[3], out var powerMin) ||
            !TryParseDouble(v[4], out var powerMax) ||
            !TryParseDouble(v[5], out var powerDefault) ||
            !TryParseDouble(v[6], out var utilization) ||
            !TryParseDouble(v[7], out var temp) ||
            !TryParseDouble(v[8], out var powerDraw) ||
            !TryParseDouble(v[9], out var coreClock) ||
            !TryParseDouble(v[10], out var memClock))
            return false;

        dto = new GpuStatusDto(
            Name: v[0].Trim(),
            Uuid: v[1].Trim(),
            PowerLimit: (int)Math.Round(powerLimit),
            PowerLimitMin: (int)Math.Round(powerMin),
            PowerLimitMax: (int)Math.Round(powerMax),
            PowerLimitDefault: (int)Math.Round(powerDefault),
            GpuUtilization: (int)utilization,
            CoreTemperature: (int)temp,
            PowerDraw: (int)powerDraw,
            CoreClock: (int)coreClock,
            MemoryClock: (int)memClock);
        return true;
    }

    internal static bool TryParseDouble(string s, out double value) =>
        double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
