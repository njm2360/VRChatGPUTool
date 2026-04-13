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
        string fileToLoad = File.Exists(FileName) ? FileName : string.Empty;

        if (fileToLoad == string.Empty)
            return new AppConfig();

        try
        {
            string json = await File.ReadAllTextAsync(fileToLoad, Encoding.UTF8).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Version フィールドがない → V1 (旧 WinForms 実装) → マイグレーション
            if (!root.TryGetProperty("Version", out _))
            {
                var migrated = ConfigMigration.MigrateV1ToV2(root);
                // マイグレーション済み設定を即座に V2 形式で保存
                await SaveAsync(migrated).ConfigureAwait(false);
                return migrated;
            }

            var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
            return config ?? new AppConfig();
        }
        catch (JsonException)
        {
            // 設定ファイルが壊れている場合はデフォルト値で起動
            return new AppConfig();
        }
    }

    public async Task SaveAsync(AppConfig config)
    {
        Directory.CreateDirectory(AppPaths.DataDir);
        string json = JsonSerializer.Serialize(config, JsonOptions);
        await File.WriteAllTextAsync(FileName, json, Encoding.UTF8).ConfigureAwait(false);
    }
}
