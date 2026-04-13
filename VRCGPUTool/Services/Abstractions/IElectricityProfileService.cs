using VRCGPUTool.Models;

namespace VRCGPUTool.Services;

public interface IElectricityProfileService
{
    Task<ElectricityProfile> LoadAsync();
    Task SaveAsync(ElectricityProfile profile);
}
