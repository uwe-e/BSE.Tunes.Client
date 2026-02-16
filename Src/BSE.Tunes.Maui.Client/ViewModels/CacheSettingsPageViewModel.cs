using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class CacheSettingsPageViewModel : BaseSettingsPageViewModel
    {
        private string _imageCacheSize;
        private string _audioCacheSize;
        private bool _isCacheChanged;
        private SubscriptionToken _cacheChangeActionToken;
        private readonly IStorageService _storageService;
        private readonly IResourceService _resourceService;
        private readonly IPageDialogService _pageDialogService;
        private readonly IEventAggregator _eventAggregator;

        public string ImageCacheSize
        {
            get
            {
                return _imageCacheSize;
            }
            set
            {
                SetProperty(ref _imageCacheSize, value);
            }
        }

        public string AudioCacheSize
        {
            get => _audioCacheSize;
            set => SetProperty(ref _audioCacheSize, value);
        }

        public CacheSettingsPageViewModel(
            INavigationService navigationService,
            IStorageService storageService,
            IResourceService resourceService,
            IPageDialogService pageDialogService,
            IEventAggregator eventAggregator) : base(navigationService, eventAggregator)
        {
            _storageService = storageService;
            _resourceService = resourceService;
            _pageDialogService = pageDialogService;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<CacheChangedEvent>().Subscribe((args) =>
            {
                LoadSettings();
            });

        }

        public async override void HandleShowAlbum(AlbumSelectionContext context)
        {
            if (PageUtilities.IsCurrentPageTypeOf(typeof(CacheSettingsPage)))
            {
                var navigationParams = new NavigationParameters
                    {
                        { "album", context.UniqueAlbum.Album }
                    };

                await NavigationService.NavigateAsync(nameof(AlbumDetailPage), navigationParams);
            }
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            _cacheChangeActionToken = _eventAggregator.GetEvent<CacheChangedEvent>()
                .Subscribe(
                    _ => LoadSettings(),
                    filter: (args) => args != CacheChangeMode.None);

            base.OnNavigatedTo(parameters);
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            if (!parameters.IsModalNavigation())
            {
                _cacheChangeActionToken?.Dispose();
                _cacheChangeActionToken = null;
            }

            base.OnNavigatedFrom(parameters);
        }

        public async override void LoadSettings()
        {
            if (!_isCacheChanged)
            {
                _isCacheChanged = true;

                var imageSize = await _storageService.GetUsedImageCacheSizeAsync();
                ImageCacheSize = $"{Math.Round(Convert.ToDecimal(imageSize / 1024f / 1024f), 2)} MB";

                var audioCacheSize = await _storageService.GetAudioCacheSizeAsync();
                var sizeInMB = audioCacheSize / 1024f / 1024f;
                var sizeInGB = sizeInMB / 1024f;

                AudioCacheSize = sizeInGB >= 1
                    ? $"{Math.Round(Convert.ToDecimal(sizeInGB), 2)} GB"
                    : $"{Math.Round(Convert.ToDecimal(sizeInMB), 2)} MB";

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
            await _storageService.DeleteCacheAsync();
            _eventAggregator.GetEvent<CacheChangedEvent>().Publish(CacheChangeMode.ImageCacheCleared);
        }

        
    }
}
