using Microsoft.UI.Xaml.Data;

namespace AveoAudio.Views;

public class BoolToStringConverter : IValueConverter
{
    public string? FalseValue { get; set; }

    public string? TrueValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language) => value switch
    {
        bool boolValue => (boolValue ? TrueValue : FalseValue) ?? "",
        _ => value
    };

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
