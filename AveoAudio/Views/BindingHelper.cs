using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace AveoAudio.Views
{
    public static class BindingHelper
    {
        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.RegisterAttached(
            "Visibility",
            typeof(string),
            typeof(BindingHelper),
            new PropertyMetadata(null, new PropertyChangedCallback(OnVisibilityChanged)));

        public static void SetVisibility(DependencyObject element, string value)
        {
            element.SetValue(VisibilityProperty, value);
        }

        public static string GetVisibility(DependencyObject element)
        {
            return (string)element.GetValue(VisibilityProperty);
        }

        private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var path = (string)e.NewValue;

            var binding = new Binding
            {
                Path = new PropertyPath(d is ContentControl ? "Content." + path : path),
                Mode = BindingMode.OneWay,
                RelativeSource = d is ContentControl ? new RelativeSource { Mode = RelativeSourceMode.Self } : null
            };

            BindingOperations.SetBinding(d, UIElement.VisibilityProperty, binding);
        }
    }
}
