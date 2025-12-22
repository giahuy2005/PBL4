using System.Globalization;
using System.Windows;
using System.Windows.Data;
namespace PBL4.Converters { 
        public class InverseBoolToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool b && !b)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is Visibility v)
                    return v != Visibility.Visible;
                return true;
            }
        }
}
