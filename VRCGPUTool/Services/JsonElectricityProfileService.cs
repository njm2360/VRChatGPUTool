using System.IO;
using System.Text;
using System.Text.Json;
using VRCGPUTool.Infrastructure;
using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public sealed class JsonElectricityProfileService : IElectricityProfileService
{
    private static readonly string FileName = AppPaths.ElecFile;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task<ElectricityProfile> LoadAsync()
    {
        if (!File.Exists(FileName))
            return new ElectricityProfile();

        try
        {
            await using var fs = File.OpenRead(FileName);
            var profile = await JsonSerializer.DeserializeAsync<ElectricityProfile>(fs, JsonOptions)
                .ConfigureAwait(false);
            return profile ?? new ElectricityProfile();
        }
        catch (JsonException)
        {
            return new ElectricityProfile();
        }
    }

    public async Task SaveAsync(ElectricityProfile profile)
    {
        Directory.CreateDirectory(AppPaths.DataDir);
        string json = JsonSerializer.Serialize(profile, JsonOptions);
        await File.WriteAllTextAsync(FileName, json, Encoding.UTF8).ConfigureAwait(false);
    }
}
