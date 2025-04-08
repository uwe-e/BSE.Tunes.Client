using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;

namespace BSE.Tunes.Maui.Client.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IDataService _dataService;
        private readonly ISettingsService _settingsService;
        private readonly IImageService _imageService;

        public PlaylistService(
            IDataService dataService,
            ISettingsService settingsService,
            IImageService imageService)
        {
            _dataService = dataService;
            _settingsService = settingsService;
            _imageService = imageService;
        }

        public Task<bool> AddToPlaylistAsync(PlaylistActionContext context)
        {
            IEnumerable<Track> tracks = default;

            if (context.Data is Track track)
            {
                tracks = Enumerable.Repeat(track, 1);
            }
            if (context.Data is Album album)
            {
                tracks = album.Tracks;
            }
            if (context.Data is PlaylistEntry playlistEntry)
            {
                tracks = Enumerable.Repeat(playlistEntry.Track, 1);
            }
            if (context.Data is Playlist playlist)
            {
                tracks = playlist.Entries?.Select(t => t.Track);
            }

            if (tracks != null)
            {
                return AddTracksToPlaylist(context, tracks);
            }
            return Task.FromResult(false);
        }

        private async Task<bool> AddTracksToPlaylist(PlaylistActionContext context, IEnumerable<Track> tracks)
        {
            var playlistTo = context.PlaylistTo;
            if (playlistTo != null && tracks != null)
            {
                foreach (var track in tracks)
                {
                    if (track != null)
                    {
                        playlistTo.Entries.Add(new PlaylistEntry
                        {
                            PlaylistId = playlistTo.Id,
                            TrackId = track.Id,
                            Guid = Guid.NewGuid()
                        });
                    }
                }
                await _dataService.AppendToPlaylist(playlistTo);
                await _imageService.RemoveStitchedBitmaps(playlistTo.Id);
                return true;
            }
            return false;
        }

        public Task<Playlist> CreatePlaylistAsync(string name)
        {
            return _dataService.InsertPlaylist(new Playlist
            {
                Name = name,
                UserName = _settingsService.User.UserName,
                Guid = Guid.NewGuid()
            });
        }
    }

}
