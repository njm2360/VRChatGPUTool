using VRCGPUTool.Models;
using VRCGPUTool.Services;

namespace VRCGPUTool.Services.Mock;

public sealed class MockNvidiaSmiService : INvidiaSmiService
{
    private static readonly Random Rng = Random.Shared;

    private readonly int[] _powerLimits = [200, 180];

    private static readonly GpuStatus[] Base =
    [
        new(
            Name: "NVIDIA GeForce RTX 5090",
            Uuid: "GPU-00000000-0000-0000-0000-000000000001",
            PowerLimit: 300, PowerLimitMin: 100, PowerLimitMax: 600, PowerLimitDefault: 450,
            GpuUtilization: 0, CoreTemperature: 0, PowerDraw: 0, CoreClock: 0, MemoryClock: 0),
        new(
            Name: "NVIDIA GeForce RTX 3060",
            Uuid: "GPU-00000000-0000-0000-0000-000000000002",
            PowerLimit: 150, PowerLimitMin: 100, PowerLimitMax: 200, PowerLimitDefault: 180,
            GpuUtilization: 0, CoreTemperature: 0, PowerDraw: 0, CoreClock: 0, MemoryClock: 0),
    ];

    public bool IsAvailable() => true;

    public Task<IReadOnlyList<GpuStatus>> QueryAllGpusAsync(CancellationToken ct = default)
    {
        IReadOnlyList<GpuStatus> result = Base.Select((b, i) => b with
        {
            PowerLimit = _powerLimits[i],
            GpuUtilization = Rng.Next(30, 100),
            CoreTemperature = Rng.Next(55, 85),
            PowerDraw = Rng.Next(80, _powerLimits[i]),
            CoreClock = Rng.Next(1800, 2600),
            MemoryClock = Rng.Next(9000, 10000),
        }).ToList();

        return Task.FromResult(result);
    }

    public Task SetPowerLimitAsync(string uuid, int watts, CancellationToken ct = default)
    {
        int i = IndexOf(uuid);
        if (i >= 0) _powerLimits[i] = watts;
        return Task.CompletedTask;
    }

    public Task SetCoreClockLimitAsync(string uuid, int minCoreClock, int maxCoreClock, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task ResetCoreClockLimitAsync(string uuid, CancellationToken ct = default)
        => Task.CompletedTask;

    private static int IndexOf(string uuid)
    {
        for (int i = 0; i < Base.Length; i++)
            if (Base[i].Uuid == uuid) return i;
        return -1;
    }
}
