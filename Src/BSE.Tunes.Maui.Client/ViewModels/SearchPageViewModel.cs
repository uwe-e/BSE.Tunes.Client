using BSE.Tunes.Maui.Client.Models;
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
        private readonly IImageService _imageService;
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
            set => SetProperty(ref _hasAlbums, value);
        }

        public bool HasMoreAlbums
        {
            get => _hasMoreAlbums;
            set => SetProperty(ref _hasMoreAlbums, value);
        }

        public bool HasTracks
        {
            get => _hasTracks;
            set => SetProperty(ref _hasTracks, value);
        }

        public bool HasMoreTracks
        {
            get => _hasMoreTracks;
            set => SetProperty(ref _hasMoreTracks, value);
        }

        public string TextValue
        {
            get => _textValue;
            set => SetProperty(ref _textValue, value);
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
            _imageService = imageService;
            IsBusy = false;
        }
        
        public override async void HandleShowAlbum(AlbumSelectionContext context)
        {
            if (PageUtilities.IsCurrentPageTypeOf(typeof(SearchPage)))
            {
                var navigationParams = new NavigationParameters
                    {
                        { "album", context.UniqueAlbum.Album }
                    };
                await NavigationService.NavigateAsync(nameof(AlbumDetailPage), navigationParams);
            }
        }

        protected override bool CanExecutePlayTrack(GridPanel panel)
        {
            return _canExecutePlayTrack;
        }

        protected override async Task PlayTrackAsync(GridPanel panel)
        {
            if (panel?.Data is Track track && _canExecutePlayTrack)
            {
                _canExecutePlayTrack = false;

                await PlayTracksAsync([track.Id], PlayerMode.Song);

                _canExecutePlayTrack = true;
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
                Tracks.Clear();
            }
            else
            {
                try
                {
                    await Task.WhenAll(
                        GetAlbumResultsAsync(textValue),
                        GetTrackResultsAsync(textValue));
                }
                catch (Exception) { }
            }
            IsBusy = false;
        }

        private async Task GetAlbumResultsAsync(string searchPhrase)
        {
            await GetSearchResultsAsync(
                () => _dataService.GetAlbumSearchResults(searchPhrase, 1, 4),
                Albums,
                item => new GridPanel
                {
                    Title = item.Title,
                    SubTitle = item.Artist.Name,
                    ImageSource = _imageService.GetBitmapSource(item.AlbumId, true),
                    Data = item
                },
                hasMore => HasMoreAlbums = hasMore,
                hasResults => HasAlbums = hasResults);
        }

        private async Task GetTrackResultsAsync(string searchPhrase)
        {
            await GetSearchResultsAsync(
                () => _dataService.GetTrackSearchResults(searchPhrase, 1, 4),
                Tracks,
                item => new GridPanel
                {
                    Title = item.Name,
                    SubTitle = item.Album.Artist.Name,
                    ImageSource = _imageService.GetBitmapSource(item.Album.AlbumId, true),
                    Data = item
                },
                hasMore => HasMoreTracks = hasMore,
                hasResults => HasTracks = hasResults);
        }

        private async Task GetSearchResultsAsync<T>(
            Func<Task<PagedResult<T>>> getResults,
            ObservableCollection<GridPanel> collection,
            Func<T, GridPanel> createPanel,
            Action<bool> setHasMore,
            Action<bool> setHasResults)
        {
            var pagedResult = await getResults();
            if (pagedResult?.Items.Count == 0)
            {
                setHasResults(false);
                collection.Clear();
            }
            else
            {
                setHasResults(true);
                setHasMore(pagedResult.HasNextPage);
                
                int newCount = Math.Min(pagedResult.Items.Count, 4);
                
                // Remove excess items first
                while (collection.Count > newCount)
                {
                    collection.RemoveAt(collection.Count - 1);
                }
                
                // Update or insert items in reverse order
                for (int i = 0; i < newCount; i++)
                {
                    var item = pagedResult.Items[newCount - 1 - i];
                    var panel = createPanel(item);
                    
                    if (i < collection.Count)
                    {
                        collection[i] = panel;
                    }
                    else
                    {
                        collection.Add(panel);
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
                
                await NavigationService.NavigateAsync(nameof(AlbumDetailPage), navigationParams);
            }
        }
        
        private async Task ShowAllAlbumSearchResults()
        {
            var navigationParams = new NavigationParameters
                    {
                        { "query",  TextValue}
                    };
            await NavigationService.NavigateAsync(nameof(SearchAlbumsPage), navigationParams);
        }

        private async Task ShowAllTrackSearchResults()
        {
            var navigationParams = new NavigationParameters
                    {
                        { "query",  TextValue}
                    };
            await NavigationService.NavigateAsync(nameof(SearchTracksPage), navigationParams);
        }
    }
}
