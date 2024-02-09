using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class ServiceEndpointSettingsPageViewModel : BaseSettingsPageViewModel
    {
        private string? _serviceEndPoint;
        private readonly ISettingsService _settingsService;
        private readonly IResourceService _resourceService;
        private readonly IPageDialogService _pageDialogService;

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

        public ServiceEndpointSettingsPageViewModel(
            INavigationService navigationService,
            ISettingsService settingsService,
            IResourceService resourceService,
            IPageDialogService pageDialogService) : base(navigationService)
        {
            _settingsService = settingsService;
            _resourceService = resourceService;
            _pageDialogService = pageDialogService;
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
