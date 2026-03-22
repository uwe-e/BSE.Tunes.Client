using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Views;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Services;

/// <summary>
/// Monitors critical settings changes and navigates appropriately.
/// Handles cascading changes: endpoint removal automatically clears user account.
/// </summary>
public class SettingsMonitorService
{
    private readonly ISettingsServiceExtended _settingsService;
    private readonly INavigationService _navigationService;
    private bool _isMonitoring;

    public SettingsMonitorService(
        ISettingsServiceExtended settingsService,
        INavigationService navigationService)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;
    }

    public void StartMonitoring()
    {
        if (_isMonitoring)
        {
            return;
        }

        _settingsService.ServiceEndpointRemoved += OnServiceEndpointRemoved;
        _settingsService.UserAccountDeleted += OnUserAccountDeleted;
        _isMonitoring = true;
        
        System.Diagnostics.Debug.WriteLine("SettingsMonitorService: Started monitoring settings changes");
    }

    public void StopMonitoring()
    {
        if (!_isMonitoring)
        {
            return;
        }

        _settingsService.ServiceEndpointRemoved -= OnServiceEndpointRemoved;
        _settingsService.UserAccountDeleted -= OnUserAccountDeleted;
        _isMonitoring = false;
        
        System.Diagnostics.Debug.WriteLine("SettingsMonitorService: Stopped monitoring settings changes");
    }

    private async void OnServiceEndpointRemoved(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("SettingsMonitorService: Service endpoint removed, navigating to EndpointConfigurationPage");
        System.Diagnostics.Debug.WriteLine("SettingsMonitorService: User account was automatically cleared (cascade)");
        
        // Create/get main frame if needed
        if (_navigationService.GetFrame(NavigationService.FrameKeyMain) is not Frame mainFrame)
        {
            mainFrame = new Frame();
            _navigationService.RegisterFrame(NavigationService.FrameKeyMain, mainFrame);
        }
        
        // Switch window content to main frame
        App.MainWindow.Content = mainFrame;
        
        // Navigate to endpoint configuration
        await _navigationService.NavigateToAsync(
            nameof(EndpointConfigurationPage),
            frameKey: NavigationService.FrameKeyMain,
            clearNavigation: true);
    }

    private async void OnUserAccountDeleted(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("SettingsMonitorService: User account deleted, navigating to LoginPage");
        
        // Create/get main frame if needed
        if (_navigationService.GetFrame(NavigationService.FrameKeyMain) is not Frame mainFrame)
        {
            mainFrame = new Frame();
            _navigationService.RegisterFrame(NavigationService.FrameKeyMain, mainFrame);
        }
        
        // Switch window content to main frame
        App.MainWindow.Content = mainFrame;
        
        // Navigate to login page
        await _navigationService.NavigateToAsync(
            nameof(LoginPage),
            frameKey: NavigationService.FrameKeyMain,
            clearNavigation: true);
    }
}