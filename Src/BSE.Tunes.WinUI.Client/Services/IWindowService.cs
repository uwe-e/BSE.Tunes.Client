using Microsoft.UI.Xaml;

namespace BSE.Tunes.WinUI.Client.Services
{
    public interface IWindowService
    {
        void SaveWindowBounds(Window window);
        void RestoreWindowBounds(Window window);
        void TrackWindow(Window window);
    }
}