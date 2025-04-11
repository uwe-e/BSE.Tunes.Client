using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class ServiceEndpointSettingsPageViewModel(
        INavigationService navigationService,
        ISettingsService settingsService,
        IResourceService resourceService,
        IPageDialogService pageDialogService) : BaseSettingsPageViewModel(navigationService)
    {
        private string _serviceEndPoint;
        private readonly ISettingsService _settingsService = settingsService;
        private readonly IResourceService _resourceService = resourceService;
        private readonly IPageDialogService _pageDialogService = pageDialogService;

        public string ServiceEndPoint
        {
            get
            {
                return _serviceEndPoint;
            }
            set
            {
                SetProperty(ref _serviceEndPoint, value);
            }
        }

        public override void LoadSettings()
        {
            ServiceEndPoint = _settingsService?.ServiceEndPoint;
        }

        public async override void DeleteSettings()
        {
            var buttons = new IActionSheetButton[]
            {
                ActionSheetButton.CreateCancelButton(
                    _resourceService.GetString("ActionSheetButton_Cancel")),
                ActionSheetButton.CreateDestroyButton(
                    _resourceService.GetString("ActionSheetButton_Delete"),
                    DeleteAction)
            };

            await _pageDialogService.DisplayActionSheetAsync(
                _resourceService.GetString("ServiceEndpointSettingsPage_ActionSheet_Title"),
                buttons);
        }

        private async void DeleteAction()
        {
            //if (await PlayerManager.CloseAsync())
            {
                _settingsService.ServiceEndPoint = null;

                await NavigationService.NavigateAsync($"/{nameof(NavigationPage)}/{nameof(ServiceEndpointWizzardPage)}");
            }
        }
    }
}
