using System;
using System.Globalization;
using System.Windows.Data;

namespace Trello.Wpf.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public sealed class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool key = value != null && (bool) value;
            return key ? "Yes" : "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            return str != null && str == "Yes";
        }
    }
}
