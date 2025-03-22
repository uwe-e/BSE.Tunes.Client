using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    internal class SplashPageViewModel : IPageLifecycleAware
    {
        private readonly ISettingsService _settingsService;
        private readonly IDataService _dataService;
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;

        public SplashPageViewModel(INavigationService navigationService,
            ISettingsService settingsService,
            IDataService dataService,
            IAuthenticationService authenticationService)
        {
            _navigationService = navigationService;
            _settingsService = settingsService;
            _dataService = dataService;
            _authenticationService = authenticationService;
        }
        public async void OnAppearing()
        {
            try
            {
                var isAccessible = await _dataService.IsEndPointAccessibleAsync(_settingsService.ServiceEndPoint);
                {
                    if (_settingsService.User is User user)
                    {
                        try
                        {
                            await _authenticationService.RequestRefreshTokenAsync(user.Token);
                            _navigationService.CreateBuilder()
                                .UseAbsoluteNavigation()
                                .AddSegment<MainPage>()
                                .Navigate();
                        }
                        catch (Exception)
                        {
                            _navigationService.CreateBuilder()
                                 .UseAbsoluteNavigation()
                                 .AddSegment<LoginWizzardPage>()
                                 .Navigate();
                        }
                    }
                    else
                    {
                        _navigationService.CreateBuilder()
                                .UseAbsoluteNavigation()
                                .AddSegment<LoginWizzardPage>()
                                .Navigate();
                    }
                }
            }
            catch (Exception)
            {
                _navigationService.CreateBuilder()
                                .UseAbsoluteNavigation()
                                .AddSegment<ServiceEndpointWizzardPageViewModel>()
                                .Navigate();
            }
        }

        public void OnDisappearing()
        {
        }
    }
}
