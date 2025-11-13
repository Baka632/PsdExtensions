using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PsdExtensions.Wpf.Helpers;

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool val)
        {
            if (parameter is string str && bool.TryParse(str, out bool isReverse) && isReverse)
            {
                val = !val;
            }

            return val switch
            {
                true => Visibility.Visible,
                false => Visibility.Collapsed,
            };
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility val)
        {
            bool retVal = val switch
            {
                Visibility.Visible => true,
                _ => false
            };

            if (parameter is string str && bool.TryParse(str, out bool isReverse) && isReverse)
            {
                retVal = !retVal;
            }

            return retVal;
        }

        return DependencyProperty.UnsetValue;
    }
}
