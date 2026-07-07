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
            // 内容の破損は読み直しても直らないため、退避してからデフォルト値で起動
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
        if (_suppressSave)
            return;

        Directory.CreateDirectory(AppPaths.DataDir);
        string json = JsonSerializer.Serialize(config, JsonOptions);
        await AtomicFile.WriteAllTextAsync(FileName, json, Encoding.UTF8).ConfigureAwait(false);
    }
}
