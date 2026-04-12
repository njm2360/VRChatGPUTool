namespace VRCGPUTool.Services;

public interface IDialogService
{
    void ShowError(string message, string title = "エラー");
    void ShowWarning(string message, string title = "警告");
}
