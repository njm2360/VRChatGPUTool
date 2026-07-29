using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using VRCGPUTool.Infrastructure;
using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public sealed class JsonElectricityProfileService(ILogger<JsonElectricityProfileService> logger) : IElectricityProfileService
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
            logger.LogError(ex, "Failed to read electricity profile. Starting with defaults (saving suppressed): {Path}", FileName);
            _suppressSave = true;
            return new ElectricityProfile();
        }

        try
        {
            var profile = JsonSerializer.Deserialize<ElectricityProfile>(json, JsonOptions);
            return profile ?? new ElectricityProfile();
        }
        catch (JsonException ex)
        {
            // 既存ファイルを退避してからデフォルト値で起動
            logger.LogError(ex, "Electricity profile is corrupt. Backing up and starting with defaults: {Path}", FileName);
            BackupExistingFile();
            return new ElectricityProfile();
        }
    }

    private void BackupExistingFile()
    {
        try
        {
            if (File.Exists(FileName))
                File.Copy(FileName, FileName + ".bak", overwrite: true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to back up corrupt electricity profile: {Path}", FileName);
        }
    }

    public async Task SaveAsync(ElectricityProfile profile)
    {
        if (_suppressSave)
        {
            logger.LogWarning("Electricity profile save skipped (suppressed by earlier read failure).");
            return;
        }

        Directory.CreateDirectory(AppPaths.DataDir);
        string json = JsonSerializer.Serialize(profile, JsonOptions);
        await AtomicFile.WriteAllTextAsync(FileName, json, Encoding.UTF8).ConfigureAwait(false);
    }
}
