using BSE.Tunes.Maui.Client.Services;

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
                }
            }
            catch (Exception)
            {
                _navigationService.CreateBuilder()
                                .UseAbsoluteNavigation()
                                .AddSegment<ServiceEndpointWizzardPageViewModel>()
                                .Navigate();
            }
            //_navigationService.CreateBuilder()
            //.UseAbsoluteNavigation()
            //.AddSegment<MainPageViewModel>()
            //.Navigate();
        }

        public void OnDisappearing()
        {

        }
    }
}
