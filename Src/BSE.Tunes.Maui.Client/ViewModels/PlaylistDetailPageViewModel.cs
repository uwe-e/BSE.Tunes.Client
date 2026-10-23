using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using BSE.Tunes.Shared.Services.Media;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class PlaylistDetailPageViewModel : TracklistBaseViewModel
    {
        private Playlist _playlist;
        private bool _canExecutePlayTrack = true;
        private int _pageNumber;
        private readonly int _pageSize;
        private bool _hasItems;
        private ICommand _remainingPlaylistEntriesThresholdReachedCommand;
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly IEventAggregator _eventAggregator;

        public Playlist Playlist
        {
            get => _playlist;
            set => SetProperty<Playlist>(ref _playlist, value);
        }

        public ICommand RemainingPlaylistEntriesThresholdReachedCommand => _remainingPlaylistEntriesThresholdReachedCommand ??= new DelegateCommand(async () =>
        {
            if (Playlist != null)
            {
                await FetchPlaylistEntriesAsync(Playlist.Id);
            }
        });

        public PlaylistDetailPageViewModel(
            INavigationService navigationService,
            IFlyoutNavigationService flyoutNavigationService,
            IDataService dataService,
            IMediaManager mediaManager,
            IImageService imageService,
            IEventAggregator eventAggregator) : base(navigationService, flyoutNavigationService, dataService, mediaManager, imageService, eventAggregator)
        {
            _dataService = dataService;
            _imageService = imageService;
            _eventAggregator = eventAggregator;

            _pageSize = 20;
            _pageNumber = 1;
            _hasItems = true;
        }

        public override async void HandleShowAlbum(AlbumSelectionContext context)
        {
            if (PageUtilities.IsCurrentPageTypeOf(typeof(PlaylistDetailPage)))
            {
                var navigationParams = new NavigationParameters
                    {
                        {KnownNavigationParameters.Animated,  true },
                        { "album", context.UniqueAlbum.Album }
                    };
                await NavigationService.NavigateAsync($"{nameof(AlbumDetailPage)}", navigationParams);
            }
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            
            Playlist playlist = parameters.GetValue<Playlist>("playlist");
            await LoadDataAsync(playlist);
        }

        protected override bool CanExecutePlayTrack(GridPanel panel)
        {
            return _canExecutePlayTrack;
        }

        protected override async Task PlayTrackAsync(GridPanel panel)
        {
            if (panel?.Data is PlaylistEntry entry)
            {
                if (CanExecutePlayTrack(panel))
                {
                    _canExecutePlayTrack = false;
                    
                    await PlayTracksAsync(new List<int>
                    {
                        entry.TrackId
                    }, PlayerMode.Song);

                    _canExecutePlayTrack = true;
                }
            }
        }

        protected override async Task PlayAllAsync()
        {
            IList<int> trackIds = await _dataService.GetTrackIdsByPlaylistId(Playlist.Id);
            await PlayTracksAsync(trackIds, PlayerMode.Playlist);
        }

        protected override async Task PlayAllRandomizedAsync()
        {
            IList<int> trackIds = await _dataService.GetTrackIdsByPlaylistId(Playlist.Id, randomize: true);
            await PlayTracksAsync(trackIds, PlayerMode.Playlist);
        }

        protected override async Task OnPlaylistUpdatedAsync(PlaylistActionContext context)
        {
            var playlist = await _dataService.GetPlaylistById(Playlist.Id);
            ImageSource = null;

            // If there's a playlistentry that has changed and there's no playlistTo object,
            // then it's probably an entry within this current playlist detail that has been removed.
            var shouldUpdateImage = context.PlaylistTo == null && context.Data is PlaylistEntry;
            var isTargetPlaylist = context.PlaylistTo?.Id == Playlist.Id;

            if (shouldUpdateImage || isTargetPlaylist)
            {
                ImageSource = await _imageService.GetComposedBitmapSourceAsync(Playlist.Id, playlist.CoverAlbumIds);

                if (isTargetPlaylist)
                {
                    Items.Clear();
                    _pageNumber = 1;
                    _hasItems = true;
                    await FetchPlaylistEntriesAsync(Playlist.Id);
                }
            }

        }

        protected override async Task RemoveFromPlaylistAsync(PlaylistActionContext managePlaylistContext)
        {
            await UpdateCurrentPlaylistAsync(managePlaylistContext);
        }

        private async Task LoadDataAsync(Playlist playlist)
        {
            if (playlist != null)
            {
                Items.Clear();
                ImageSource = null;

                Playlist = await _dataService.GetPlaylistById(playlist.Id);
                if (Playlist != null)
                {
                    IsBusy = false;
                    
                    // Start both tasks concurrently
                    var imageTask = _imageService.GetComposedBitmapSourceAsync(playlist.Id, playlist.CoverAlbumIds);
                    var entriesTask = FetchPlaylistEntriesAsync(playlist.Id);
                    
                    // Await both tasks
                    ImageSource = await imageTask;
                    await entriesTask;

                    PlayAllCommand.RaiseCanExecuteChanged();
                    PlayAllRandomizedCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private async Task FetchPlaylistEntriesAsync(int playlistId)
        {
            if (IsBusy)
            {
                return;
            }

            if (_hasItems)
            {
                IsBusy = true;
                try
                {
                    var pagedEntries = await _dataService.GetPagedPlaylistEntriesByIdAsync(
                                        playlistId,
                                        _pageNumber,
                                        _pageSize);

                    if (pagedEntries?.Items == null || !pagedEntries.Items.Any())
                    {
                        _hasItems = false;
                        return;
                    }

                    if (pagedEntries.TotalPages == _pageNumber)
                    {
                        _hasItems = false;
                    }

                    if (pagedEntries.HasNextPage)
                    {
                        _pageNumber++;
                    }

                    foreach (PlaylistEntry entry in pagedEntries.Items.OrderBy(pe => pe.SortOrder))
                    {
                        if (entry != null)
                        {
                            Items.Add(new GridPanel
                            {
                                Id = entry.Id,
                                Title = entry.Name,
                                SubTitle = entry.Track?.Album?.Artist?.Name,
                                ImageSource = _imageService.GetBitmapSource(entry.AlbumId, true),
                                Data = entry
                            });
                        }
                    }
                }
                finally
                {
                    IsBusy = false;

                }
            }
        }

        private async Task UpdateCurrentPlaylistAsync(PlaylistActionContext context)
        {
            IsBusy = true;
            if (context.Data is PlaylistEntry playlistEntry)
            {
                Playlist.Entries.Remove(playlistEntry);

                await _dataService.DeletePlaylistEntryAsync(playlistEntry);

                int entryId = playlistEntry.Id;
                int playlistId = playlistEntry.PlaylistId;

                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    if (Items[i].Id == entryId)
                    {
                        Items.RemoveAt(i);
                        break;
                    }
                }

                await _imageService.RemoveComposedBitmaps(playlistId);

                context.ActionMode = PlaylistActionMode.PlaylistUpdated;
                _eventAggregator.GetEvent<PlaylistActionContextChanged>().Publish(context);

            }
            IsBusy = false;
        }
    }
}
