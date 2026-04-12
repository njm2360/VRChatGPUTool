using VRCGPUTool.Service;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(o => o.ServiceName = "VRCGPUTool NvidiaSmi Proxy");
builder.Services.AddHostedService<NvidiaSmiWorker>();

await builder.Build().RunAsync();
