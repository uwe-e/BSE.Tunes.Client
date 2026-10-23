using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Extensions;
using BSE.Tunes.WinUI.Client.Helpers;
using BSE.Tunes.WinUI.Client.Services;
using BSE.Tunes.WinUI.Client.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace BSE.Tunes.WinUI.Client.Views;

public sealed partial class ShellPage : Page
{
    private readonly SettingsMonitorService _settingsMonitor;
    private readonly IThemeSelectorService _themeSelectorService;

    public ShellViewModel ViewModel { get; }

    public ShellPage(ShellViewModel viewModel, SettingsMonitorService settingsMonitor, IThemeSelectorService themeSelectorService)
    {
        ViewModel = viewModel;
        _settingsMonitor = settingsMonitor;
        _themeSelectorService = themeSelectorService;
        
        InitializeComponent();


        // Apply theme immediately to this page
        this.RequestedTheme = _themeSelectorService.Theme;

        // Ensure NavigationFrame is available before registering
        if (NavigationFrame == null)
        {
            throw new InvalidOperationException("NavigationFrame was not initialized by InitializeComponent()");
        }

        ViewModel.NavigationService.RegisterFrame(NavigationService.FrameKeyShell, NavigationFrame);
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        // TODO: Set the title bar icon by updating /Assets/WindowIcon.ico.
        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
        
        // Start monitoring for settings changes
        _settingsMonitor.StartMonitoring();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Use the theme from ThemeSelectorService instead of the page's RequestedTheme
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        App.AppTitlebar = AppTitleBarText as UIElement;
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }
}
