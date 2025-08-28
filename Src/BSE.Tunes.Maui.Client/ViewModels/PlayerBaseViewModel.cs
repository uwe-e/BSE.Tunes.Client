using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using System.Collections.Specialized;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class PlayerBaseViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMediaManager _mediaManager;
        private ICommand _playCommand;
        private DelegateCommand _playNextCommand;
        private DelegateCommand _playPreviousCommand;
        private PlayerState _playerState;
        private bool _isPlaying;
        private Track _currentTrack;
        private double _progress;

        public ICommand PlayCommand => _playCommand ??= new DelegateCommand(async() => await PlayAsync());
        public DelegateCommand PlayNextCommand => _playNextCommand ??= new DelegateCommand(async() => await PlayNextAsync(), CanPlayNext);
        public DelegateCommand PlayPreviousCommand => _playPreviousCommand ??= new DelegateCommand(async () => await PlayPreviousAsync(), CanPlayPrevious);

        public PlayerState PlayerState
        {
            get { return _playerState; }
            set { SetProperty<PlayerState>(ref _playerState, value); }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { SetProperty<bool>(ref _isPlaying, value); }
        }

        public Track CurrentTrack
        {
            get { return _currentTrack; }
            set { SetProperty<Track>(ref _currentTrack, value); }
        }

        public double Progress
        {
            get { return _progress; }
            set { SetProperty<double>(ref _progress, value); }
        }

        public PlayerBaseViewModel(INavigationService navigationService,
            IEventAggregator eventAggregator,
            IMediaManager mediaManager) : base(navigationService)
        {
            _eventAggregator = eventAggregator;
            _mediaManager = mediaManager;
            _mediaManager.PlayerStateChanged += OnPlayerStateChanged;
            _mediaManager.MediaStateChanged += OnMediaStateChanged;
            _mediaManager.PlaylistCollectionChanged += OnPlaylistCollectionChanged;

            _eventAggregator.GetEvent<MediaProgressChangedEvent>().Subscribe((progress) =>
            {
                Progress = progress;
            }, ThreadOption.UIThread);
        }

        protected virtual void OnPlaylistCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            PlayPreviousCommand.RaiseCanExecuteChanged();
            PlayNextCommand.RaiseCanExecuteChanged();
        }

        protected virtual void OnTrackChanged(Track track)
        {
        }

        private void OnMediaStateChanged(MediaState state)
        {
            switch (state)
            {
                case MediaState.Opened:
                    OnMediaOpened();
                    break;
            }
        }

        private void OnMediaOpened()
        {
            CurrentTrack = _mediaManager.CurrentTrack;
            PlayNextCommand.RaiseCanExecuteChanged();
            PlayPreviousCommand.RaiseCanExecuteChanged();
            
            OnTrackChanged(CurrentTrack);
        }

        private void OnPlayerStateChanged(PlayerState state)
        {
            PlayerState = state;
            IsPlaying = state == PlayerState.Playing;
        }

        private async Task PlayAsync()
        {
            switch (PlayerState)
            {
                case PlayerState.Closed:
                case PlayerState.Stopped:
                    if (_mediaManager.CanPlay())
                    {
                        await _mediaManager.PlayTracksAsync(PlayerMode.Playlist);
                    }
                    break;
                case PlayerState.Paused:
                    _mediaManager.Play();
                    break;
                case PlayerState.Playing:
                    _mediaManager.Pause();
                    break;

            }
        }

        private bool CanPlayNext()
        {
            bool canPlayNext = _mediaManager.CanPlayNextTrack();
            return canPlayNext;
            //return _mediaManager.CanPlayNextTrack();
        }

        private async Task PlayNextAsync()
        {
            await _mediaManager.PlayNextTrackAsync();
        }
        
        private bool CanPlayPrevious()
        {
            return _mediaManager.CanPlayPreviousTrack();
        }

        private async Task PlayPreviousAsync()
        {
            await _mediaManager.PlayPreviousTrackAsync();
        }
    }
}
