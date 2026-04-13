namespace VRCGPUTool.Services;

public interface IStartupService
{
    bool IsEnabled();
    void Enable();
    void Disable();
}
