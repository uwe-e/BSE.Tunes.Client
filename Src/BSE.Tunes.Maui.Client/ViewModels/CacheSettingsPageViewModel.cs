using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Services;
using Prism.Events;
using Prism.Navigation;
using Prism.Services;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class CacheSettingsPageViewModel : BaseSettingsPageViewModel
    {
        private string _usedDiskSpace;
        private bool _isCacheChanged;
        private readonly IStorageService _storageService;
        private readonly IResourceService _resourceService;
        private readonly IPageDialogService _pageDialogService;
        private readonly IEventAggregator _eventAggregator;

        public string UsedDiskSpace
        {
            get
            {
                return _usedDiskSpace;
            }
            set
            {
                SetProperty(ref _usedDiskSpace, value);
            }
        }

        public CacheSettingsPageViewModel(
            INavigationService navigationService,
            IStorageService storageService,
            IResourceService resourceService,
            IPageDialogService pageDialogService,
            IEventAggregator eventAggregator) : base(navigationService)
        {
            _storageService = storageService;
            _resourceService = resourceService;
            _pageDialogService = pageDialogService;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<CacheChangedEvent>().Subscribe((args) =>
            {
                LoadSettings();
            });

            _eventAggregator.GetEvent<AlbumInfoSelectionEvent>().ShowAlbum(async (uniqueTrack) =>
            {
                //if (PageUtilities.IsCurrentPageTypeOf(typeof(CacheSettingsPage)))
                //{
                //    var navigationParams = new NavigationParameters
                //    {
                //        { "album", uniqueTrack.Album }
                //    };

                //    await NavigationService.NavigateAsync(nameof(AlbumDetailPage), navigationParams);
                //}
            });
        }

        public async override void LoadSettings()
        {
            if (!_isCacheChanged)
            {
                _isCacheChanged = true;

                var usedSpace = await _storageService.GetUsedDiskSpaceAsync();
                UsedDiskSpace = $"{Math.Round(Convert.ToDecimal(usedSpace / 1024f / 1024f), 2)} MB";

                _isCacheChanged = false;
            }
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
                _resourceService.GetString("CacheSettingsPage_ActionSheet_Title"),
                buttons);
        }

        private async void DeleteAction()
        {
            await _storageService.DeleteCachedImagesAsync();

            _eventAggregator.GetEvent<CacheChangedEvent>().Publish(CacheChangeMode.Removed);
        }
    }
}
