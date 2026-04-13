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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Pipe server starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            NamedPipeServerStream pipe;
            try
            {
                pipe = CreatePipeServer();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create pipe server.");
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
            return new PipeResponse(Ok: false, Error: "Invalid JSON request.");
        }

        if (req is null)
            return new PipeResponse(Ok: false, Error: "Empty request.");

        try
        {
            return req.Cmd switch
            {
                "ping" => new PipeResponse(Ok: true),

                "query" => new PipeResponse(Ok: true,
                    Gpus: await NvidiaSmiExecutor.QueryAllGpusAsync(ct).ConfigureAwait(false)),

                "set-power-limit" when req.Uuid is not null && req.Watts is not null =>
                    await RunWriteAsync(() =>
                        NvidiaSmiExecutor.SetPowerLimitAsync(req.Uuid, req.Watts.Value, ct)),

                "set-core-clock" when req.Uuid is not null && req.MinCoreClock is not null && req.MaxCoreClock is not null =>
                    await RunWriteAsync(() =>
                        NvidiaSmiExecutor.SetCoreClockLimitAsync(req.Uuid, req.MinCoreClock.Value, req.MaxCoreClock.Value, ct)),

                "reset-core-clock" when req.Uuid is not null =>
                    await RunWriteAsync(() =>
                        NvidiaSmiExecutor.ResetCoreClockLimitAsync(req.Uuid, ct)),

                _ => new PipeResponse(Ok: false, Error: $"Unknown command: {req.Cmd}"),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "nvidia-smi command failed: {Cmd}", req.Cmd);
            return new PipeResponse(Ok: false, Error: ex.Message);
        }
    }

    private static async Task<PipeResponse> RunWriteAsync(Func<Task> action)
    {
        await action().ConfigureAwait(false);
        return new PipeResponse(Ok: true);
    }
}
