namespace VRCGPUTool.Services;

public sealed record UpdateInfo(string TagName, string ReleaseNotes);

public interface IUpdateCheckService
{
    Task<UpdateInfo?> CheckForUpdateAsync(Version currentVersion, CancellationToken ct = default);
}
