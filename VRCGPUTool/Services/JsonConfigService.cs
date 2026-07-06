using System.IO;
using System.Text;
using System.Text.Json;
using VRCGPUTool.Infrastructure;
using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public sealed class JsonConfigService : IConfigService
{
    private static readonly string FileName = AppPaths.ConfigFile;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task<AppConfig> LoadAsync()
    {
        if (!File.Exists(FileName))
            return new AppConfig();

        try
        {
            string json = await File.ReadAllTextAsync(FileName, Encoding.UTF8).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Version フィールドがない → V1 (旧 WinForms 実装) → マイグレーション
            if (!root.TryGetProperty("Version", out _))
            {
                var migrated = ConfigMigration.MigrateV1ToV2(root);
                try
                {
                    await SaveAsync(migrated).ConfigureAwait(false);
                }
                catch
                {
                    // 保存失敗は無視
                }
                return migrated;
            }

            var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
            return config ?? new AppConfig();
        }
        catch
        {
            // 既存ファイルを退避してからデフォルト値で起動
            BackupExistingFile();
            return new AppConfig();
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

    public async Task SaveAsync(AppConfig config)
    {
        Directory.CreateDirectory(AppPaths.DataDir);
        string json = JsonSerializer.Serialize(config, JsonOptions);
        await AtomicFile.WriteAllTextAsync(FileName, json, Encoding.UTF8).ConfigureAwait(false);
    }
}
