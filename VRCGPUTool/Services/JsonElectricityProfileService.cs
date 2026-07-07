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

    private bool _suppressSave;

    public async Task<ElectricityProfile> LoadAsync()
    {
        if (!File.Exists(FileName))
            return new ElectricityProfile();

        string json;
        try
        {
            json = await File.ReadAllTextAsync(FileName, Encoding.UTF8).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _suppressSave = true;
            return new ElectricityProfile();
        }

        try
        {
            var profile = JsonSerializer.Deserialize<ElectricityProfile>(json, JsonOptions);
            return profile ?? new ElectricityProfile();
        }
        catch (JsonException)
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
        if (_suppressSave)
            return;

        Directory.CreateDirectory(AppPaths.DataDir);
        string json = JsonSerializer.Serialize(profile, JsonOptions);
        await AtomicFile.WriteAllTextAsync(FileName, json, Encoding.UTF8).ConfigureAwait(false);
    }
}
