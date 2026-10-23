using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace BSE.Tunes.WinUI.Client.Converters
{
    public class ItemClickEventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as ItemClickEventArgs)?.ClickedItem;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}