

using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class AlbumDetailPageViewModel : TracklistBaseViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private Album _album;
        private GridPanel? _selectedAlbum;
        private bool _isQueryBusy;
        private bool _hasFurtherAlbums;
        private int _pageNumber;
        private int _pageSize;
        private bool _hasItems;
        private ObservableCollection<GridPanel>? _albums;
        private ICommand _selectAlbumCommand;
        private ICommand _loadMoreAlbumssCommand;

        public ICommand LoadMoreAlbumsCommand => _loadMoreAlbumssCommand ?? (
           _loadMoreAlbumssCommand = new DelegateCommand(() =>
           {
               //Device.BeginInvokeOnMainThread(async () => await LoadMoreAlbums());
               MainThread.BeginInvokeOnMainThread(async () => await LoadMoreAlbums());

           }));

        public ICommand SelectAlbumCommand => _selectAlbumCommand
            ?? (_selectAlbumCommand = new Command<GridPanel>(SelectAlbum));

        public ObservableCollection<GridPanel> Albums => _albums ??= [];

        public GridPanel? SelectedAlbum
        {
            get => _selectedAlbum;
            set => SetProperty<GridPanel>(ref _selectedAlbum, value);
        }

        public Album Album
        {
            get => _album;
            set => SetProperty<Album>(ref _album, value);
        }

        public bool IsQueryBusy
        {
            get
            {
                return _isQueryBusy;
            }
            set
            {
                SetProperty<bool>(ref _isQueryBusy, value);
            }
        }

        public bool HasFurtherAlbums
        {
            get
            {
                return _hasFurtherAlbums;
            }
            set
            {
                SetProperty<bool>(ref _hasFurtherAlbums, value);
            }
        }

        public AlbumDetailPageViewModel(
            INavigationService navigationService,
            IFlyoutNavigationService flyoutNavigationService,
            IEventAggregator eventAggregator,
            IResourceService resourceService,
            IDataService dataService,
            IImageService imageService) : base(navigationService)
        {
            _eventAggregator = eventAggregator;
            _dataService = dataService;
            _imageService = imageService;

            _pageSize = 10;
            _pageNumber = 0;
            _hasItems = true;
            HasFurtherAlbums = false;
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            Album album = parameters.GetValue<Album>("album");
            await LoadAlbum(album);
            await LoadMoreAlbums();
        }

        private async Task LoadAlbum(Album album)
        {
            if (album != null)
            {
                Album = await _dataService.GetAlbumById(album.Id);
                foreach (Track track in Album.Tracks)
                {
                    track.Album = new Album
                    {
                        AlbumId = Album.AlbumId,
                        Id = Album.Id,
                        Title = Album.Title,
                        Artist = Album.Artist
                    };
                    Items.Add(new GridPanel
                    {
                        Number = track.TrackNumber,
                        Title = track.Name,
                        Data = track

                    });
                }
                ImageSource = _imageService.GetBitmapSource(Album.AlbumId);
                //PlayAllCommand.RaiseCanExecuteChanged();
                //PlayAllRandomizedCommand.RaiseCanExecuteChanged();
                IsBusy = false;
            }
        }

        private async Task LoadMoreAlbums()
        {
            if (IsQueryBusy)
            {
                return;
            }

            if (_hasItems)
            {
                IsQueryBusy = true;
                try
                {
                    var albums = await _dataService.GetAlbumsByArtist(Album.Artist.Id, _pageNumber, _pageSize);
                    if (albums == null || albums.Count == 0)
                    {
                        _hasItems = false;
                        return;
                    }
                    foreach (var album in albums)
                    {
                        if (album != null)
                        {
                            Albums.Add(new GridPanel
                            {
                                Title = album.Title,
                                SubTitle = album.Artist.Name,
                                ImageSource = _imageService.GetBitmapSource(album.AlbumId),
                                Data = album
                            });
                        }
                    }
                    if (Albums.Count > 1)
                    {
                        HasFurtherAlbums = true;
                    }
                    _pageNumber = Albums.Count;
                }
                finally
                {
                    IsQueryBusy = false;
                }
            }
        }
        
        private async void SelectAlbum(GridPanel panel)
        {
            if (panel?.Data is Album album)
            {
                /*
                 * The property SelectedAlbum is the parameter for the SelectionChangedCommand command.
                 * To reselect the previously selected item within the collection we need to reset the SelectedAlbum
                 */
                SelectedAlbum = null;
                var navigationParams = new NavigationParameters
                    {
                        { "album", album }
                    };
                await NavigationService.NavigateAsync($"{nameof(AlbumDetailPage)}", navigationParams);
            }
        }
    }
}
