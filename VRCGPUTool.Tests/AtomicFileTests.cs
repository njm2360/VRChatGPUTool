using System.IO;
using System.Text;
using FluentAssertions;
using VRCGPUTool.Infrastructure;
using Xunit;

namespace VRCGPUTool.Tests;

public class AtomicFileTests : IDisposable
{
    private readonly string _dir;

    public AtomicFileTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "vgt-atomic-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_dir))
                Directory.Delete(_dir, recursive: true);
        }
        catch
        {
            // クリーンアップ失敗は無視
        }
    }

    private IEnumerable<string> TempFiles() => Directory.EnumerateFiles(_dir, "*.tmp");

    [Fact]
    public async Task WriteAllTextAsync_DestMissing_CreatesFile()
    {
        string dest = Path.Combine(_dir, "config.json");

        await AtomicFile.WriteAllTextAsync(dest, "hello", Encoding.UTF8);

        File.Exists(dest).Should().BeTrue();
        (await File.ReadAllTextAsync(dest)).Should().Be("hello");
    }

    [Fact]
    public async Task WriteAllTextAsync_DestExists_OverwritesContent()
    {
        string dest = Path.Combine(_dir, "config.json");
        await File.WriteAllTextAsync(dest, "old content");

        await AtomicFile.WriteAllTextAsync(dest, "new content", Encoding.UTF8);

        (await File.ReadAllTextAsync(dest)).Should().Be("new content");
    }

    [Fact]
    public async Task WriteAllTextAsync_RoundTripsUtf8()
    {
        string dest = Path.Combine(_dir, "config.json");
        const string content = "日本語テスト 電力プロファイル {\"a\":1}";

        await AtomicFile.WriteAllTextAsync(dest, content, Encoding.UTF8);

        (await File.ReadAllTextAsync(dest, Encoding.UTF8)).Should().Be(content);
    }

    [Fact]
    public async Task WriteAllTextAsync_Success_LeavesNoTempFiles()
    {
        string dest = Path.Combine(_dir, "config.json");

        await AtomicFile.WriteAllTextAsync(dest, "data", Encoding.UTF8);

        TempFiles().Should().BeEmpty();
    }

    [Fact]
    public async Task WriteAllTextAsync_MoveFails_LeavesOriginalIntactAndCleansTemp()
    {
        string dest = Path.Combine(_dir, "config.json");
        await File.WriteAllTextAsync(dest, "original");

        using (var _ = new FileStream(dest, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            var act = async () =>
                await AtomicFile.WriteAllTextAsync(dest, "replacement", Encoding.UTF8);

            await act.Should().ThrowAsync<Exception>();
        }

        (await File.ReadAllTextAsync(dest)).Should().Be("original");
        TempFiles().Should().BeEmpty();
    }
}
