using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VRCGPUTool.Behaviors;

public static class NumericInputBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled", typeof(bool), typeof(NumericInputBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject d) => (bool)d.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject d, bool v) => d.SetValue(IsEnabledProperty, v);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;

        tb.PreviewTextInput -= OnPreviewTextInput;
        DataObject.RemovePastingHandler(tb, OnPasting);

        if ((bool)e.NewValue)
        {
            tb.PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(tb, OnPasting);
        }
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !IsAllDigits(e.Text);
    }

    private static void OnPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetData(typeof(string)) is string text && IsAllDigits(text))
            return;
        e.CancelCommand();
    }

    private static bool IsAllDigits(string s) => s.Length > 0 && s.All(char.IsDigit);
}
