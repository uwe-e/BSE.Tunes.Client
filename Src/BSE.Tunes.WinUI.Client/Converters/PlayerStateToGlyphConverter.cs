using Microsoft.UI.Xaml.Data;

namespace BSE.Tunes.WinUI.Client.Converters
{
    public class PlayerStateToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PlayerState playerState)
            {
                return playerState == PlayerState.Playing 
                    ? "\uF8AE"  // Pause icon
                    : "\uF5B0"; // Play icon
            }

            return "\uF5B0"; // Default to play icon
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}