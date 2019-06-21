using System;

using Windows.UI.Xaml.Data;

namespace AveoAudio.Views
{
    public class StringFormatConverter : IValueConverter
    {
        public IFormatProvider FormatProvider { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var format = parameter as string;
            return !string.IsNullOrEmpty(format) ? string.Format(this.FormatProvider, format, value) : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
