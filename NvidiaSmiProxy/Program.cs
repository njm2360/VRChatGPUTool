using Serilog;
using VRCGPUTool.Service;

// LocalSystem で動作するため ProgramData 配下に置く (ユーザーから参照可能)
string logDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
    "VRChatGPUTool", "logs");

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(o => o.ServiceName = "VRCGPUTool NvidiaSmi Proxy");
builder.Logging.AddSerilog(new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        Path.Combine(logDir, "service-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        fileSizeLimitBytes: 10 * 1024 * 1024,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger(), dispose: true);
builder.Services.AddHostedService<NvidiaSmiWorker>();

await builder.Build().RunAsync();
