using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using BSEtunes.Contracts.Enums;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class AlbumDetailPageViewModel : TracklistBaseViewModel
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private Album _album;
        private GridPanel _selectedAlbum;
        private bool _isQueryBusy;
        private bool _hasFurtherAlbums;
        private int _pageNumber;
        private readonly int _pageSize;
        private bool _hasItems;
        private ObservableCollection<GridPanel> _albums;
        private ICommand _selectAlbumCommand;
        private ICommand _loadMoreAlbumssCommand;
        private bool _canExecutePlayTrack = true;

        public ICommand LoadMoreAlbumsCommand => _loadMoreAlbumssCommand ??= new DelegateCommand(async () => await LoadMoreAlbumsAsync());

        public ICommand SelectAlbumCommand => _selectAlbumCommand ??= new Command<GridPanel>(SelectAlbum);

        public ObservableCollection<GridPanel> Albums => _albums ??= [];

        public GridPanel SelectedAlbum
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
            get => _isQueryBusy;
            set => SetProperty<bool>(ref _isQueryBusy, value);
        }

        public bool HasFurtherAlbums
        {
            get => _hasFurtherAlbums;
            set => SetProperty<bool>(ref _hasFurtherAlbums, value);
        }

        public AlbumDetailPageViewModel(
            INavigationService navigationService,
            IFlyoutNavigationService flyoutNavigationService,
            IEventAggregator eventAggregator,
            IDataService dataService,
            IImageService imageService,
            IMediaManager mediaManager) : base(navigationService, flyoutNavigationService, dataService, mediaManager, imageService, eventAggregator)
        {
            _dataService = dataService;
            _imageService = imageService;

            _pageSize = 10;
            _pageNumber = 1;
            _hasItems = true;
            HasFurtherAlbums = false;
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            
            Album album = parameters.GetValue<Album>("album");
            LoadData(album);
        }

        public override async void HandleShowAlbum(AlbumSelectionContext context)
        {
            if (PageUtilities.IsCurrentPageTypeOf(typeof(AlbumDetailPage)))
            {
                var navigationParams = new NavigationParameters
                    {
                        {KnownNavigationParameters.Animated,  true },
                        { "album", context.UniqueAlbum.Album }
                    };
                await NavigationService.NavigateAsync($"{nameof(AlbumDetailPage)}", navigationParams);
            }
        }

        protected override async Task PlayAllAsync()
        {
            await PlayTracksAsync(GetTrackIds(), PlayerMode.CD);
        }

        protected override async Task PlayAllRandomizedAsync()
        {
            await PlayTracksAsync(GetTrackIds().ToRandomCollection(), PlayerMode.CD);
        }

        protected override ObservableCollection<int> GetTrackIds()
        {
            return new ObservableCollection<int>(Items.Select(track => ((Track)track.Data).Id));
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

                    _canExecutePlayTrack = true;
                }
            }
        }

        private void LoadData(Album album)
        {
            _ = LoadAlbumAsync(album);
        }

        private async Task LoadAlbumAsync(Album album)
        {
            if (album != null)
            {
                Album = await _dataService.GetAlbumById(album.Id);
                ImageSource = _imageService.GetBitmapSource(Album.AlbumId);
                if (Album.Tracks != null)
                {
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
                }
                PlayAllCommand.RaiseCanExecuteChanged();
                PlayAllRandomizedCommand.RaiseCanExecuteChanged();
                IsBusy = false;

                await LoadMoreAlbumsAsync();
            }
        }

        private async Task LoadMoreAlbumsAsync()
        {
            if (Albums == null)
            {
                return;
            }
            
            if (IsQueryBusy)
            {
                return;
            }

            if (_hasItems)
            {
                IsQueryBusy = true;
                try
                {
                    PagedResult<Album> pagedAlbums = await _dataService.GetPagedAlbums(
                        null,
                        Album.Artist.Id,
                        null,
                        null,
                        null,
                        _pageNumber,
                        _pageSize,
                        AlbumSortOption.Title);

                    if (pagedAlbums?.Items == null|| !pagedAlbums.Items.Any())
                    {
                        _hasItems = false;
                        return;
                    }

                    HasFurtherAlbums = pagedAlbums.TotalCount > 1;
                    
                    if(pagedAlbums.TotalPages == _pageNumber)
                    {
                        _hasItems = false;
                    }

                    if (pagedAlbums.HasNextPage)
                    {
                        _pageNumber++;
                    }

                    foreach (var album in pagedAlbums.Items)
                    {
                        if (album != null)
                        {
                            Albums.Add(new GridPanel
                            {
                                Title = album.Title,
                                SubTitle = album.Artist?.Name,
                                ImageSource = _imageService.GetBitmapSource(album.AlbumId),
                                Data = album
                            });
                        }
                    }
                }
                finally
                {
                    IsQueryBusy = false;
                }
            }
        }

        private void SelectAlbum(GridPanel panel)
        {
            _ = SelectAlbumAsync(panel);
        }

        private async Task SelectAlbumAsync(GridPanel panel)
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
