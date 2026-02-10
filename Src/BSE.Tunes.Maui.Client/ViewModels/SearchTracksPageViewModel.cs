using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class SearchTracksPageViewModel : BaseSearchPageViewModel, IAlbumInfoSelectionHandler
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private bool _canExecutePlayTrack = true;

        public SearchTracksPageViewModel(
            INavigationService navigationService,
            IDataService dataService,
            IFlyoutNavigationService flyoutNavigationService,
            IMediaManager mediaManager,
            IImageService imageService,
            IEventAggregator eventAggregator)
            : base(navigationService, flyoutNavigationService, dataService, mediaManager, imageService, eventAggregator)
        {
            _dataService = dataService;
            _imageService = imageService;
        }

        public async override void HandleShowAlbum(AlbumSelectionContext context)
        {
            if (PageUtilities.IsCurrentPageTypeOf(typeof(SearchTracksPage)))
            {
                var navigationParams = new NavigationParameters
                    {
                        { "album", context.UniqueAlbum.Album }
                    };
                await NavigationService.NavigateAsync(nameof(AlbumDetailPage), navigationParams);
            }
        }

        protected async override Task GetSearchResults()
        {
            var pagedResult = await _dataService.GetTrackSearchResults(Query, PageNumber, PageSize);
            
            UpdatePaginationState(
                hasItems: pagedResult?.Items is { Count: > 0 },
                hasNextPage: pagedResult?.HasNextPage ?? false
                );

            if (!HasItems && pagedResult?.Items is not { Count: > 0 })
            {
                return;
            }

            var gridPanels = pagedResult.Items
                .Where(track => track != null) // Keep if API might return nulls
                .Select(track => new GridPanel
                {
                    Title = track.Name,
                    SubTitle = track.Album.Artist.Name,
                    ImageSource = _imageService.GetBitmapSource(track.Album.AlbumId, true),
                    Data = track
                });

            foreach (var panel in gridPanels)
            {
                Items.Add(panel);
            }
        }

        protected override bool CanExecutePlayTrack(GridPanel panel)
        {
            return _canExecutePlayTrack;
        }

        protected override async Task PlayTrackAsync(GridPanel panel)
        {
            if (panel?.Data is Track track)
            {
                if (CanExecutePlayTrack(panel))
                {
                    _canExecutePlayTrack = false;

                    try
                    {
                        await PlayTracksAsync(new List<int> { track.Id }, PlayerMode.Song);
                    }
                    finally
                    {
                        _canExecutePlayTrack = true; 
                    }
                }
            }
        }

        protected override Task OpenFlyoutAsync(object obj)
        {
            return base.OpenFlyoutAsync(obj, new PlaylistActionContext { DisplayAlbumInfo = true });
        }
    }
}
