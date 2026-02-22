using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class SearchAlbumsPageViewModel : BaseSearchPageViewModel, IAlbumInfoSelectionHandler
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;

        public SearchAlbumsPageViewModel(
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
            if (PageUtilities.IsCurrentPageTypeOf(typeof(SearchAlbumsPage)))
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
            var pagedResult = await _dataService.GetAlbumSearchResults(Query, PageNumber, PageSize);

            UpdatePaginationState(
                hasItems: pagedResult?.Items is { Count: > 0 },
                hasNextPage: pagedResult?.HasNextPage ?? false
                );

            if (!HasItems && pagedResult?.Items is not { Count: > 0 })
            {
                return;
            }

            var gridPanels = pagedResult.Items
                .Where(album => album != null) // Keep if API might return nulls
                .Select(album => new GridPanel
                {
                    Title = album.Title,
                    SubTitle = album.Artist.Name,
                    ImageSource = _imageService.GetBitmapSource(album.AlbumId, true),
                    Data = album
                });

            foreach (var panel in gridPanels)
            {
                Items.Add(panel);
            }
        }

        protected override async void SelectItem(GridPanel obj)
        {
            if (obj?.Data is Album album)
            {
                var navigationParams = new NavigationParameters{
                    { "album", album }
                };

                await NavigationService.NavigateAsync($"{nameof(AlbumDetailPage)}", navigationParams);
            }
        }

        protected override Task OpenFlyoutAsync(object obj)
        {
            return base.OpenFlyoutAsync(obj, new PlaylistActionContext { DisplayAlbumInfo = true });
        }
    }
}
