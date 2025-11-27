using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace BSE.Tunes.Maui.Client.Converters
{
    public class AnyStringNotNullOrEmptyMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return false;
            foreach (var v in values)
            {
                if (v is string s && !string.IsNullOrWhiteSpace(s))
                    return true;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}