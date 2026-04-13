using System.Windows;

namespace VRCGPUTool.Services;

public sealed class WpfApplicationHost : IApplicationHost
{
    public Task InvokeOnUiAsync(Action action)
        => Application.Current.Dispatcher.InvokeAsync(action).Task;

    public void Shutdown()
        => Application.Current.Shutdown();
}
