using BSE.Tunes.WinUI.Client.Extensions;
using BSE.Tunes.WinUI.Client.Helpers;

using Windows.UI.ViewManagement;

namespace BSE.Tunes.WinUI.Client;

public sealed partial class MainWindow : WindowEx
{
    private Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private UISettings settings;

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

        // Critical fix for .NET 8/9: Unsubscribe and cleanup on window close
        Closed += OnWindowClosed;
    }

    private void OnWindowClosed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
    {
        // Unsubscribe from all events
        settings.ColorValuesChanged -= Settings_ColorValuesChanged;
        Closed -= OnWindowClosed;
        
        // Clear static references to allow proper disposal
        App.MainWindow = null;
        App.AppTitlebar = null;
    }

    // this handles updating the caption button colors correctly when indows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        try
        {
            dispatcherQueue?.TryEnqueue(() =>
            {
                // Verify window still exists before applying theme
                if (App.MainWindow != null)
                {
                    TitleBarHelper.ApplySystemThemeToCaptionButtons();
                }
            });
        }
        catch
        {
            // Ignore exceptions during shutdown
        }
    }
}
