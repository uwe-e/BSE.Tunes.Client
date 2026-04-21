using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace BSE.Tunes.WinUI.Client.Converters;

/// <summary>
/// Converts a boolean value to a Visibility value.
/// True = Visible, False = Collapsed.
/// Use ConverterParameter=True to invert (True = Collapsed, False = Visible).
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool boolValue = value is bool b && b;
        bool invert = parameter?.ToString()?.Equals("True", StringComparison.OrdinalIgnoreCase) == true;
        
        return (invert ? !boolValue : boolValue) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        bool invert = parameter?.ToString()?.Equals("True", StringComparison.OrdinalIgnoreCase) == true;
        bool isVisible = value is Visibility v && v == Visibility.Visible;
        
        return invert ? !isVisible : isVisible;
    }
}