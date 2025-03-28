using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace AveoAudio.Views;

public static class FlyoutBehavior
{
    public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
        "Enabled",
        typeof(bool),
        typeof(FlyoutBehavior),
        new PropertyMetadata(false, new PropertyChangedCallback(OnEnabledChanged)));

    public static bool GetEnabled(DependencyObject element) => (bool)element.GetValue(EnabledProperty);

    public static void SetEnabled(DependencyObject element, bool value) => element.SetValue(EnabledProperty, value);

    private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element) element.GotFocus += OnGotFocus;
    }

    private static void OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement element && FlyoutBase.GetAttachedFlyout(element) != null)
        {
            FlyoutBase.ShowAttachedFlyout(element);
        }
    }
}
