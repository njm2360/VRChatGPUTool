using System.IO;
using System.Text;

namespace VRCGPUTool.Infrastructure;

internal static class AtomicFile
{
    public static async Task WriteAllTextAsync(string path, string contents, Encoding encoding)
    {
        string dir = Path.GetDirectoryName(path)!;
        string tempPath = Path.Combine(dir, $"{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write,
                                                 FileShare.None, bufferSize: 4096, useAsync: true))
            await using (var writer = new StreamWriter(fs, encoding))
            {
                await writer.WriteAsync(contents).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
                fs.Flush(flushToDisk: true);
            }

            File.Move(tempPath, path, overwrite: true);
        }
        catch
        {
            TryDelete(tempPath);
            throw;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // 一時ファイルの削除失敗は無視
        }
    }
}
