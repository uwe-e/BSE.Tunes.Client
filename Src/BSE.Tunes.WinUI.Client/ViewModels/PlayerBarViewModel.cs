using BSE.Tunes.WinUI.Client.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class PlayerBarViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly IMediaManager _mediaManager;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private BitmapImage? _coverSource;

        [ObservableProperty]
        private Track _currentTrack;

        [ObservableProperty]
        private double _progress;

        [ObservableProperty]
        private string _currentPosition = "00:00:00";

        [ObservableProperty]
        private string _totalDuration = "00:00:00";

        [ObservableProperty]
        private PlayerState _playerState = PlayerState.Stopped;

        [ObservableProperty]
        private bool _canPlayPrevious;

        [ObservableProperty]
        private bool _canPlayNext;

        public PlayerBarViewModel(
            IDataService dataService,
            IImageService imageService,
            IMediaManager mediaManager,
            IMessenger messenger)
        {
            _dataService = dataService;
            _imageService = imageService;
            _mediaManager = mediaManager;
            _mediaManager.MediaStateChanged += HandleMediaManagerMediaStateChanged;
            _mediaManager.PlayerStateChanged += HandleMediaManagerPlayerStateChanged;
            _mediaManager.PlaylistCollectionChanged += HandlePlaylistCollectionChanged;
            _messenger = messenger;

            _messenger.Register<PlayerBarViewModel, TrackChangedMessage>(this, (r, m) =>
            {
                if (m.Track != null)
                {
                    LoadCoverSource(m.Track);
                    CurrentTrack = m.Track;
                }
            });
            
            _messenger.Register<PlayerBarViewModel, MediaProgressChangedMessage>(this, (r, m) =>
            {
                Progress = m.Progress;
                CurrentPosition = FormatTime(m.Position);
                TotalDuration = FormatTime(m.Duration);

                System.Diagnostics.Debug.WriteLine($"Progress: {m.Progress:P1}, Position: {CurrentPosition}, Duration: {TotalDuration}");
            });
        }

        private string FormatTime(TimeSpan timeSpan)
        {
            // You can now easily format the TimeSpan in any way you need
            // For example: hh:mm:ss, mm:ss, or even include milliseconds
            if (timeSpan == TimeSpan.Zero)
                return "00:00:00";

            return timeSpan.ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Handles player state changes from the MediaManager (Playing, Paused, Stopped, etc.)
        /// and synchronizes the local PlayerState property for UI binding.
        /// Note: Different from the MVVM Toolkit's OnPlayerStateChanged partial method which is
        /// called when the PlayerState property value changes.
        /// </summary>
        /// <param name="state"></param>
        private void HandleMediaManagerPlayerStateChanged(BSE.Tunes.Shared.Services.Enums.PlayerState state)
        {
            System.Diagnostics.Debug.WriteLine($"PlayerBarViewModel: PlayerState changed to {state}");
            PlayerState = state;
            UpdateNavigationButtonStates();
        }

        /// <summary>
        /// Handle changes to the playlist collection from the MediaManager.
        /// Whenever the playlist changes, we need to update the navigation button states
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandlePlaylistCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateNavigationButtonStates();
        }

        /// <summary>
        /// Handle the MediaStateChanged event from the media manager. When a new track is opened, update the current track and cover source.
        /// </summary>
        /// <param name="state"></param>
        private void HandleMediaManagerMediaStateChanged(MediaState state)
        {
            System.Diagnostics.Debug.WriteLine($"PlayerBarViewModel: MediaState changed to {state}");
            switch (state)
            {
                case MediaState.Opened:
                    CurrentTrack = _mediaManager.CurrentTrack;
                    LoadCoverSource(CurrentTrack);
                    UpdateNavigationButtonStates();
                    break;
            }
        }

        /// <summary>
        /// Updates the CanPlayPrevious and CanPlayNext properties based on the media manager state.
        /// </summary>
        private void UpdateNavigationButtonStates()
        {
            CanPlayPrevious = _mediaManager.CanPlayPreviousTrack();
            CanPlayNext = _mediaManager.CanPlayNextTrack();
        }

        private void LoadCoverSource(Track track)
        {
            if (track != null && track.Album != null)
            {
                var bitmapSource = _imageService.GetBitmapSource(track.Album.AlbumId, true);
                Uri coverSource = new Uri(bitmapSource);

                if (!coverSource.Equals(CoverSource?.UriSource))
                {
                    CoverSource = new BitmapImage(coverSource);
                }
            }
        }

        [RelayCommand]
        private void Play()
        {
            if (_mediaManager.PlayerState is PlayerState.Closed or PlayerState.Stopped)
            {
                if (_mediaManager.CanPlay())
                {
                    _ = _mediaManager.PlayTracksAsync(PlayerMode.Playlist);
                }
            }
            else if (_mediaManager.PlayerState == PlayerState.Paused)
            {
                _mediaManager.Play();
            }
            else if (_mediaManager.PlayerState == PlayerState.Playing)
            {
                _mediaManager.Pause();
            }
        }

        [RelayCommand(CanExecute = nameof(CanPlayPrevious))]
        private async Task PlayPreviousAsync()
        {
            await _mediaManager.PlayPreviousTrackAsync();
        }

        [RelayCommand(CanExecute = nameof(CanPlayNext))]
        private async Task PlayNextAsync()
        {
            await _mediaManager.PlayNextTrackAsync();
        }

        /// <summary>
        /// Notifies command can-execute states when navigation properties change.
        /// </summary>
        partial void OnCanPlayPreviousChanged(bool value)
        {
            PlayPreviousCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Notifies command can-execute states when navigation properties change.
        /// </summary>
        partial void OnCanPlayNextChanged(bool value)
        {
            PlayNextCommand.NotifyCanExecuteChanged();
        }
    }
}
