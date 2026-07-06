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
        catch
        {
            // 既存ファイルを退避してからデフォルト値で起動
            BackupExistingFile();
            return new ElectricityProfile();
        }
    }

    private static void BackupExistingFile()
    {
        try
        {
            if (File.Exists(FileName))
                File.Copy(FileName, FileName + ".bak", overwrite: true);
        }
        catch
        {
            // 退避失敗は無視
        }
    }

    public async Task SaveAsync(ElectricityProfile profile)
    {
        Directory.CreateDirectory(AppPaths.DataDir);
        string json = JsonSerializer.Serialize(profile, JsonOptions);
        await AtomicFile.WriteAllTextAsync(FileName, json, Encoding.UTF8).ConfigureAwait(false);
    }
}
