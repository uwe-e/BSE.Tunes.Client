using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class SearchAlbumsPageViewModel : BaseSearchPageViewModel
    {
        private readonly IDataService _dataService;

        public SearchAlbumsPageViewModel(
            INavigationService navigationService,
            IDataService dataService) : base(navigationService)
        {
            _dataService = dataService;
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
    }
}
