using System.Windows;
using System.Windows.Threading;
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
    private bool _ownsMutex;
    private IServiceProvider? _services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        base.OnStartup(e);

        _mutex = new Mutex(true, MutexName, out bool created);
        _ownsMutex = created;
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
            HandleFatal("Startup", ex);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        HandleFatal("DispatcherUnhandledException", e.Exception);
    }

    private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        CrashLogger.Log("AppDomainUnhandledException", e.ExceptionObject as Exception);
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        CrashLogger.Log("UnobservedTaskException", e.Exception);
    }

    private void HandleFatal(string source, Exception ex)
    {
        CrashLogger.Log(source, ex);

        MessageBox.Show(
            $"予期しないエラーが発生しました。アプリケーションを終了します。\n\n{ex.Message}\n\n" +
            $"エラーログ\n{CrashLogger.LogPath}",
            "エラー",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        Shutdown(1);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_ownsMutex)
            _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // Services

        services.AddSingleton<INvidiaSmiService, NvidiaSmiProxyService>(); // GPU
        // services.AddSingleton<INvidiaSmiService, MockNvidiaSmiService>(); // Mock

        services.AddSingleton<IConfigService, JsonConfigService>();
        services.AddSingleton<IPowerLogService, SqlitePowerLogService>();
        services.AddSingleton<PowerLogCsvExporter>();
        services.AddSingleton<IElectricityProfileService, JsonElectricityProfileService>();
        services.AddSingleton<IUpdateCheckService, GitHubUpdateCheckService>();
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
