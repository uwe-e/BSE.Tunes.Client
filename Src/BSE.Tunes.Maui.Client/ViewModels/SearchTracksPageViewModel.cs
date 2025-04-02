using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class SearchTracksPageViewModel : BaseSearchPageViewModel
    {
        private readonly IDataService _dataService;

        public SearchTracksPageViewModel(
            INavigationService navigationService,
            IDataService dataService) : base(navigationService)
        {
            _dataService = dataService;
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
    }
}
