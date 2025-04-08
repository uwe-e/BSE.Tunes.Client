using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class SearchAlbumsPageViewModel : BaseSearchPageViewModel
    {
        private readonly IDataService _dataService;
        private readonly IEventAggregator _eventAggregator;

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
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<AlbumInfoSelectionEvent>().ShowAlbum(async (uniqueTrack) =>
            {
                if (PageUtilities.IsCurrentPageTypeOf(typeof(SearchAlbumsPage)))
                {
                    var navigationParams = new NavigationParameters
                    {
                        { "album", uniqueTrack.Album }
                    };
                    await NavigationService.NavigateAsync(nameof(AlbumDetailPage), navigationParams);
                }
            });
        }

        protected async override Task GetSearchResults()
        {
            var albums = await _dataService.GetAlbumSearchResults(Query, PageNumber, PageSize);
            if (albums.Length == 0)
            {
                HasItems = false;
            }

            foreach (var album in albums)
            {
                if (album != null)
                {
                    Items.Add(new GridPanel
                    {
                        Title = album.Title,
                        SubTitle = album.Artist.Name,
                        ImageSource = _dataService.GetImage(album.AlbumId, true)?.AbsoluteUri,
                        Data = album
                    });
                }
            }
            PageNumber = Items.Count;
        }

        protected override Task OpenFlyoutAsync(object obj)
        {
            return base.OpenFlyoutAsync(obj, new PlaylistActionContext { DisplayAlbumInfo = true });
        }
    }
}
