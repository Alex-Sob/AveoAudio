using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace AveoAudio.Views
{
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not DateTime date) return value;

            var format = parameter as string;
            return format == "relative" ? Format(date) : date.ToString(format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private static string Format(DateTime date)
        {
            if (date.Year == DateTime.Today.Year)
            {
                if (date.Month == DateTime.Today.Month)
                    return "This month";
                else
                    return DateTimeFormatInfo.CurrentInfo.MonthNames[date.Month - 1];
            }
            else
                return $"{DateTimeFormatInfo.CurrentInfo.MonthNames[date.Month - 1]} {date.Year}";
        }
    }
}
