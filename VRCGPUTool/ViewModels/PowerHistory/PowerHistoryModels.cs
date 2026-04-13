namespace VRCGPUTool.ViewModels.PowerHistory;

public sealed record HourlyBar(string Label, int Ws, double BarRatio, string WhLabel)
{
    public double EmptyRatio => 1.0 - BarRatio;
}

public sealed record YTick(string Label, double TopRatio);

public sealed record DailyBar(string Label, int TotalWs, double BarRatio, string WhLabel)
{
    public double EmptyRatio => 1.0 - BarRatio;
}
