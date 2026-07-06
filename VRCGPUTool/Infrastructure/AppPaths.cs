using System.IO;
using System.Reflection;

namespace VRCGPUTool.Infrastructure;

internal static class AppPaths
{
    public static readonly string DataDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     Assembly.GetExecutingAssembly().GetName().Name!);

    public static readonly string ConfigFile = Path.Combine(DataDir, "config.json");
    public static readonly string ElecFile = Path.Combine(DataDir, "elec_profile.json");
    public static readonly string PowerLogDb = Path.Combine(DataDir, "powerlog.db");

    /// <summary>旧JSON形式のディレクトリ。マイグレーション後に使用されない。</summary>
    public static readonly string PowerLogDir = Path.Combine(DataDir, "powerlog");
}
