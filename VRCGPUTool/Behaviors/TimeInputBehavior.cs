using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VRCGPUTool.Behaviors;

public static class TimeInputBehavior
{
    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.RegisterAttached(
            "MaxValue", typeof(int), typeof(TimeInputBehavior),
            new PropertyMetadata(-1, OnMaxValueChanged));

    public static int GetMaxValue(DependencyObject d) => (int)d.GetValue(MaxValueProperty);
    public static void SetMaxValue(DependencyObject d, int v) => d.SetValue(MaxValueProperty, v);

    private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;
        tb.PreviewTextInput -= OnPreviewTextInput;
        if ((int)e.NewValue >= 0)
            tb.PreviewTextInput += OnPreviewTextInput;
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = true;
        if (sender is not TextBox tb || e.Text.Length == 0 || !char.IsDigit(e.Text[0])) return;

        int maxValue = GetMaxValue(tb);

        // 選択範囲を除いたテキストのみを使う（全選択→入力で置換できるように）
        string unselected = tb.Text[..tb.SelectionStart] + tb.Text[(tb.SelectionStart + tb.SelectionLength)..];
        string digits = new(unselected.Where(char.IsDigit).ToArray());
        string combined = digits + e.Text[0];

        if (combined.Length > 2) combined = combined[^2..];

        int val = Math.Min(int.Parse(combined), maxValue);

        tb.Text = val.ToString();
        tb.CaretIndex = tb.Text.Length;
    }
}
