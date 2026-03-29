using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Services;
using BSE.Tunes.WinUI.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public partial class EndpointConfigurationViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly IDataService _dataService;
    private readonly IAuthenticationService _authenticationService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _serviceEndPoint = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isValidating;

    public EndpointConfigurationViewModel(
        ISettingsService settingsService,
        IDataService dataService,
        IAuthenticationService authenticationService,
        INavigationService navigationService)
    {
        _settingsService = settingsService;
        _dataService = dataService;
        _authenticationService = authenticationService;
        _navigationService = navigationService;
        
        ServiceEndPoint = _settingsService.ServiceEndPoint ?? string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsValidating = true;
        ErrorMessage = string.Empty;

        try
        {
            var serviceEndPoint = ValidateAndNormalizeUrl(ServiceEndPoint);
            
            if (string.IsNullOrEmpty(serviceEndPoint))
            {
                ErrorMessage = "Please enter a valid service URL.";
                return;
            }

            var isAccessible = await _dataService.IsEndPointAccessibleAsync(serviceEndPoint);
            
            if (!isAccessible)
            {
                ErrorMessage = "Unable to connect to the service. Please check the URL and try again.";
                return;
            }

            // Save the endpoint
            _settingsService.ServiceEndPoint = serviceEndPoint;

            // Check if user needs to login or can go directly to main
            if (_settingsService.User == null)
            {
                // No user - go to login page
                await _navigationService.NavigateToAsync(
                    nameof(LoginPage),
                    frameKey: NavigationService.FrameKeyMain,
                    clearNavigation: true);
            }
            else
            {
                // User exists - try to authenticate
                try
                {
                    await _authenticationService.GetAuthTokenAsync();
                    
                    // Success - go to main app (loads ShellPage)
                    var shell = App.GetService<ShellPage>();
                    App.MainWindow.Content = shell;
                    
                    await _navigationService.NavigateToAsync(
                        nameof(MainPage),
                        frameKey: NavigationService.FrameKeyShell,
                        clearNavigation: true);
                }
                catch
                {
                    // Token invalid - go to login
                    await _navigationService.NavigateToAsync(
                        nameof(LoginPage),
                        frameKey: NavigationService.FrameKeyMain,
                        clearNavigation: true);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsValidating = false;
        }
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(ServiceEndPoint) && !IsValidating;

    private string? ValidateAndNormalizeUrl(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var trimmed = input.Trim();

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out Uri? uriResult) &&
            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            return uriResult.AbsoluteUri;
        }

        var hasScheme = trimmed.StartsWith(Uri.UriSchemeHttp + "://", StringComparison.OrdinalIgnoreCase)
                        || trimmed.StartsWith(Uri.UriSchemeHttps + "://", StringComparison.OrdinalIgnoreCase);

        var candidate = hasScheme ? trimmed : (Uri.UriSchemeHttps + "://" + trimmed);

        if (Uri.TryCreate(candidate, UriKind.Absolute, out uriResult) &&
            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            return uriResult.AbsoluteUri;
        }

        return null;
    }

    partial void OnServiceEndPointChanged(string value)
    {
        SaveCommand.NotifyCanExecuteChanged();
        ErrorMessage = string.Empty;
    }
}