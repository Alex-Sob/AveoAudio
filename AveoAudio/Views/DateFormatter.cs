using System;
using System.Globalization;

namespace AveoAudio.Views
{
    public class DateFormatter : IFormatProvider, ICustomFormatter
    {
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is DateTime date) return format == "relative" ? Format(date) : date.ToString(format);

            if (arg is IFormattable formattable)
                return formattable.ToString(format, CultureInfo.CurrentCulture);
            else
                return arg?.ToString();
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
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
