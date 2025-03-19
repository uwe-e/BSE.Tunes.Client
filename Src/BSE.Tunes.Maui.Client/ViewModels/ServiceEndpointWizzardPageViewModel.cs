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

            /* 
             * if theres a valid url, use it.
             * Valid urls are urls with a http or https scheme.
             * an url with the scheme "http" is valid for debugging reasons.
            */
            if (Uri.TryCreate(ServiceEndPoint, UriKind.Absolute, out Uri uriResult))
            {
                serviceEndPoint = uriResult.AbsoluteUri;
            }
            else
            {
                //invalid urls have no scheme
                if (!ServiceEndPoint?.StartsWith(Uri.UriSchemeHttps) ?? false)
                {
                    //build a valid url with a https scheme. That should be the default scheme.
                    var uriBuilder = new UriBuilder(Uri.UriSchemeHttps, ServiceEndPoint);
                    serviceEndPoint = uriBuilder.Uri.AbsoluteUri;
                }
            }

            try
            {
                await _dataService.IsEndPointAccessibleAsync(serviceEndPoint);
                _settingsService.ServiceEndPoint = serviceEndPoint;
                if (_settingsService.User is User user)
                {
                    try
                    {
                        await _authenticationService.RequestRefreshTokenAsync(user.Token);
                        NavigationService.CreateBuilder()
                                .UseAbsoluteNavigation()
                                .AddSegment<MainPage>()
                                .Navigate();
                    }
                    catch (Exception)
                    {
                        NavigationService.CreateBuilder()
                                .UseAbsoluteNavigation()
                                .AddSegment<LoginWizzardPage>()
                                .Navigate();
                    }
                }
                else
                {
                    NavigationService.CreateBuilder()
                                .UseAbsoluteNavigation()
                                .AddSegment<LoginWizzardPage>()
                                .Navigate();
                }
            }
            catch (Exception exception)
            {
                var title = _resourceService.GetString("AlertDialog_Error_Title_Text");
                var dialogResult = _resourceService.GetString("Dialog_Result_Cancel");

                await _pageDialogService.DisplayAlertAsync(
                    title,
                    exception.Message,
                    dialogResult);
            }
        }
    }
}
