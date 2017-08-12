using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Trello.Wpf.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public sealed class GroupKeyToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string key = value as string;
            if (!string.IsNullOrEmpty(key))
                return ((MainWindow) Application.Current.MainWindow).MainWindowViewModel.Cards.First(x => x.GroupKey == key).GroupTitle;

            return "All";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
