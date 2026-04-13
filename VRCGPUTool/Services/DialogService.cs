using System.Windows;

namespace VRCGPUTool.Services;

public sealed class DialogService : IDialogService
{
    public void ShowError(string message, string title = "エラー")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public void ShowWarning(string message, string title = "警告")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
}
