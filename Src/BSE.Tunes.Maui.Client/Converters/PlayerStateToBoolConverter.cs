using System.Globalization;

namespace BSE.Tunes.Maui.Client.Converters
{
    public class PlayerStateToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is PlayerState state && state == PlayerState.Playing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
