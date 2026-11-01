using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace BSE.Tunes.WinUI.Client.Converters
{
    public class AutoSuggestBoxSuggestionChosenEventArgsConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is AutoSuggestBoxSuggestionChosenEventArgs args)
            {
                // returns the selected object
                return args;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
