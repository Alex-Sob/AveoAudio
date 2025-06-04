using Microsoft.UI.Xaml;

namespace AveoAudio.Views;

public static class VisibilityHelper
{
    public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.RegisterAttached(
        "IsVisible",
        typeof(bool),
        typeof(VisibilityHelper),
        new PropertyMetadata(true, new PropertyChangedCallback(OnIsVisibleChanged)));

    public static bool GetIsVisible(DependencyObject element) => (bool)element.GetValue(IsVisibleProperty);

    public static void SetIsVisible(DependencyObject element, bool value) => element.SetValue(IsVisibleProperty, value);

    private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element) element.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }
}
