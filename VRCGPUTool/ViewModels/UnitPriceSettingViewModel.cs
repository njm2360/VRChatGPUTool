using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VRCGPUTool.Models;
using VRCGPUTool.Services;

namespace VRCGPUTool.ViewModels;

public sealed partial class UnitPriceSettingViewModel : ObservableObject
{
    private readonly IElectricityProfileService _service;
    private readonly ElectricityProfile _profile;

    public DayProfileViewModel WeekdayProfile { get; } = new();

    public DayProfileViewModel HolidayProfile { get; } = new();

    [ObservableProperty]
    private bool _holidayUseSameAsWeekday = true;

    public UnitPriceSettingViewModel(IElectricityProfileService service, ElectricityProfile profile)
    {
        _service = service;
        _profile = profile;

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

        await _service.SaveAsync(_profile);
        window?.Close();
    }

    [RelayCommand]
    private static void Cancel(Window? window) => window?.Close();
}
