
using Android.OS.Storage;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class PlayerBaseViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMediaManager _mediaManager;
        private DelegateCommand _playCommand;
        private DelegateCommand _playNextCommand;
        private PlayerState _playerState;
        private bool _isPlaying;
        private Track _currentTrack;

        public DelegateCommand PlayCommand => _playCommand ??= new DelegateCommand(Play);
        public DelegateCommand PlayNextCommand => _playNextCommand ??= new DelegateCommand(PlayNext, CanPlayNext);
        

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

        public PlayerBaseViewModel(INavigationService navigationService,
            IEventAggregator eventAggregator,
            IMediaManager mediaManager) : base(navigationService)
        {
            _mediaManager = mediaManager;
            _mediaManager.PlayerStateChanged += OnPlayerStateChanged;
            _mediaManager.MediaStateChanged += OnMediaStateChanged;
        }

        protected virtual void OnTrackChanged(Track track)
        {
        }

        public override void OnDisappearing()
        {

            if (_isPlaying)
            {
                //_mediaManager.Stop();
                _mediaManager.Disconnect();
            }
            base.OnDisappearing();
        }

        private void OnMediaStateChanged(MediaState state)
        {
            switch (state) {
            case MediaState.Opened:
                    OnMediaOpened();
                    break;
            }

        }

        private void OnMediaOpened()
        {
            CurrentTrack = _mediaManager.CurrentTrack;
            OnTrackChanged(CurrentTrack);
        }

        private void OnPlayerStateChanged(PlayerState state)
        {
            PlayerState = state;
            IsPlaying = state == PlayerState.Playing;
        }

        private void Play()
        {
            switch (PlayerState)
            {
                case PlayerState.Closed:
                case PlayerState.Stopped:
                    if (_mediaManager.CanPlay())
                    {
                        _mediaManager.PlayTracks(PlayerMode.Playlist);
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
            return _mediaManager.CanPlayNextTrack();
        }

        private void PlayNext()
        {
            _mediaManager.PlayNextTrack();
        }
    }
}
