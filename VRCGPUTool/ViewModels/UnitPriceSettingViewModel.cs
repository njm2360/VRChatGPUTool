using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VRCGPUTool.Models;
using VRCGPUTool.Services;

namespace VRCGPUTool.ViewModels;

public sealed partial class UnitPriceSettingViewModel : ObservableObject
{
    private readonly IElectricityProfileService _service;
    private readonly ElectricityProfile _profile;
    private readonly ILogger _logger;

    public DayProfileViewModel WeekdayProfile { get; } = new();

    public DayProfileViewModel HolidayProfile { get; } = new();

    [ObservableProperty]
    private bool _holidayUseSameAsWeekday = true;

    public UnitPriceSettingViewModel(IElectricityProfileService service, ElectricityProfile profile,
        ILogger? logger = null)
    {
        _service = service;
        _profile = profile;
        _logger = logger ?? NullLogger.Instance;

        WeekdayProfile.LoadFromSlots(profile.WeekdaySlots);
        HolidayProfile.LoadFromSlots(profile.HolidaySlots);
        _holidayUseSameAsWeekday = !profile.UseDayOfWeek;
    }

    [RelayCommand]
    private void CopyWeekdayToHoliday() => HolidayProfile.CopyFrom(WeekdayProfile);

    [RelayCommand]
    private async Task SaveAndCloseAsync(Window? window)
    {
        _profile.WeekdaySlots = WeekdayProfile.IsDefault ? [] : WeekdayProfile.ToSlots();
        _profile.HolidaySlots = (HolidayUseSameAsWeekday || HolidayProfile.IsDefault) ? [] : HolidayProfile.ToSlots();
        _profile.UseDayOfWeek = !HolidayUseSameAsWeekday;

        try
        {
            await _service.SaveAsync(_profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save electricity profile.");
            MessageBox.Show($"単価設定の保存に失敗しました:\n{ex.Message}", "エラー",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        window?.Close();
    }

    [RelayCommand]
    private static void Cancel(Window? window) => window?.Close();
}
