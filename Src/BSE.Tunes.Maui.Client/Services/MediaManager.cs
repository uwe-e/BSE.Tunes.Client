using BSE.Tunes.Maui.Client.Collections;
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

        public MediaManager(IDataService dataService,
            IMediaService mediaService,
            IEventAggregator eventAggregator,
            ISettingsService settingsService)
        {
            _dataService = dataService;
            _mediaService = mediaService;
            _eventAggregator = eventAggregator;
            _settingsService = settingsService;
        }

        public NavigableCollection<int> Playlist { get; set; }

        public PlayerMode PlayerMode { get; private set; }

        public PlayerState PlayerState { get; private set; } = PlayerState.Closed;

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

        private async Task PlayTrackAsync(int trackId)
        {
            _mediaService.Stop();
            if (trackId > 0)
            {
                Track track = await _dataService.GetTrackById(trackId);
                if (track != null)
                {
                    _mediaService.SetTrack(track, _dataService.GetImage(track.Album.AlbumId, true));
                }
            }
        }
    }
}
