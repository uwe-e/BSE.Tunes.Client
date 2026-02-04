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
        private readonly ISettingsService _settingsService;

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
            IEventAggregator eventAggregator,
            ISettingsService settingsService) : base(navigationService, flyoutNavigationService, dataService, mediaManager, imageService, eventAggregator)
        {
            _dataService = dataService;
            _imageService = imageService;
            _eventAggregator = eventAggregator;
            _settingsService = settingsService;

            _pageSize = 20;
            _pageNumber = 1;
            _hasItems = true;

            _eventAggregator.GetEvent<PlaylistActionContextChanged>().Subscribe(async args =>
            {
                if (args is PlaylistActionContext managePlaylistContext)
                {
                    //if (managePlaylistContext.ActionMode == PlaylistActionMode.PlaylistUpdated)
                    //{
                    //    var playlist = await _dataService.GetPlaylistById(Playlist.Id);
                    //    ImageSource = null;

                    //    // If there's a playlistentry that has changed and there's no playlistTo object,
                    //    // then it's probably an entry within this current playlist detail that has been removed.
                    //    var shouldUpdateImage = managePlaylistContext.PlaylistTo == null && managePlaylistContext.Data is PlaylistEntry;
                    //    var isTargetPlaylist = managePlaylistContext.PlaylistTo?.Id == Playlist.Id;

                    //    if (shouldUpdateImage || isTargetPlaylist)
                    //    {
                    //        ImageSource = await _imageService.GetStitchedBitmapSourceAsync(Playlist.Id, playlist.CoverAlbumIds);

                    //        if (isTargetPlaylist)
                    //        {
                    //            Items.Clear();
                    //            _pageNumber = 1;
                    //            _hasItems = true;
                    //            await FetchPlaylistEntriesAsync(Playlist.Id);
                    //        }
                    //    }


                    //    //// if there's a playlistentry that has changed..
                    //    //// and there's no playlistTo object, then it's probably an entry within this current playlist detail that has been removed. 
                    //    //if (managePlaylistContext.PlaylistTo == null && managePlaylistContext.Data is PlaylistEntry playlistEntry)
                    //    //{
                    //    //    //if so, then we need a new image
                    //    //    ImageSource = null;
                    //    //    ImageSource = await _imageService.GetStitchedBitmapSourceAsync(Playlist.Id);
                    //    //}

                    //    //if (managePlaylistContext.PlaylistTo?.Id == Playlist.Id)
                    //    //{
                    //    //    await LoadDataAsync(managePlaylistContext.PlaylistTo);
                    //    //}
                    //}
                    if (managePlaylistContext.ActionMode == PlaylistActionMode.ShowAlbum)
                    {
                        await ShowAlbumAsync(managePlaylistContext);
                    }
                }
            });

            _eventAggregator.GetEvent<AlbumInfoSelectionEvent>().ShowAlbum(async (uniqueTrack) =>
            {
                if (PageUtilities.IsCurrentPageTypeOf(typeof(PlaylistDetailPage)))
                {
                    var navigationParams = new NavigationParameters
                    {
                        {KnownNavigationParameters.Animated,  true },
                        { "album", uniqueTrack.Album }
                    };
                    await NavigationService.NavigateAsync($"{nameof(AlbumDetailPage)}", navigationParams);
                }
            });
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
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
                ImageSource = await _imageService.GetStitchedBitmapSourceAsync(Playlist.Id, playlist.CoverAlbumIds);

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
            await base.RemoveFromPlaylistAsync(managePlaylistContext);
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
                    var imageTask = _imageService.GetStitchedBitmapSourceAsync(playlist.Id, playlist.CoverAlbumIds);
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

        private async Task UpdateCurrentPlaylistAsync(PlaylistActionContext managePlaylistContext)
        {
            IsBusy = true;
            if (managePlaylistContext.Data is PlaylistEntry playlistEntry)
            {
                Playlist.Entries.Remove(playlistEntry);

                await _dataService.DeletePlaylistEntryAsync(playlistEntry);

                GridPanel panel = Items.Where(p => p.Id == playlistEntry.Id).FirstOrDefault<GridPanel>();
                Items.Remove(panel);

                await _imageService.RemoveStitchedBitmaps(playlistEntry.PlaylistId);

                managePlaylistContext.ActionMode = PlaylistActionMode.PlaylistUpdated;
                _eventAggregator.GetEvent<PlaylistActionContextChanged>().Publish(managePlaylistContext);

            }
            IsBusy = false;
        }

        private async Task ShowAlbumAsync(PlaylistActionContext managePlaylistContext)
        {
            if (managePlaylistContext?.Data is PlaylistEntry playlistEntry)
            {
                var album = playlistEntry.Track?.Album;
                if (album != null)
                {
                    var navigationParams = new NavigationParameters
                    {
                        {KnownNavigationParameters.Animated,  true },
                        { "album", album }
                    };
                    await NavigationService.NavigateAsync($"{nameof(AlbumDetailPage)}", navigationParams);
                }
            }
        }
    }
}
