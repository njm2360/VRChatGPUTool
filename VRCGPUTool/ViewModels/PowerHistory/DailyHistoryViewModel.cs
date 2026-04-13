using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using VRCGPUTool.Models;
using VRCGPUTool.Services;

namespace VRCGPUTool.ViewModels.PowerHistory;

public sealed partial class DailyHistoryViewModel : ObservableObject
{
    private readonly IPowerLogService _powerLogService;
    private readonly Func<HourlyPowerLog> _getLiveLog;

    private readonly ElectricityProfile _profile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DateText))]
    [NotifyCanExecuteChangedFor(nameof(NextDayCommand))]
    private DateOnly _selectedDate;

    [ObservableProperty]
    private ObservableCollection<HourlyBar> _bars = [];

    [ObservableProperty]
    private ObservableCollection<YTick> _yTicks = [];

    [ObservableProperty]
    private int _maxWatts;

    [ObservableProperty]
    private string _totalText = "";

    public string DateText => SelectedDate.ToDateTime(TimeOnly.MinValue).ToString("yyyy年M月d日(ddd)", System.Globalization.CultureInfo.GetCultureInfo("ja-JP"));

    public DailyHistoryViewModel(
        IPowerLogService powerLogService,
        Func<HourlyPowerLog> getLiveLog,
        ElectricityProfile profile)
    {
        _powerLogService = powerLogService;
        _getLiveLog = getLiveLog;
        _selectedDate = getLiveLog().Date;
        _profile = profile;

        LoadBars(getLiveLog());
    }

    [RelayCommand]
    private async Task PreviousDayAsync()
    {
        SelectedDate = SelectedDate.AddDays(-1);
        await ReloadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanNextDay))]
    private async Task NextDayAsync()
    {
        SelectedDate = SelectedDate.AddDays(1);
        await ReloadAsync();
    }

    private bool CanNextDay() => SelectedDate < DateOnly.FromDateTime(DateTime.Today);

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        var dialog = new SaveFileDialog
        {
            Title = "CSVエクスポート",
            Filter = "CSVファイル (*.csv)|*.csv",
            FileName = $"powerlog_{SelectedDate:yyyy-MM-dd}.csv",
            DefaultExt = ".csv",
        };

        if (dialog.ShowDialog() != true) return;

        var log = await _powerLogService.LoadForDateAsync(SelectedDate);
        await _powerLogService.ExportToCsvAsync(log, dialog.FileName);
    }

    internal async Task ReloadAsync()
    {
        var liveLog = _getLiveLog();
        HourlyPowerLog log;
        if (SelectedDate == liveLog.Date)
            log = liveLog;
        else
            log = await _powerLogService.LoadForDateAsync(SelectedDate);
        LoadBars(log);
    }

    private void LoadBars(HourlyPowerLog log)
    {
        int max = log.HourlyWatts.Max();
        MaxWatts = max;

        bool isWeekday = SelectedDate.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
        double[] unitPrices = _profile.ComputeHourlyPrices(isWeekday);
        double totalWs = 0;
        double totalPrice = 0;
        for (int h = 0; h < 24; h++)
        {
            totalWs += log.HourlyWatts[h];
            totalPrice += unitPrices[h] * log.HourlyWatts[h];
        }
        double totalKwh = totalWs / (3600.0 * 1000.0);
        totalPrice /= 3600.0 * 1000.0;

        var relevantSlots = (_profile.UseDayOfWeek && !isWeekday) ? _profile.HolidaySlots : _profile.WeekdaySlots;
        TotalText = relevantSlots.Count > 0
            ? $"合計: {totalKwh:F3} kWh  /  電気代: {totalPrice:F1} 円"
            : $"合計: {totalKwh:F3} kWh  (単価未設定)";

        YTicks = new ObservableCollection<YTick>(
            Enumerable.Range(0, 4).Select(i =>
            {
                double ratio = i / 4.0;
                int w = max > 0 ? (int)Math.Round(max * (1.0 - ratio)) : 0;
                return new YTick(FormatWhs(w), ratio);
            })
        );

        Bars = new ObservableCollection<HourlyBar>(
            Enumerable.Range(0, 24).Select(h =>
            {
                int ws = log.HourlyWatts[h];
                double ratio = max > 0 ? ws / (double)max : 0;
                return new HourlyBar($"{h}", ws, ratio, FormatWhs(ws));
            })
        );
    }

    /// <summary>W·s 値を Wh/kWh に変換してフォーマット</summary>
    internal static string FormatWhs(int ws)
    {
        double wh = ws / 3600.0;
        return wh >= 1000 ? $"{wh / 1000.0:F2} kWh" : $"{wh:F1} Wh";
    }
}
