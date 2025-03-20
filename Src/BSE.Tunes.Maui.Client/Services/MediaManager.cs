using BSE.Tunes.Maui.Client.Collections;
using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models.Contract;
using System.Collections.ObjectModel;

namespace BSE.Tunes.Maui.Client.Services
{
    public class MediaManager : IMediaManager
    {
        private readonly IDataService _dataService;
        private readonly IMediaService _mediaService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettingsService _settingsService;
        private readonly ITimerService _timerService;
        private double _oldProgress;

        public event Action<PlayerState> PlayerStateChanged;
        public event Action<MediaState> MediaStateChanged;

        public NavigableCollection<int> Playlist { get; set; }

        public PlayerMode PlayerMode { get; private set; }

        public PlayerState PlayerState { get; private set; } = PlayerState.Closed;

        public Track CurrentTrack { get; private set; }

        public MediaManager(IDataService dataService,
            IMediaService mediaService,
            IEventAggregator eventAggregator,
            ISettingsService settingsService,
            ITimerService timerService)
        {
            _dataService = dataService;
            _mediaService = mediaService;
            _eventAggregator = eventAggregator;
            _settingsService = settingsService;
            _timerService = timerService;
            _timerService.TimerElapsed += OnTimerElapsed;
            _timerService.Start();
            
            _eventAggregator.GetEvent<CleanUpResourcesEvent>().Subscribe(() =>
            {
                Disconnect();
            }, ThreadOption.UIThread);

            _mediaService.PlayerStateChanged += OnPlayerStateChanged;
            _mediaService.MediaStateChanged += OnMediaStateChanged;
        }

        public void Disconnect()
        {
            // Stop and cleanup MediaElement when we navigate away
            _mediaService?.Disconnect();
        }

        public void Pause()
        {
            _mediaService.Pause();
        }

        public bool CanPlay()
        {
            return Playlist?.Count > 0;
        }

        public void Play()
        {
            _mediaService.Play();
        }

        public async void PlayTracks(PlayerMode playerMode)
        {
            PlayerMode = playerMode;
            int trackId = Playlist?.FirstOrDefault() ?? 0;
            await PlayTrackAsync(trackId);
        }

        public void PlayTracks(ObservableCollection<int> trackIds, PlayerMode playerMode)
        {
            _mediaService.Stop();
            Playlist = trackIds.ToNavigableCollection();
            PlayTracks(playerMode);
        }

        public bool CanPlayPreviousTrack()
        {
            return Playlist?.CanMovePrevious ?? false;
        }

        public async void PlayPreviousTrack()
        {
            if (CanPlayPreviousTrack())
            {
                if (Playlist.MovePrevious())
                {
                    await PlayTrackAsync(Playlist.Current);
                }
            }
        }

        public bool CanPlayNextTrack()
        {
            return Playlist?.CanMoveNext ?? false;
        }

        public async void PlayNextTrack()
        {
            if (CanPlayNextTrack())
            {
                if (Playlist.MoveNext())
                {
                    await PlayTrackAsync(Playlist.Current);
                }
            }
        }

        private async Task PlayTrackAsync(int trackId)
        {
            _mediaService.Stop();
            if (trackId > 0)
            {
                Track track = await _dataService.GetTrackById(trackId);
                if (track != null)
                {
                    await _mediaService.SetTrackAsync(track, _dataService.GetImage(track.Album.AlbumId, true));
                }
            }
        }

        private void OnPlayerStateChanged(PlayerState state)
        {
            if (PlayerState != state)
            {
                PlayerState = state;
                PlayerStateChanged?.Invoke(state);
            }
        }

        private async void OnMediaStateChanged(MediaState state)
        {
            switch (state)
            {
                case MediaState.Opened:
                    var trackId = Playlist.Current;
                    if (trackId > 0)
                    {
                        CurrentTrack = await _dataService.GetTrackById(trackId);
                        UpdateHistoryAsync(CurrentTrack);
                    }
                    break;
                case MediaState.Ended:
                    if (PlayerMode != PlayerMode.None && PlayerMode != PlayerMode.Song && CanPlayNextTrack())
                    {
                        PlayNextTrack();
                    }
                    break;
            }
            MediaStateChanged?.Invoke(state);
        }

        private void OnTimerElapsed()
        {
            var newProgress = _mediaService.Progress;
            if (newProgress != _oldProgress && newProgress < 1.0)
            {
                _eventAggregator.GetEvent<MediaProgressChangedEvent>().Publish(newProgress);
                _oldProgress = newProgress;
            }
        }
        
        private async void UpdateHistoryAsync(Track currentTrack)
        {
            var userName = _settingsService.User.UserName;
            if (!string.IsNullOrEmpty(userName))
            {
                await _dataService.UpdateHistory(new History
                {
                    PlayMode = (int)PlayerMode,
                    AlbumId = currentTrack.Album.Id,
                    TrackId = currentTrack.Id,
                    UserName = userName,
                    PlayedAt = DateTime.Now
                });
            }
        }
    }
}
