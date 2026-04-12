using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace VRCGPUTool.Services;

public sealed class GitHubUpdateCheckService : IUpdateCheckService
{
    private const string LatestReleaseUrl =
        "https://api.github.com/repos/njm2360/VRChatGPUTool/releases/latest";

    // HttpClient は static で保持してソケット枯渇を防ぐ
    private static readonly HttpClient HttpClient = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    })
    {
        Timeout = TimeSpan.FromSeconds(10),
        DefaultRequestHeaders = { { "User-Agent", "VRCGPUTool/3.0" } },
    };

    public async Task<UpdateInfo?> CheckForUpdateAsync(Version currentVersion, CancellationToken ct = default)
    {
        try
        {
            var release = await HttpClient
                .GetFromJsonAsync<GitHubRelease>(LatestReleaseUrl, ct)
                .ConfigureAwait(false);

            if (release is null) return null;

            string versionStr = release.TagName.TrimStart('v');
            if (!Version.TryParse(versionStr, out var latestVersion)) return null;

            return latestVersion > currentVersion
                ? new UpdateInfo(release.TagName, release.Body ?? "")
                : null;
        }
        catch
        {
            // ネットワークエラーはアップデート確認の失敗として静かに無視する
            return null;
        }
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")] public string TagName { get; init; } = "";
        [JsonPropertyName("body")] public string? Body { get; init; }
    }
}
