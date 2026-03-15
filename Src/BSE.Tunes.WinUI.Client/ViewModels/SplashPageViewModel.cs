using BSE.Tunes.Shared.Services;
using BSE.Tunes.Shared.Services.Abstractions;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Contracts.ViewModels;
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
            }
            catch (Exception)
            {
                // Service endpoint check failed, navigate to configuration page
                _navigationService.NavigateTo(nameof(EndpointConfigurationPage), clearNavigation: true);
            }
        }

        public void OnNavigatedFrom()
        {
            
        }

        public void OnNavigatedTo(object parameter)
        {
            
        }
    }
}
