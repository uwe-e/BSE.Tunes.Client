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
                            //await _authenticationService.RequestRefreshTokenAsync(user.Token);
                            await _authenticationService.GetAuthTokenAsync();
                            await _navigationService.CreateBuilder()
                                .UseAbsoluteNavigation()
                                .AddSegment<MainPage>()
                                .NavigateAsync();
                        }
                        catch (Exception)
                        {
                            await _navigationService.CreateBuilder()
                                 .UseAbsoluteNavigation()
                                 .AddSegment<LoginWizzardPage>()
                                 .NavigateAsync();
                        }
                    }
                    else
                    {
                        await _navigationService.CreateBuilder()
                                .UseAbsoluteNavigation()
                                .AddSegment<LoginWizzardPage>()
                                .NavigateAsync();
                    }
                }
            }
            catch (Exception)
            {
                await _navigationService.CreateBuilder()
                                .UseAbsoluteNavigation()
                                .AddSegment<ServiceEndpointWizzardPageViewModel>()
                                .NavigateAsync();
            }
        }

        public void OnDisappearing()
        {
        }
    }
}
