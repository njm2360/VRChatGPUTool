using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using VRCGPUTool.Models;
using VRCGPUTool.Services;

namespace VRCGPUTool.ViewModels.PowerHistory;

public sealed partial class MonthlyHistoryViewModel : ObservableObject
{
    private readonly IPowerLogService _powerLogService;
    private readonly PowerLogCsvExporter _exporter;
    private readonly Func<HourlyPowerLog> _getLiveLog;
    private readonly ElectricityProfile _profile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MonthText))]
    [NotifyCanExecuteChangedFor(nameof(NextMonthCommand))]
    private DateOnly _selectedMonth;

    [ObservableProperty]
    private ObservableCollection<DailyBar> _dayBars = [];

    [ObservableProperty]
    private ObservableCollection<YTick> _dayYTicks = [];

    [ObservableProperty]
    private string _monthTotalText = "";

    public string MonthText => SelectedMonth.ToString("yyyy年M月");

    public MonthlyHistoryViewModel(
        IPowerLogService powerLogService,
        PowerLogCsvExporter exporter,
        Func<HourlyPowerLog> getLiveLog,
        ElectricityProfile profile)
    {
        _powerLogService = powerLogService;
        _exporter = exporter;
        _getLiveLog = getLiveLog;
        _profile = profile;
        var today = DateOnly.FromDateTime(DateTime.Today);
        _selectedMonth = new DateOnly(today.Year, today.Month, 1);
        _ = LoadDayBarsAsync();
    }

    [RelayCommand]
    private async Task PreviousMonthAsync()
    {
        SelectedMonth = SelectedMonth.AddMonths(-1);
        await LoadDayBarsAsync();
    }

    [RelayCommand(CanExecute = nameof(CanNextMonth))]
    private async Task NextMonthAsync()
    {
        SelectedMonth = SelectedMonth.AddMonths(1);
        await LoadDayBarsAsync();
    }

    private bool CanNextMonth()
    {
        var thisMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        return SelectedMonth < thisMonth;
    }

    [RelayCommand]
    private async Task ExportMonthCsvAsync()
    {
        var dialog = new SaveFileDialog
        {
            Title = "CSVエクスポート",
            Filter = "CSVファイル (*.csv)|*.csv",
            FileName = $"powerlog_{SelectedMonth:yyyy-MM}.csv",
            DefaultExt = ".csv",
        };
        if (dialog.ShowDialog() != true) return;
        await _exporter.ExportMonthToCsvAsync(SelectedMonth, dialog.FileName);
    }

    internal Task ReloadAsync() => LoadDayBarsAsync();

    private async Task LoadDayBarsAsync()
    {
        int year = SelectedMonth.Year;
        int month = SelectedMonth.Month;
        int daysInMonth = DateTime.DaysInMonth(year, month);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var liveLog = _getLiveLog();

        var tasks = Enumerable.Range(1, daysInMonth).Select(async d =>
        {
            var date = new DateOnly(year, month, d);
            if (date > today) return (TotalWs: 0, TotalPrice: 0.0);
            HourlyPowerLog log = date == liveLog.Date
                ? liveLog
                : await _powerLogService.LoadForDateAsync(date).ConfigureAwait(false);
            bool isWeekday = date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
            double[] unitPrices = _profile.ComputeHourlyPrices(isWeekday);
            double price = 0;
            for (int h = 0; h < 24; h++)
                price += unitPrices[h] * log.HourlyWatts[h];
            return (TotalWs: log.HourlyWatts.Sum(), TotalPrice: price);
        });

        var results = await Task.WhenAll(tasks).ConfigureAwait(true);
        int[] totals = [.. results.Select(r => r.TotalWs)];
        int max = totals.Length > 0 ? totals.Max() : 0;

        DayYTicks = new ObservableCollection<YTick>(
            Enumerable.Range(0, 4).Select(i =>
            {
                double ratio = i / 4.0;
                int w = max > 0 ? (int)Math.Round(max * (1.0 - ratio)) : 0;
                return new YTick(FormatWhs(w), ratio);
            })
        );

        DayBars = new ObservableCollection<DailyBar>(
            Enumerable.Range(0, daysInMonth).Select(d =>
            {
                int ws = totals[d];
                double ratio = max > 0 ? ws / (double)max : 0;
                return new DailyBar($"{d + 1}", ws, ratio, FormatWhs(ws));
            })
        );

        double totalKwh = totals.Sum(x => (double)x) / (3600.0 * 1000.0);
        double totalPrice = results.Sum(r => r.TotalPrice) / (3600.0 * 1000.0);
        bool priceAvailable = _profile.WeekdaySlots.Count > 0
            && (!_profile.UseDayOfWeek || _profile.HolidaySlots.Count > 0);
        MonthTotalText = priceAvailable
            ? $"合計: {totalKwh:F3} kWh  /  電気代: {totalPrice:F1} 円"
            : $"合計: {totalKwh:F3} kWh  (単価未設定)";
    }

    private static string FormatWhs(int ws)
    {
        double kwh = ws / (3600.0 * 1000.0);
        return $"{kwh:F3} kWh";
    }
}
