using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VRCGPUTool.Models;

namespace VRCGPUTool.ViewModels;

public sealed partial class PriceSlotItem : ObservableObject
{
    public const double MinUnitPrice = 0.0;
    public const double MaxUnitPrice = 100.0;
    public const int MinHour = 0;
    public const int MaxHour = 23;

    public bool IsFixed { get; init; }

    [ObservableProperty] private int _hour;
    [ObservableProperty] private double _unitPrice;

    internal int PreviousHour { get; private set; }

    partial void OnHourChanging(int value) => PreviousHour = Hour;

    partial void OnHourChanged(int value)
    {
        if (IsFixed) return;
        if (value < MinHour) Hour = MinHour;
        else if (value > MaxHour) Hour = MaxHour;
    }

    partial void OnUnitPriceChanged(double value)
    {
        if (value < MinUnitPrice) UnitPrice = MinUnitPrice;
        else if (value > MaxUnitPrice) UnitPrice = MaxUnitPrice;
    }
}

public sealed record HourBlock(string Hour, string ToolTip, Brush Fill);

public sealed record SlotLegend(string Label, Brush Fill);

public sealed partial class DayProfileViewModel : ObservableObject
{
    public const int MaxSlots = 8;

    internal static readonly Brush[] Palette =
    [
        new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),
        new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3)),
        new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)),
        new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)),
        new SolidColorBrush(Color.FromRgb(0x00, 0x96, 0x88)),
        new SolidColorBrush(Color.FromRgb(0x9C, 0x27, 0xB0)),
        new SolidColorBrush(Color.FromRgb(0x79, 0x55, 0x48)),
        new SolidColorBrush(Color.FromRgb(0x60, 0x7D, 0x8B)),
    ];

    private ObservableCollection<PriceSlotItem>? _subscribedSlots;

    [ObservableProperty] private ObservableCollection<PriceSlotItem> _slots = [];
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSlotCommand))]
    private PriceSlotItem? _selectedSlot;
    [ObservableProperty] private int _inputHour;
    [ObservableProperty] private double _inputPrice;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private ObservableCollection<HourBlock> _hourBlocks = [];
    [ObservableProperty] private ObservableCollection<SlotLegend> _legend = [];

    public DayProfileViewModel()
    {
        Slots = [new PriceSlotItem { Hour = 0, UnitPrice = 0, IsFixed = true }];
    }

    public void LoadFromSlots(List<PriceSlot> slots)
    {
        var items = slots
            .Select(s => new PriceSlotItem { Hour = s.Hour, UnitPrice = s.UnitPrice, IsFixed = s.Hour == 0 })
            .OrderBy(s => s.Hour)
            .ToList();

        if (!items.Any(s => s.Hour == 0))
            items.Insert(0, new PriceSlotItem { Hour = 0, UnitPrice = 0, IsFixed = true });

        Slots = new ObservableCollection<PriceSlotItem>(items);
    }

    public void CopyFrom(DayProfileViewModel source)
    {
        Slots = new ObservableCollection<PriceSlotItem>(
            source.Slots.Select(s => new PriceSlotItem { Hour = s.Hour, UnitPrice = s.UnitPrice, IsFixed = s.Hour == 0 })
        );
    }

    public bool IsDefault => Slots.Count == 1 && Slots[0].Hour == 0 && Slots[0].UnitPrice == 0;

    public List<PriceSlot> ToSlots()
        => [.. Slots.OrderBy(s => s.Hour).Select(s => new PriceSlot { Hour = s.Hour, UnitPrice = s.UnitPrice })];

    partial void OnSlotsChanged(ObservableCollection<PriceSlotItem> value)
    {
        if (_subscribedSlots is not null)
            foreach (var s in _subscribedSlots) s.PropertyChanged -= OnSlotItemChanged;

        _subscribedSlots = value;
        foreach (var s in value) s.PropertyChanged += OnSlotItemChanged;

        RefreshChart(value);
    }

    private void OnSlotItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PriceSlotItem.Hour)
            && sender is PriceSlotItem changed
            && Slots.Any(s => s != changed && s.Hour == changed.Hour))
        {
            changed.Hour = changed.PreviousHour;
            return;
        }

        RefreshChart(Slots);
    }

    private void RefreshChart(IList<PriceSlotItem> slots)
    {
        var sorted = slots.OrderBy(s => s.Hour).ToArray();

        Legend = new ObservableCollection<SlotLegend>(
            sorted.Select((s, idx) =>
                new SlotLegend($"{s.Hour}時～  {s.UnitPrice:F2} 円/kWh",
                    Palette[idx % Palette.Length]))
        );

        HourBlocks = new ObservableCollection<HourBlock>(
            Enumerable.Range(0, 24).Select(h =>
            {
                int si = -1;
                for (int i = sorted.Length - 1; i >= 0; i--)
                    if (sorted[i].Hour <= h) { si = i; break; }

                return new HourBlock($"{h}",
                    $"{h}:00  {sorted[si].UnitPrice:F2} 円/kWh",
                    Palette[si % Palette.Length]);
            })
        );
    }

    [RelayCommand]
    private void AddSlot()
    {
        ErrorMessage = null;

        if (Slots.Count >= MaxSlots)
        {
            ErrorMessage = $"スロットは最大{MaxSlots}個までです";
            return;
        }
        if (InputHour < PriceSlotItem.MinHour || InputHour > PriceSlotItem.MaxHour)
        {
            ErrorMessage = $"開始時は{PriceSlotItem.MinHour}〜{PriceSlotItem.MaxHour}時の範囲で入力してください";
            return;
        }
        if (Slots.Any(s => s.Hour == InputHour))
        {
            ErrorMessage = "同じ時刻のスロットは追加できません";
            return;
        }
        if (InputPrice < PriceSlotItem.MinUnitPrice || InputPrice > PriceSlotItem.MaxUnitPrice)
        {
            ErrorMessage = $"単価は{PriceSlotItem.MinUnitPrice}〜{PriceSlotItem.MaxUnitPrice}円/kWhの範囲で入力してください";
            return;
        }

        var newSlot = new PriceSlotItem { Hour = InputHour, UnitPrice = InputPrice, IsFixed = InputHour == 0 };
        Slots = new ObservableCollection<PriceSlotItem>(
            Slots.Append(newSlot).OrderBy(s => s.Hour));
    }

    [RelayCommand(CanExecute = nameof(CanRemoveSlot))]
    private void RemoveSlot()
    {
        ErrorMessage = null;
        Slots = new ObservableCollection<PriceSlotItem>(Slots.Where(s => s != SelectedSlot));
        SelectedSlot = null;
    }

    private bool CanRemoveSlot() => SelectedSlot is { IsFixed: false };
}
