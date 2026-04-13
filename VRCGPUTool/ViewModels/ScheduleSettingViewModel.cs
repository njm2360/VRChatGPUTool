using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VRCGPUTool.Models;

namespace VRCGPUTool.ViewModels;

public sealed partial class ScheduleSlotViewModel : ObservableObject
{
    [ObservableProperty] private bool _enabled = true;
    [ObservableProperty] private int _startHour;
    [ObservableProperty] private int _startMinute;
    [ObservableProperty] private int _endHour;
    [ObservableProperty] private int _endMinute;

    public bool IsValid => StartHour * 60 + StartMinute != EndHour * 60 + EndMinute;

    partial void OnStartHourChanged(int value) => OnPropertyChanged(nameof(IsValid));
    partial void OnStartMinuteChanged(int value) => OnPropertyChanged(nameof(IsValid));
    partial void OnEndHourChanged(int value) => OnPropertyChanged(nameof(IsValid));
    partial void OnEndMinuteChanged(int value) => OnPropertyChanged(nameof(IsValid));

    [ObservableProperty] private bool _monday;
    [ObservableProperty] private bool _tuesday;
    [ObservableProperty] private bool _wednesday;
    [ObservableProperty] private bool _thursday;
    [ObservableProperty] private bool _friday;
    [ObservableProperty] private bool _saturday;
    [ObservableProperty] private bool _sunday;

    public ScheduleSlotViewModel() { }

    public ScheduleSlotViewModel(ScheduleSlot slot)
    {
        _enabled = slot.Enabled;
        _startHour = slot.StartHour;
        _startMinute = slot.StartMinute;
        _endHour = slot.EndHour;
        _endMinute = slot.EndMinute;

        _monday = slot.Days.HasFlag(ScheduleDays.Monday);
        _tuesday = slot.Days.HasFlag(ScheduleDays.Tuesday);
        _wednesday = slot.Days.HasFlag(ScheduleDays.Wednesday);
        _thursday = slot.Days.HasFlag(ScheduleDays.Thursday);
        _friday = slot.Days.HasFlag(ScheduleDays.Friday);
        _saturday = slot.Days.HasFlag(ScheduleDays.Saturday);
        _sunday = slot.Days.HasFlag(ScheduleDays.Sunday);
    }

    public ScheduleSlot ToModel() => new()
    {
        Enabled = Enabled,
        StartHour = StartHour,
        StartMinute = StartMinute,
        EndHour = EndHour,
        EndMinute = EndMinute,
        Days = (Monday ? ScheduleDays.Monday : 0)
                    | (Tuesday ? ScheduleDays.Tuesday : 0)
                    | (Wednesday ? ScheduleDays.Wednesday : 0)
                    | (Thursday ? ScheduleDays.Thursday : 0)
                    | (Friday ? ScheduleDays.Friday : 0)
                    | (Saturday ? ScheduleDays.Saturday : 0)
                    | (Sunday ? ScheduleDays.Sunday : 0),
    };
}

public sealed partial class ScheduleSettingViewModel : ObservableObject
{
    public const int MaxSlots = 20;

    public ObservableCollection<ScheduleSlotViewModel> Slots { get; } = [];

    public ScheduleSettingViewModel(IEnumerable<ScheduleSlot> slots)
    {
        Slots.CollectionChanged += OnSlotsCollectionChanged;
        foreach (var slot in slots)
            Slots.Add(new ScheduleSlotViewModel(slot));
    }

    private void OnSlotsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (ScheduleSlotViewModel slot in e.NewItems)
                slot.PropertyChanged += OnSlotPropertyChanged;

        if (e.OldItems is not null)
            foreach (ScheduleSlotViewModel slot in e.OldItems)
                slot.PropertyChanged -= OnSlotPropertyChanged;

        AddSlotCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
    }

    private void OnSlotPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ScheduleSlotViewModel.IsValid))
            SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanAddSlot))]
    private void AddSlot()
    {
        if (!CanAddSlot()) return;

        var now = DateTime.Now;
        Slots.Add(new ScheduleSlotViewModel(new ScheduleSlot
        {
            StartHour = now.Hour,
            StartMinute = now.Minute,
            EndHour = (now.Hour + 1) % 24,
            EndMinute = now.Minute,
        }));
    }

    private bool CanAddSlot() => Slots.Count < MaxSlots;

    [RelayCommand]
    private void RemoveSlot(ScheduleSlotViewModel slot) => Slots.Remove(slot);

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save(Window window)
    {
        window.DialogResult = true;
        window.Close();
    }

    private bool CanSave() => Slots.All(s => s.IsValid);

    [RelayCommand]
    private static void Cancel(Window window)
    {
        window.DialogResult = false;
        window.Close();
    }

    public List<ScheduleSlot> GetSlots() => [.. Slots.Select(s => s.ToModel())];
}
