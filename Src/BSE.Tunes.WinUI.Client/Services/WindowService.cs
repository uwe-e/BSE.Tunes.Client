using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using Windows.Storage;
using WinRT.Interop;

namespace BSE.Tunes.WinUI.Client.Services
{
    public class WindowService : IWindowService
    {
        private const string WindowWidthKey = "WindowWidth";
        private const string WindowHeightKey = "WindowHeight";
        private const string WindowXKey = "WindowX";
        private const string WindowYKey = "WindowY";

        private AppWindow _appWindow;

        public void TrackWindow(Window window)
        {
            _appWindow = GetAppWindow(window);
            
            if (_appWindow != null)
            {
                _appWindow.Changed += AppWindow_Changed;
            }

            window.Closed += (s, e) => SaveWindowBounds(window);
        }

        public void SaveWindowBounds(Window window)
        {
            var appWindow = GetAppWindow(window);
            if (appWindow != null)
            {
                var position = appWindow.Position;
                var size = appWindow.Size;

                var localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values[WindowWidthKey] = size.Width;
                localSettings.Values[WindowHeightKey] = size.Height;
                localSettings.Values[WindowXKey] = position.X;
                localSettings.Values[WindowYKey] = position.Y;
            }
        }

        public void RestoreWindowBounds(Window window)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            
            if (localSettings.Values.ContainsKey(WindowWidthKey) &&
                localSettings.Values.ContainsKey(WindowHeightKey) &&
                localSettings.Values.ContainsKey(WindowXKey) &&
                localSettings.Values.ContainsKey(WindowYKey))
            {
                var appWindow = GetAppWindow(window);
                if (appWindow != null)
                {
                    var width = (int)localSettings.Values[WindowWidthKey];
                    var height = (int)localSettings.Values[WindowHeightKey];
                    var x = (int)localSettings.Values[WindowXKey];
                    var y = (int)localSettings.Values[WindowYKey];

                    appWindow.MoveAndResize(new RectInt32(x, y, width, height));
                }
            }
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (args.DidPositionChange || args.DidSizeChange)
            {
                // Optionally save on every change (can be resource-intensive)
                // SaveWindowBounds(Window); // Would need to keep window reference
            }
        }

        private static AppWindow GetAppWindow(Window window)
        {
            var windowHandle = WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            return AppWindow.GetFromWindowId(windowId);
        }
    }
}