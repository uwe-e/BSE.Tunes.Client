
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class SearchPageViewModel : ViewModelBase
    {
        private ICommand _textChangedCommand;
        private bool _hasAlbums;
        private bool _hasTracks;
        private ObservableCollection<GridPanel> _albums;
        private ObservableCollection<GridPanel> _tracks;
        private bool _hasMoreAlbums;
        private bool _hasMoreTracks;
        private readonly IDataService _dataService;
        private CancellationTokenSource _cancellationTokenSource;

        public ICommand TextChangedCommand => _textChangedCommand ??= new DelegateCommand<string>(async (textValue) => await TextChangedAsync(textValue));

        public ObservableCollection<GridPanel> Albums => _albums ??= new ObservableCollection<GridPanel>();

            public ObservableCollection<GridPanel> Tracks => _tracks ??= new ObservableCollection<GridPanel>();

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

        public SearchPageViewModel(
            INavigationService navigationService,
            IDataService dataService) : base(navigationService)
        {
            _dataService = dataService;
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
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation was canceled.");
                }
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



    }
}
