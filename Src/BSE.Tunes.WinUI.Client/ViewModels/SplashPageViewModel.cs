using BSE.Tunes.Shared.Services;
using BSE.Tunes.Shared.Services.Abstractions;
using BSE.Tunes.Shared.Services.Models;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Contracts.ViewModels;
using BSE.Tunes.WinUI.Client.Services;
using BSE.Tunes.WinUI.Client.Views;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class SplashPageViewModel(
        INavigationService navigationService,
        ISettingsService settingsService,
        IDataService dataService,
        IAuthenticationService authenticationService) : ViewModelBase, INavigationAware, IActivationAware
    {
        private readonly INavigationService _navigationService = navigationService;
        private readonly ISettingsService _settingsService = settingsService;
        private readonly IDataService _dataService = dataService;
        private readonly IAuthenticationService _authenticationService = authenticationService;

        public async Task OnActivatedAsync(object? parameter = null)
        {
            await PerformStartUpChecksasync();
        }

        private async Task PerformStartUpChecksasync()
        {
            try
            {
                // Check if service endpoint is accessible
                var isAccessible = await _dataService.IsEndPointAccessibleAsync(_settingsService.ServiceEndPoint);
                if (isAccessible)
                {
                    if (_settingsService.User is User user)
                    {
                        try
                        {
                            // Try to get/refresh authentication token
                            await _authenticationService.GetAuthTokenAsync();

                            // Authentication successful, navigate to main shell
                            _navigationService.NavigateTo(nameof(MainPage), null, clearNavigation: true, navigateFullscreen: false);
                            //_navigationService.NavigateTo(nameof(EndpointConfigurationPage),null, clearNavigation: true, navigateFullscreen: true);
                        }
                        catch (Exception ex)
                        {
                            // Authentication failed, navigate to login page
                            _navigationService.NavigateTo(nameof(LoginPage), NavigationService.FrameKeyMain, clearNavigation: true);
                        }
                    }
                    else
                    {
                        _navigationService.NavigateTo(nameof(LoginPage), NavigationService.FrameKeyMain, clearNavigation: true);
                    }
                }
                else
                {
                    // Service endpoint is not accessible, navigate to configuration page
                    _navigationService.NavigateTo(nameof(EndpointConfigurationPage), NavigationService.FrameKeyMain, clearNavigation: true);
                }
            }
            catch (Exception)
            {
                // Service endpoint check failed, navigate to configuration page
                _navigationService.NavigateTo(nameof(EndpointConfigurationPage), NavigationService.FrameKeyMain, clearNavigation: true);
            }
        }

        public void OnNavigatedFrom()
        {
            
        }

        public async void OnNavigatedTo(object parameter)
        {
            await PerformStartUpChecksasync();
        }
    }
}
