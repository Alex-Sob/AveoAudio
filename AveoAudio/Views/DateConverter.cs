using Microsoft.UI.Xaml.Data;

namespace AveoAudio.Views;

public class DateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value switch
    {
        DateTime date when date.Year == DateTime.Today.Year => date.ToString("m"),
        DateTime date => $"{date:m}, {date.Year}",
        _ => value
    };

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
