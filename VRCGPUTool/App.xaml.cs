using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using VRCGPUTool.Services;
using VRCGPUTool.Services.Mock;
using VRCGPUTool.ViewModels;
using VRCGPUTool.ViewModels.PowerHistory;
using VRCGPUTool.Views;

namespace VRCGPUTool;

public partial class App : Application
{
    private const string MutexName = "VRCGPUTool_SingleInstance_Mutex";

    private Mutex? _mutex;
    private IServiceProvider? _services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, ex) =>
        {
            MessageBox.Show(ex.Exception.ToString(), "未処理例外 (Dispatcher)",
                MessageBoxButton.OK, MessageBoxImage.Error);
            ex.Handled = true;
            Shutdown(1);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, ex) =>
        {
            MessageBox.Show(ex.ExceptionObject?.ToString(), "未処理例外 (AppDomain)",
                MessageBoxButton.OK, MessageBoxImage.Error);
        };

        _mutex = new Mutex(true, MutexName, out bool created);
        if (!created)
        {
            MessageBox.Show("既に起動しています。", "VRChat GPU Tool",
                MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        _services = BuildServiceProvider();

        var mainWindow = _services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        var vm = (MainViewModel)mainWindow.DataContext;
        try
        {
            await vm.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "初期化エラー",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // Services

        // services.AddSingleton<INvidiaSmiService, NvidiaSmiProxyService>(); // GPU
        services.AddSingleton<INvidiaSmiService, MockNvidiaSmiService>(); // Mock

        services.AddSingleton<IConfigService, JsonConfigService>();
        services.AddSingleton<IPowerLogService, PowerLogService>();
        services.AddSingleton<IElectricityProfileService, JsonElectricityProfileService>();
        services.AddSingleton<IUpdateCheckService, MockUpdateCheckService>();
        services.AddSingleton<IAutoLimitDetector, AutoLimitDetector>();
        services.AddSingleton<IStartupService, StartupService>();
        services.AddSingleton<IApplicationHost, WpfApplicationHost>();
        services.AddSingleton<INavigationService, WpfNavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton(TimeProvider.System);

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<PowerHistoryViewModel>();

        // Views
        services.AddTransient<MainWindow>();
        services.AddTransient<SettingsWindow>();
        services.AddTransient<PowerHistoryWindow>();

        return services.BuildServiceProvider();
    }
}
