using BSE.Tunes.WinUI.Client.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Imaging;

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

        public PlayerBarViewModel(
            IDataService dataService,
            IImageService imageService,
            IMediaManager mediaManager,
            IMessenger messenger)
        {
            _dataService = dataService;
            _imageService = imageService;
            _mediaManager = mediaManager;
            _mediaManager.MediaStateChanged += OnMediaStateChanged; ;
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
                    // Could update play/pause button state here if needed
                    // For now, we just trigger a property change for CurrentTrack to update the UI
                    Progress = m.Progress; // Trigger UI update

                    System.Diagnostics.Debug.WriteLine($"Progress updated: {m.Progress}");
                });
        }

        private void OnMediaStateChanged(MediaState state)
        {
            switch (state)
            {
                case MediaState.Opened:
                    CurrentTrack = _mediaManager.CurrentTrack;
                    LoadCoverSource(CurrentTrack);
                    break;
            }
        }

        //private void OnMediaOpened()
        //{
        //    CurrentTrack = _mediaManager.CurrentTrack;
        //    LoadCoverSource(CurrentTrack);
        //}

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
    }
}
