using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class ServiceEndpointWizzardPageViewModel(
        INavigationService navigationService,
        IResourceService resourceService,
        IPageDialogService pageDialogService,
        ISettingsService settingsService,
        IDataService dataService,
        IAuthenticationService authenticationService) : ViewModelBase(navigationService)
    {
        private readonly IResourceService _resourceService = resourceService;
        private readonly IPageDialogService _pageDialogService = pageDialogService;
        private readonly ISettingsService _settingsService = settingsService;
        private readonly IDataService _dataService = dataService;
        private readonly IAuthenticationService _authenticationService = authenticationService;
        private DelegateCommand _saveCommand;
        private string _serviceEndPoint;

        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(Save, CanSave);

        public string ServiceEndPoint
        {
            get => _serviceEndPoint;
            set
            {
                SetProperty(ref _serviceEndPoint, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private bool CanSave()
        {
            return !String.IsNullOrEmpty(ServiceEndPoint);
        }

        private async void Save()
        {
            string serviceEndPoint = null;
            var input = ServiceEndPoint?.Trim();

            if (String.IsNullOrEmpty(input))
            {
                return; // Safeguard; command should prevent this.
            }

            /* 
             * if there's a valid url, use it.
             * Valid urls are urls with a http or https scheme.
             * a URL with the scheme "http" is valid for debugging reasons.
            */
            if (Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                serviceEndPoint = uriResult.AbsoluteUri;
            }
            else
            {
                var hasScheme = input.StartsWith(Uri.UriSchemeHttp + "://", StringComparison.OrdinalIgnoreCase)
                                || input.StartsWith(Uri.UriSchemeHttps + "://", StringComparison.OrdinalIgnoreCase);

                var candidate = hasScheme ? input : (Uri.UriSchemeHttps + "://" + input);

                if (Uri.TryCreate(candidate, UriKind.Absolute, out uriResult) &&
                    (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    serviceEndPoint = uriResult.AbsoluteUri;
                }
            }

            try
            {
                
                if (String.IsNullOrEmpty(serviceEndPoint))
                {
                    throw new InvalidOperationException("The service endpoint is not a valid URL.");
                }
            
                await _dataService.IsEndPointAccessibleAsync(serviceEndPoint);
                _settingsService.ServiceEndPoint = serviceEndPoint;
                if (_settingsService.User is User user)
                {
                    try
                    {
                        await _authenticationService.RequestRefreshTokenAsync(user.Token);

                        var result = await NavigationService.RestartAndNavigateAsync("/" + nameof(SplashPage));
                        if (!result.Success)
                        {
                            // log result.Exception or message to debug output
                            System.Diagnostics.Debug.WriteLine(result.Exception);
                        }
                    }
                    catch (Exception)
                    {
                        await NavigationService.CreateBuilder()
                                .UseAbsoluteNavigation()
                                .AddSegment<LoginWizzardPage>()
                                .NavigateAsync();
                    }
                }
                else
                {
                    await NavigationService.CreateBuilder()
                                .UseAbsoluteNavigation()
                                .AddSegment<LoginWizzardPage>()
                                .NavigateAsync();
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(nameof(Save) + ":" + exception.Message);

                var title = _resourceService.GetString("AlertDialog_Error_Title_Text");
                string message = _resourceService.GetString("ServiceEndpointPageViewModel_NotAvailableExceptionMessage");
                var dialogResult = _resourceService.GetString("Dialog_Result_Cancel");

                await _pageDialogService.DisplayAlertAsync(
                    title,
                    message,
                    dialogResult);
            }
        }
    }
}
