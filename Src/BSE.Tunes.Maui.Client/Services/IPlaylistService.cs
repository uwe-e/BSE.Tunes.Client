using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;

namespace BSE.Tunes.Maui.Client.Services
{
    public interface IPlaylistService
    {
        //Task<Playlist> GetPlaylistAsync(string playlistId);
        //Task<IEnumerable<Playlist>> GetPlaylistsAsync();
        Task<Playlist> CreatePlaylistAsync(string name);
        Task<bool> AddToPlaylistAsync(PlaylistActionContext context);
        //Task<bool> UpdatePlaylistAsync(Playlist playlist);
        //Task<bool> DeletePlaylistAsync(string playlistId);
        //Task<bool> AddTrackToPlaylistAsync(string playlistId, string trackId);
        //Task<bool> RemoveTrackFromPlaylistAsync(string playlistId, string trackId);
        //Task<IEnumerable<Track>> GetTracksInPlaylistAsync(string playlistId);
    }
}
