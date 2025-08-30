
using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class SearchPageViewModel : TracklistBaseViewModel
    {
        private ICommand _textChangedCommand;
        private ICommand _showAllAlbumSearchResultsCommand;
        private ICommand _showAllTrackSearchResultsCommand;
        private ICommand _selectItemCommand;
        private bool _hasAlbums;
        private bool _hasTracks;
        private ObservableCollection<GridPanel> _albums;
        private ObservableCollection<GridPanel> _tracks;
        private bool _hasMoreAlbums;
        private bool _hasMoreTracks;
        private readonly IDataService _dataService;
        private readonly IEventAggregator _eventAggregator;
        private CancellationTokenSource _cancellationTokenSource;
        private string _textValue;
        private bool _canExecutePlayTrack = true;

        public ICommand TextChangedCommand => _textChangedCommand
            ??= new DelegateCommand<string>(async (textValue) => await TextChangedAsync(textValue));

        public ICommand SelectItemCommand => _selectItemCommand
            ??= new DelegateCommand<GridPanel>(async (item) => await SelectItemAsync(item));

        public ICommand ShowAllAlbumSearchResultsCommand => _showAllAlbumSearchResultsCommand
           ??= new DelegateCommand(async() => await ShowAllAlbumSearchResults());

        public ICommand ShowAllTrackSearchResultsCommand => _showAllTrackSearchResultsCommand
            ??= new DelegateCommand(async() => await ShowAllTrackSearchResults());

        public ObservableCollection<GridPanel> Albums => _albums ??= [];

        public ObservableCollection<GridPanel> Tracks => _tracks ??= [];

        public bool HasAlbums
        {
            get => _hasAlbums;
            set => SetProperty<bool>(ref _hasAlbums, value);
        }

        public bool HasMoreAlbums
        {
            get => _hasMoreAlbums;
            set => SetProperty<bool>(ref _hasMoreAlbums, value);
        }

        public bool HasTracks
        {
            get => _hasTracks;
            set => SetProperty<bool>(ref _hasTracks, value);
        }

        public bool HasMoreTracks
        {
            get => _hasMoreTracks;
            set => SetProperty<bool>(ref _hasMoreTracks, value);
        }

        public string TextValue
        {
            get => _textValue;
            set => SetProperty<string>(ref _textValue, value);
        }

        public SearchPageViewModel(
            INavigationService navigationService,
            IDataService dataService,
            IFlyoutNavigationService flyoutNavigationService,
            IMediaManager mediaManager,
            IImageService imageService,
            IEventAggregator eventAggregator) : base(navigationService,
                flyoutNavigationService, dataService, mediaManager, imageService, eventAggregator)
        {
            _dataService = dataService;
            _eventAggregator = eventAggregator;
            IsBusy = false;

             _eventAggregator.GetEvent<AlbumInfoSelectionEvent>().ShowAlbum(async (uniqueTrack) =>
            {
                if (PageUtilities.IsCurrentPageTypeOf(typeof(SearchPage)))
                {
                    var navigationParams = new NavigationParameters
                    {
                        { "album", uniqueTrack.Album }
                    };
                    await NavigationService.NavigateAsync(nameof(AlbumDetailPage), navigationParams);
                }
            });
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

                    await PlayTracksAsync(new List<int>
                    {
                        track.Id
                    }, PlayerMode.Song);

                    _canExecutePlayTrack = false;
                }
            }
        }

        protected override Task OpenFlyoutAsync(object obj)
        {
            return base.OpenFlyoutAsync(obj, new PlaylistActionContext { DisplayAlbumInfo = true });
        }

        private async Task TextChangedAsync(string textValue)
        {
            IsBusy = true;
            if (string.IsNullOrEmpty(textValue) || textValue.Length < 3)
            {
                HasAlbums = HasTracks = false;
                Albums.Clear();
            }
            else
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = _cancellationTokenSource.Token;
                try
                {
                    await GetAlbumResultsAsync(textValue, token);
                    await GetTrackResultsAsync(textValue, token);
                }
                catch (Exception) { }
            }
            IsBusy = false;
        }

        private async Task GetAlbumResultsAsync(string searchPhrase, CancellationToken token)
        {
            var albums = await _dataService.GetAlbumSearchResults(searchPhrase, 0, 4, token);
            if (albums.Length == 0)
            {
                HasAlbums = false;
            }
            else
            {
                HasAlbums = true;
                HasMoreAlbums = albums.Length > 3;
                var index = 0;
                var newResults = albums.Take(4).Reverse();

                foreach (var item in newResults)
                {
                    Albums.Insert(index, new GridPanel
                    {
                        Title = item.Title,
                        SubTitle = item.Artist.Name,
                        ImageSource = _dataService.GetImage(item.AlbumId, true)?.AbsoluteUri,
                        Data = item
                    });
                    index++;
                }
                if (Albums.Count > newResults.Count())
                {
                    var c = Albums.Count;
                    for (int i = c - 1; i >= newResults.Count(); i--)
                    {
                        Albums.RemoveAt(i);
                    }
                }
            }
        }

        private async Task GetTrackResultsAsync(string searchPhrase, CancellationToken token)
        {
            var tracks = await _dataService.GetTrackSearchResults(searchPhrase, 0, 4, token);
            if (tracks.Length == 0)
            {
                HasTracks = false;
            }
            else
            {
                HasTracks = true;
                HasMoreTracks = tracks.Length > 3;
                var index = 0;
                var newResults = tracks.Take(4).Reverse();

                foreach (var item in newResults)
                {
                    Tracks.Insert(index, new GridPanel
                    {
                        Title = item.Name,
                        SubTitle = item.Album.Artist.Name,
                        ImageSource = _dataService.GetImage(item.Album.AlbumId, true)?.AbsoluteUri,
                        Data = item
                    });
                    index++;
                }
                if (Tracks.Count > newResults.Count())
                {
                    var c = Tracks.Count;
                    for (int i = c - 1; i >= newResults.Count(); i--)
                    {
                        Tracks.RemoveAt(i);
                    }
                }
            }
        }

        private async Task SelectItemAsync(GridPanel item)
        {
            if (item?.Data is Album album)
            {
                var navigationParams = new NavigationParameters{
                    { "album", album }
                };
                
                await NavigationService.NavigateAsync($"{nameof(AlbumDetailPage)}", navigationParams);
            }
        }
        
        private async Task ShowAllAlbumSearchResults()
        {
            var navigationParams = new NavigationParameters
                    {
                        { "query",  TextValue}
                    };
            await NavigationService.NavigateAsync($"{nameof(SearchAlbumsPage)}", navigationParams);
        }

        private async Task ShowAllTrackSearchResults()
        {
            var navigationParams = new NavigationParameters
                    {
                        { "query",  TextValue}
                    };
            await NavigationService.NavigateAsync($"{nameof(SearchTracksPage)}", navigationParams);
        }
    }
}
