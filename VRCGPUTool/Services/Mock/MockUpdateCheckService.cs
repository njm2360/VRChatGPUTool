using VRCGPUTool.Services;

namespace VRCGPUTool.Services.Mock;

/// <summary>
/// アップデート確認のモック実装。
/// HasUpdate = true のときバナー表示を確認できる。
/// </summary>
public sealed class MockUpdateCheckService : IUpdateCheckService
{
    public bool HasUpdate { get; init; } = true;
    public string TagName { get; init; } = "v99.0.0";
    public string ReleaseNotes { get; init; } = "モックリリースノート";

    public Task<UpdateInfo?> CheckForUpdateAsync(Version currentVersion, CancellationToken ct = default)
    {
        UpdateInfo? result = HasUpdate ? new UpdateInfo(TagName, ReleaseNotes) : null;
        return Task.FromResult(result);
    }
}
