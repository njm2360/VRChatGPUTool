using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using VRCGPUTool.Infrastructure;
using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public sealed class JsonConfigService(ILogger<JsonConfigService> logger) : IConfigService
{
    private static readonly string FileName = AppPaths.ConfigFile;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>読めなかっただけの正常な設定ファイルをデフォルト値で上書きしないよう、
    /// 読み取り失敗時はセッション中の保存を抑止する。</summary>
    private bool _suppressSave;

    public async Task<AppConfig> LoadAsync()
    {
        if (!File.Exists(FileName))
            return new AppConfig();

        string json;
        try
        {
            json = await File.ReadAllTextAsync(FileName, Encoding.UTF8).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 一時的な I/O 障害の可能性があるためファイルには触れずデフォルト値で起動
            logger.LogError(ex, "Failed to read config file. Starting with defaults (saving suppressed): {Path}", FileName);
            _suppressSave = true;
            return new AppConfig();
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Version フィールドがない → V1 (旧 WinForms 実装) → マイグレーション
            if (!root.TryGetProperty("Version", out _))
            {
                logger.LogInformation("Migrating config file from V1 format: {Path}", FileName);
                var migrated = ConfigMigration.MigrateV1ToV2(root);
                try
                {
                    await SaveAsync(migrated).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // 保存失敗は無視
                    logger.LogWarning(ex, "Failed to save migrated config: {Path}", FileName);
                }
                return migrated;
            }

            var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
            return config ?? new AppConfig();
        }
        catch (Exception ex)
        {
            // 内容の破損は読み直しても直らないため、退避してからデフォルト値で起動
            logger.LogError(ex, "Config file is corrupt. Backing up and starting with defaults: {Path}", FileName);
            BackupExistingFile();
            return new AppConfig();
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
            logger.LogWarning(ex, "Failed to back up corrupt config file: {Path}", FileName);
        }
    }

    public async Task SaveAsync(AppConfig config)
    {
        if (_suppressSave)
        {
            logger.LogWarning("Config save skipped (suppressed by earlier read failure).");
            return;
        }

        Directory.CreateDirectory(AppPaths.DataDir);
        string json = JsonSerializer.Serialize(config, JsonOptions);
        await AtomicFile.WriteAllTextAsync(FileName, json, Encoding.UTF8).ConfigureAwait(false);
    }
}
