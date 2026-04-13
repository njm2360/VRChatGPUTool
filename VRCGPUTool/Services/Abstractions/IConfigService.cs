using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public interface IConfigService
{
    Task<AppConfig> LoadAsync();
    Task SaveAsync(AppConfig config);
}
