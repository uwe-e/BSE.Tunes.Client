using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace BSE.Tunes.WinUI.Client.Converters
{
    public class AutoSuggestBoxTextChangedEventArgsConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is AutoSuggestBoxTextChangedEventArgs args)
            {
                // returns the reason for the change (Reason) to the ViewModel
                return args.Reason;
            }

            return null;
        }


        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
