using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using System.Collections.ObjectModel;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class PlaylistDetailPageViewModel : TracklistBaseViewModel
    {
        private Playlist _playlist;
        private bool _canExecutePlayTrack = true;
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettingsService _settingsService;

        public Playlist Playlist
        {
            get => _playlist;
            set => SetProperty<Playlist>(ref _playlist, value);
        }

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

            _eventAggregator.GetEvent<PlaylistActionContextChanged>().Subscribe(async args =>
            {
                if (args is PlaylistActionContext managePlaylistContext)
                {
                    if (managePlaylistContext.ActionMode == PlaylistActionMode.PlaylistUpdated)
                    {
                        // if there's a playlistentry that has changed..
                        // and there's no playlistTo object, then it's probably an entry within this current playlist detail that has been removed. 
                        if (managePlaylistContext.PlaylistTo == null && managePlaylistContext.Data is PlaylistEntry playlistEntry)
                        {
                            //if so, then we need a new image
                            ImageSource = null;
                            ImageSource = await _imageService.GetStitchedBitmapSourceAsync(Playlist.Id);
                        }

                        if (managePlaylistContext.PlaylistTo?.Id == Playlist.Id)
                        {
                            await LoadDataAsync(managePlaylistContext.PlaylistTo);
                        }
                    }
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

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            Playlist playlist = parameters.GetValue<Playlist>("playlist");
            LoadData(playlist);
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
            await PlayTracksAsync(GetTrackIds(), PlayerMode.Playlist);
        }

        protected override async Task PlayAllRandomizedAsync()
        {
            await PlayTracksAsync(GetTrackIds().ToRandomCollection(), PlayerMode.Playlist);
        }

        protected override ObservableCollection<int> GetTrackIds()
        {
            return new ObservableCollection<int>(Items.Select(track => ((PlaylistEntry)track.Data).TrackId));
        }

        protected override async Task RemoveFromPlaylistAsync(PlaylistActionContext managePlaylistContext)
        {
            await base.RemoveFromPlaylistAsync(managePlaylistContext);
            await UpdateCurrentPlaylistAsync(managePlaylistContext);
        }

        private void LoadData(Playlist playlist)
        {
            _ = LoadDataAsync(playlist);
        }

        private async Task LoadDataAsync(Playlist playlist)
        {
            if (playlist != null)
            {
                Items.Clear();
                ImageSource = null;

                Playlist = await _dataService.GetPlaylistById(playlist.Id, _settingsService.User.UserName);
                if (Playlist != null)
                {
                    ImageSource = await _imageService.GetStitchedBitmapSourceAsync(playlist.Id);

                    foreach (var entry in Playlist.Entries?.OrderBy(pe => pe.SortOrder))
                    {
                        if (entry != null)
                        {
                            Items.Add(new GridPanel
                            {
                                Id = entry.Id,
                                Title = entry.Name,
                                SubTitle = entry.Artist,
                                ImageSource = _imageService.GetBitmapSource(entry.AlbumId, true),
                                Data = entry
                            });
                        }
                    }

                    PlayAllCommand.RaiseCanExecuteChanged();
                    PlayAllRandomizedCommand.RaiseCanExecuteChanged();

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

                var playlist = await _dataService.UpdatePlaylist(Playlist);

                GridPanel panel = Items.Where(p => p.Id == playlistEntry.Id).FirstOrDefault<GridPanel>();
                Items.Remove(panel);

                await _imageService.RemoveStitchedBitmaps(playlist.Id);

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
