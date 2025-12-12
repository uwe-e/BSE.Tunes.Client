using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class LoginSettingsPageViewModel : BaseSettingsPageViewModel
    {
        private string _userName;
        private readonly ISettingsService _settingsService;
        private readonly IResourceService _resourceService;
        private readonly IPageDialogService _pageDialogService;
        private readonly IEventAggregator _eventAggregator;

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                SetProperty(ref _userName, value);
            }
        }

        public LoginSettingsPageViewModel(
            INavigationService navigationService,
            ISettingsService settingsService,
            IResourceService resourceService,
            IPageDialogService pageDialogService,
            IEventAggregator eventAggregator) : base(navigationService)
        {
            _settingsService = settingsService;
            _resourceService = resourceService;
            _pageDialogService = pageDialogService;
            _eventAggregator = eventAggregator;
            
            _eventAggregator.GetEvent<AlbumInfoSelectionEvent>().ShowAlbum(async (uniqueTrack) =>
            {
                if (PageUtilities.IsCurrentPageTypeOf(typeof(LoginSettingsPage), uniqueTrack.UniqueId))
                {
                    var navigationParams = new NavigationParameters
                    {
                        { "album", uniqueTrack.Album }
                    };

                    await NavigationService.NavigateAsync(nameof(AlbumDetailPage), navigationParams);
                }
            });
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
                _resourceService.GetString("LoginSettingsPage_ActionSheet_Title"),
                buttons);
        }

        private async void DeleteAction()
        {
            _settingsService.User = null;

            await NavigationService.NavigateAsync($"/{nameof(NavigationPage)}/{nameof(LoginWizzardPage)}");
        }

        public override void LoadSettings()
        {
            UserName = _settingsService?.User?.UserName;
        }
    }
}
