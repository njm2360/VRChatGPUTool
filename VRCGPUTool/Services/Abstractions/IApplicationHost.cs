namespace VRCGPUTool.Services;

public interface IApplicationHost
{
    Task InvokeOnUiAsync(Action action);
    void Shutdown();
}
