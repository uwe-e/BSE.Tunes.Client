using System.Globalization;

namespace BSE.Tunes.Maui.Client.Converters
{
    public class AnyStringNotNullOrEmptyMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return false;

            return values
                .OfType<string>()
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Any();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}