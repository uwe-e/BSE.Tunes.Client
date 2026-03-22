using BSE.Tunes.Shared.Services;
using BSE.Tunes.Shared.Services.Abstractions;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Services;
using BSE.Tunes.WinUI.Client.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;
    private readonly ISettingsService _settingsService;
    private readonly IDataService _dataService;
    private readonly IAuthenticationService _authenticationService;

    public DefaultActivationHandler(
        INavigationService navigationService,
        ISettingsService settingsService,
        IDataService dataService,
        IAuthenticationService authenticationService)
    {
        _navigationService = navigationService;
        _settingsService = settingsService;
        _dataService = dataService;
        _authenticationService = authenticationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        // Determine which page to start with based on app state
        var startupPage = await DetermineStartupPageAsync();

        switch (startupPage)
        {
            case StartupPage.EndpointConfiguration:
                await NavigateToFullscreenPageAsync(nameof(EndpointConfigurationPage));
                break;

            case StartupPage.Login:
                await NavigateToFullscreenPageAsync(nameof(LoginPage));
                break;

            case StartupPage.Main:
            default:
                await NavigateToMainPageAsync(args.Arguments);
                break;
        }
    }

    private async Task<StartupPage> DetermineStartupPageAsync()
    {
        try
        {
            // Step 1: Check if endpoint is configured
            if (string.IsNullOrWhiteSpace(_settingsService.ServiceEndPoint))
            {
                System.Diagnostics.Debug.WriteLine("DefaultActivationHandler: No endpoint configured");
                return StartupPage.EndpointConfiguration;
            }

            // Step 2: Check if endpoint is accessible
            System.Diagnostics.Debug.WriteLine($"DefaultActivationHandler: Checking endpoint accessibility: {_settingsService.ServiceEndPoint}");
            var isAccessible = await _dataService.IsEndPointAccessibleAsync(_settingsService.ServiceEndPoint);
            
            if (!isAccessible)
            {
                System.Diagnostics.Debug.WriteLine("DefaultActivationHandler: Endpoint not accessible");
                return StartupPage.EndpointConfiguration;
            }

            System.Diagnostics.Debug.WriteLine("DefaultActivationHandler: Endpoint is accessible");

            // Step 3: Check if user exists
            if (_settingsService.User == null)
            {
                System.Diagnostics.Debug.WriteLine("DefaultActivationHandler: No user configured");
                return StartupPage.Login;
            }

            System.Diagnostics.Debug.WriteLine($"DefaultActivationHandler: User found: {_settingsService.User.UserName}");

            // Step 4: Validate authentication token
            try
            {
                System.Diagnostics.Debug.WriteLine("DefaultActivationHandler: Validating authentication token");
                await _authenticationService.GetAuthTokenAsync();
                
                System.Diagnostics.Debug.WriteLine("DefaultActivationHandler: Authentication valid, proceeding to main app");
                return StartupPage.Main; // All checks passed, go to main app
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DefaultActivationHandler: Authentication failed: {ex.Message}");
                return StartupPage.Login; // Token invalid/expired
            }
        }
        catch (Exception ex)
        {
            // Any unexpected error, go to endpoint configuration
            System.Diagnostics.Debug.WriteLine($"DefaultActivationHandler: Unexpected error during startup checks: {ex.Message}");
            return StartupPage.EndpointConfiguration;
        }
    }

    private async Task NavigateToFullscreenPageAsync(string pageKey)
    {
        System.Diagnostics.Debug.WriteLine($"DefaultActivationHandler: Setting up fullscreen navigation to {pageKey}");

        // Create or get the main frame for fullscreen pages
        Frame mainFrame;
        if (_navigationService.GetFrame(NavigationService.FrameKeyMain) is not Frame existingFrame)
        {
            System.Diagnostics.Debug.WriteLine("DefaultActivationHandler: Creating new MainFrame");
            mainFrame = new Frame();
            _navigationService.RegisterFrame(NavigationService.FrameKeyMain, mainFrame);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("DefaultActivationHandler: Using existing MainFrame");
            mainFrame = existingFrame;
        }

        // Set the main frame as window content
        App.MainWindow.Content = mainFrame;

        // Navigate to the requested page
        await _navigationService.NavigateToAsync(
            pageKey,
            frameKey: NavigationService.FrameKeyMain,
            clearNavigation: true);

        System.Diagnostics.Debug.WriteLine($"DefaultActivationHandler: Navigated to {pageKey}");
    }

    private async Task NavigateToMainPageAsync(string? arguments)
    {
        System.Diagnostics.Debug.WriteLine("DefaultActivationHandler: Navigating to MainPage within ShellPage");

        // ShellPage is already set as window content by ActivationService
        // and ShellFrame is already registered by ShellPage constructor
        // Just navigate to MainPage within the shell frame
        await _navigationService.NavigateToAsync(
            nameof(MainPage),
            frameKey: NavigationService.FrameKeyShell,
            parameter: arguments,
            clearNavigation: false);

        System.Diagnostics.Debug.WriteLine("DefaultActivationHandler: Navigated to MainPage");
    }

    private enum StartupPage
    {
        EndpointConfiguration,
        Login,
        Main
    }
}
