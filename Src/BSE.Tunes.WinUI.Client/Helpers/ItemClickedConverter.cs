using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace BSE.Tunes.WinUI.Client.Helpers;

public class ItemClickedConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ItemClickEventArgs args)
        {
            return args.ClickedItem;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}