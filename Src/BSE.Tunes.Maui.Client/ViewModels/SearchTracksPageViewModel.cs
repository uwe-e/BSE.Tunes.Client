using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class SearchTracksPageViewModel : BaseSearchPageViewModel
    {
        private readonly IDataService _dataService;
        private readonly IEventAggregator _eventAggregator;

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
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<AlbumInfoSelectionEvent>().ShowAlbum(async (uniqueTrack) =>
            {
                if (PageUtilities.IsCurrentPageTypeOf(typeof(SearchTracksPage)))
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
            var tracks = await _dataService.GetTrackSearchResults(Query, PageNumber, PageSize);
            if (tracks.Length == 0)
            {
                HasItems = false;
            }
            foreach (var track in tracks)
            {
                if (track != null)
                {
                    Items.Add(new GridPanel
                    {
                        Title = track.Name,
                        SubTitle = track.Album.Artist.Name,
                        ImageSource = _dataService.GetImage(track.Album.AlbumId, true)?.AbsoluteUri,
                        Data = track
                    });
                }
            }
            PageNumber = Items.Count;
        }

        protected override void PlayTrack(GridPanel panel)
        {
            if (panel?.Data is Track track)
            {
                PlayTracks(new List<int>
                {
                    track.Id
                }, PlayerMode.Song);
            }
        }

        protected override Task OpenFlyoutAsync(object obj)
        {
            return base.OpenFlyoutAsync(obj, new PlaylistActionContext { DisplayAlbumInfo = true });
        }
    }
}
