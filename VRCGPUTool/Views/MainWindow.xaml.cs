using System.Reflection;
using System.Windows;

using VRCGPUTool.ViewModels;

namespace VRCGPUTool.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        Title = $"VRChat GPU Tool v{version!.Major}.{version.Minor}.{version.Build}";

        TrayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!)!;
        TrayIcon.Visibility = Visibility.Collapsed;
    }

    // ─────────────────────────────────────────
    // Tray Icon
    // ─────────────────────────────────────────

    private void TrayIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        => RestoreWindow();

    private void TrayMenu_Open_Click(object sender, RoutedEventArgs e)
        => RestoreWindow();

    private void TrayMenu_Exit_Click(object sender, RoutedEventArgs e)
    {
        RestoreWindow();
        Close();
    }

    private void RestoreWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        TrayIcon.Visibility = Visibility.Collapsed;
        Activate();
    }

    // ─────────────────────────────────────────
    // Minimize to Tray
    // ─────────────────────────────────────────

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            TrayIcon.Visibility = Visibility.Visible;
        }
    }

    // ─────────────────────────────────────────
    // Close Handling
    // ─────────────────────────────────────────

    private bool _allowClose;

    private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_allowClose) return;

        var vm = (MainViewModel)DataContext;

        if (vm.IsLimiting)
        {
            var result = MessageBox.Show(
                "現在電力制限中です。終了すると制限が解除されませんが、よろしいですか？",
                "確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }
        }

        e.Cancel = true;

        await vm.DisposeAsync();

        TrayIcon.Dispose();
        _allowClose = true;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Application.Current.Shutdown();
    }
}
