using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace VRCGPUTool.Converters;

/// <summary>null → Collapsed、非null → Visible</summary>
[ValueConversion(typeof(object), typeof(Visibility))]
public sealed class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>boolを反転する</summary>
[ValueConversion(typeof(bool), typeof(bool))]
public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;
}

/// <summary>IsLimiting(bool) → ステータステキスト</summary>
[ValueConversion(typeof(bool), typeof(string))]
public sealed class LimitStatusTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "■ 制限中" : "○ 待機中";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>bool → Visibility (true=Visible, false=Collapsed)</summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Visibility.Visible;
}

/// <summary>bool → Visibility (true=Collapsed, false=Visible)</summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not Visibility.Visible;
}

/// <summary>0〜1の比率 → GridLength(Star)</summary>
[ValueConversion(typeof(double), typeof(GridLength))]
public sealed class RatioToGridLengthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double d ? new GridLength(d, GridUnitType.Star) : new GridLength(0);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>int (時・分) ↔ string</summary>
public sealed class TimePartConverter : IValueConverter
{
    public int Max { get; set; } = 59;
    public bool ZeroSuppress { get; set; } = false;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int v = value is int i ? Math.Clamp(i, 0, Max) : 0;
        return ZeroSuppress ? v.ToString() : v.ToString("D2");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (int.TryParse(value?.ToString(), out int v))
            return Math.Clamp(v, 0, Max);
        return 0;
    }
}

/// <summary>IsLimiting (bool) → 背景ブラシ</summary>
[ValueConversion(typeof(bool), typeof(Brush))]
public sealed class LimitStatusBrushConverter : IValueConverter
{
    private static readonly Brush ActiveBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xEB, 0xEE)); // 薄い赤
    private static readonly Brush InactiveBrush = new SolidColorBrush(Color.FromRgb(0xE8, 0xF5, 0xE9)); // 薄い緑

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? ActiveBrush : InactiveBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
