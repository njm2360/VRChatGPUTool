using Microsoft.Win32;

namespace VRCGPUTool.Services;

public sealed class StartupService
{
    private const string RegistryKeyPath =
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "VRCGPUTool";

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: false);
        return key?.GetValue(ValueName) is not null;
    }

    public void Enable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true)
            ?? throw new InvalidOperationException("スタートアップ用のレジストリキーを開けませんでした。");
        key.SetValue(ValueName, $"\"{GetExePath()}\"");
    }

    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    private static string GetExePath()
        => Environment.ProcessPath
           ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
           ?? throw new InvalidOperationException("実行ファイルのパスを取得できませんでした。");
}
