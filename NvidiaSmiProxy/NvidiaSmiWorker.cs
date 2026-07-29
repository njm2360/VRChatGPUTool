using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using VRCGPUTool.Shared;

namespace VRCGPUTool.Service;

public sealed class NvidiaSmiWorker(
    ILogger<NvidiaSmiWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private string? _lastPipeCreateErrorMessage;
    private string? _lastQueryErrorMessage;
    private bool _parseFailureWarned;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Pipe server starting.");

        if (!NvidiaSmiExecutor.IsAvailable())
            logger.LogWarning("nvidia-smi.exe not found in {Dir}. GPU commands will fail.",
                Environment.SystemDirectory);

        while (!stoppingToken.IsCancellationRequested)
        {
            NamedPipeServerStream pipe;
            try
            {
                pipe = CreatePipeServer();
                if (_lastPipeCreateErrorMessage is not null)
                {
                    _lastPipeCreateErrorMessage = null;
                    logger.LogInformation("Pipe server creation recovered.");
                }
            }
            catch (Exception ex)
            {
                // 同一エラーの継続中は初回のみ記録する
                if (ex.Message != _lastPipeCreateErrorMessage)
                {
                    _lastPipeCreateErrorMessage = ex.Message;
                    logger.LogError(ex, "Failed to create pipe server.");
                }
                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
                continue;
            }

            try
            {
                await pipe.WaitForConnectionAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                pipe.Dispose();
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error waiting for connection.");
                pipe.Dispose();
                continue;
            }

            var connTask = HandleConnectionAsync(pipe, stoppingToken);
            _ = connTask.ContinueWith(t =>
                logger.LogError(t.Exception, "HandleConnectionAsync faulted."),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
        }

        logger.LogInformation("Pipe server stopped.");
    }

    private static NamedPipeServerStream CreatePipeServer()
    {
        var security = new PipeSecurity();
        security.AddAccessRule(new PipeAccessRule(
            WindowsIdentity.GetCurrent().User!,
            PipeAccessRights.FullControl,
            AccessControlType.Allow));
        security.AddAccessRule(new PipeAccessRule(
            new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
            PipeAccessRights.ReadWrite,
            AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(
            PipeNames.NvidiaSmi,
            PipeDirection.InOut,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous | PipeOptions.WriteThrough,
            inBufferSize: 0,
            outBufferSize: 0,
            security);
    }

    private async Task HandleConnectionAsync(NamedPipeServerStream pipe, CancellationToken ct)
    {
        await using (pipe)
        {
            try
            {
                using var reader = new StreamReader(pipe, new UTF8Encoding(false), leaveOpen: true);
                await using var writer = new StreamWriter(pipe, new UTF8Encoding(false), leaveOpen: true);

                // 1接続で複数リクエストをクライアントが切断するまで処理し続ける
                while (!ct.IsCancellationRequested)
                {
                    string? line = await reader.ReadLineAsync(ct).ConfigureAwait(false);
                    if (line is null) break; // クライアントが切断

                    PipeResponse response = await DispatchAsync(line, ct).ConfigureAwait(false);
                    string json = JsonSerializer.Serialize(response, JsonOpts);
                    await writer.WriteLineAsync(json.AsMemory(), ct).ConfigureAwait(false);
                    await writer.FlushAsync(ct).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
            catch (IOException) { }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling pipe connection.");
            }
        }
    }

    private async Task<PipeResponse> DispatchAsync(string requestJson, CancellationToken ct)
    {
        PipeRequest? req;
        try
        {
            req = JsonSerializer.Deserialize<PipeRequest>(requestJson, JsonOpts);
        }
        catch
        {
            logger.LogWarning("Invalid JSON request received: {Request}", Truncate(requestJson));
            return new PipeResponse(Ok: false, Error: "Invalid JSON request.");
        }

        if (req is null)
        {
            logger.LogWarning("Empty request received.");
            return new PipeResponse(Ok: false, Error: "Empty request.");
        }

        try
        {
            return req.Cmd switch
            {
                "ping" => new PipeResponse(Ok: true),

                "query" => await QueryAsync(ct),

                "set-power-limit" when req.Uuid is not null && req.Watts is not null =>
                    await RunWriteAsync($"set-power-limit {req.Watts} W ({req.Uuid})", () =>
                        NvidiaSmiExecutor.SetPowerLimitAsync(req.Uuid, req.Watts.Value, ct)),

                "set-core-clock" when req.Uuid is not null && req.MinCoreClock is not null && req.MaxCoreClock is not null =>
                    await RunWriteAsync($"set-core-clock {req.MinCoreClock}-{req.MaxCoreClock} MHz ({req.Uuid})", () =>
                        NvidiaSmiExecutor.SetCoreClockLimitAsync(req.Uuid, req.MinCoreClock.Value, req.MaxCoreClock.Value, ct)),

                "reset-core-clock" when req.Uuid is not null =>
                    await RunWriteAsync($"reset-core-clock ({req.Uuid})", () =>
                        NvidiaSmiExecutor.ResetCoreClockLimitAsync(req.Uuid, ct)),

                _ => UnknownCommand(req.Cmd),
            };
        }
        catch (Exception ex)
        {
            // 毎秒来る query は同一エラーの継続中は初回のみ記録する
            bool repeated = req.Cmd == "query" && ex.Message == _lastQueryErrorMessage;
            if (req.Cmd == "query")
                _lastQueryErrorMessage = ex.Message;
            if (!repeated)
                logger.LogError(ex, "nvidia-smi command failed: {Cmd}", req.Cmd);
            return new PipeResponse(Ok: false, Error: ex.Message);
        }
    }

    private async Task<PipeResponse> QueryAsync(CancellationToken ct)
    {
        var (gpus, rawOutput) = await NvidiaSmiExecutor.QueryAllGpusAsync(ct).ConfigureAwait(false);

        // 出力があるのに1件もパースできない場合 (Laptop GPU の [N/A] や形式変更) は初回のみ記録する
        if (gpus.Count == 0 && !string.IsNullOrWhiteSpace(rawOutput))
        {
            if (!_parseFailureWarned)
            {
                _parseFailureWarned = true;
                logger.LogWarning("nvidia-smi output could not be parsed: {Output}",
                    Truncate(rawOutput.Trim(), 500));
            }
        }
        else if (gpus.Count > 0)
        {
            _parseFailureWarned = false;
        }

        if (_lastQueryErrorMessage is not null)
        {
            _lastQueryErrorMessage = null;
            logger.LogInformation("GPU query recovered.");
        }
        return new PipeResponse(Ok: true, Gpus: gpus);
    }

    private async Task<PipeResponse> RunWriteAsync(string description, Func<Task> action)
    {
        await action().ConfigureAwait(false);
        logger.LogInformation("Executed {Command}", description);
        return new PipeResponse(Ok: true);
    }

    private PipeResponse UnknownCommand(string cmd)
    {
        logger.LogWarning("Unknown or malformed command received: {Cmd}", cmd);
        return new PipeResponse(Ok: false, Error: $"Unknown command: {cmd}");
    }

    private static string Truncate(string s, int max = 200)
        => s.Length <= max ? s : s[..max] + "...";
}
