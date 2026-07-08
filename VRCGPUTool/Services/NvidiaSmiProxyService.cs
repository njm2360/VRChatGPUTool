using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using VRCGPUTool.Models;
using VRCGPUTool.Shared;

namespace VRCGPUTool.Services;

public sealed class NvidiaSmiProxyService : INvidiaSmiService, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly SemaphoreSlim _lock = new(1, 1);
    private NamedPipeClientStream? _pipe;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public bool IsAvailable()
    {
        try
        {
            using var pipe = new NamedPipeClientStream(".", PipeNames.NvidiaSmi, PipeDirection.InOut);
            pipe.Connect(timeout: 500);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<GpuStatus>> QueryAllGpusAsync(CancellationToken ct = default)
    {
        var response = await SendAsync(new PipeRequest(Cmd: "query"), ct).ConfigureAwait(false);

        if (!response.Ok)
            throw new InvalidOperationException(response.Error ?? "query failed.");

        return (response.Gpus ?? [])
            .Select(ToModel)
            .ToList();
    }

    public Task SetPowerLimitAsync(string uuid, int watts, CancellationToken ct = default)
        => SendWriteAsync(new PipeRequest(Cmd: "set-power-limit", Uuid: uuid, Watts: watts), ct);

    public Task SetCoreClockLimitAsync(string uuid, int minCoreClock, int maxCoreClock, CancellationToken ct = default)
        => SendWriteAsync(new PipeRequest(Cmd: "set-core-clock", Uuid: uuid, MinCoreClock: minCoreClock, MaxCoreClock: maxCoreClock), ct);

    public Task ResetCoreClockLimitAsync(string uuid, CancellationToken ct = default)
        => SendWriteAsync(new PipeRequest(Cmd: "reset-core-clock", Uuid: uuid), ct);

    // ── internal helpers ────────────────────────────────────────────────────

    private async Task SendWriteAsync(PipeRequest request, CancellationToken ct)
    {
        var response = await SendAsync(request, ct).ConfigureAwait(false);
        if (!response.Ok)
            throw new InvalidOperationException(response.Error ?? $"{request.Cmd} failed.");
    }

    private async Task<PipeResponse> SendAsync(PipeRequest request, CancellationToken ct)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await EnsureConnectedAsync(ct).ConfigureAwait(false);

            try
            {
                try
                {
                    return await ExchangeAsync(request, ct).ConfigureAwait(false);
                }
                catch (IOException)
                {
                    // 接続が切れていたら再接続して1回リトライ
                    ResetConnection();
                    await EnsureConnectedAsync(ct).ConfigureAwait(false);
                    return await ExchangeAsync(request, ct).ConfigureAwait(false);
                }
            }
            catch
            {
                // 交換に失敗した接続は応答がずれる可能性があるため破棄
                ResetConnection();
                throw;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (_pipe?.IsConnected == true) return;

        ResetConnection();
        var pipe = new NamedPipeClientStream(".", PipeNames.NvidiaSmi,
            PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
        await pipe.ConnectAsync(timeout: 5000, ct).ConfigureAwait(false);
        _pipe = pipe;
        _reader = new StreamReader(pipe, new UTF8Encoding(false), leaveOpen: true);
        _writer = new StreamWriter(pipe, new UTF8Encoding(false), leaveOpen: true) { AutoFlush = true };
    }

    private async Task<PipeResponse> ExchangeAsync(PipeRequest request, CancellationToken ct)
    {
        string requestJson = JsonSerializer.Serialize(request, JsonOpts);
        await _writer!.WriteLineAsync(requestJson.AsMemory(), ct).ConfigureAwait(false);
        await _writer.FlushAsync(ct).ConfigureAwait(false);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));

        string? responseJson = await _reader!.ReadLineAsync(timeoutCts.Token).ConfigureAwait(false);
        if (responseJson is null)
            throw new IOException("Service returned empty response.");

        return JsonSerializer.Deserialize<PipeResponse>(responseJson, JsonOpts)
            ?? throw new InvalidOperationException("Failed to deserialize service response.");
    }

    private void ResetConnection()
    {
        try { _reader?.Dispose(); } catch { }
        try { _writer?.Dispose(); } catch { }
        try { _pipe?.Dispose(); } catch { }
        _reader = null;
        _writer = null;
        _pipe = null;
    }

    public ValueTask DisposeAsync()
    {
        ResetConnection();
        _lock.Dispose();
        return ValueTask.CompletedTask;
    }

    private static GpuStatus ToModel(GpuStatusDto d) => new(
        Name: d.Name,
        Uuid: d.Uuid,
        PowerLimit: d.PowerLimit,
        PowerLimitMin: d.PowerLimitMin,
        PowerLimitMax: d.PowerLimitMax,
        PowerLimitDefault: d.PowerLimitDefault,
        GpuUtilization: d.GpuUtilization,
        CoreTemperature: d.CoreTemperature,
        PowerDraw: d.PowerDraw,
        CoreClock: d.CoreClock,
        MemoryClock: d.MemoryClock);
}
