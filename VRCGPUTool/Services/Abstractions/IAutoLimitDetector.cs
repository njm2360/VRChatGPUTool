namespace VRCGPUTool.Services;

public interface IAutoLimitDetector
{
    void Reset();
    bool Update(int utilization, int threshold);
}
