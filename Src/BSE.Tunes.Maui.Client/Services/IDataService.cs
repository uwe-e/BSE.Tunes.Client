using BSE.Tunes.Maui.Client.Models.Contract;
using System.Collections.ObjectModel;

namespace BSE.Tunes.Maui.Client.Services
{
    public interface IDataService
    {
        Task<ObservableCollection<Album>> GetAlbumsByArtist(int artistId, int skip, int limit);
        Task<Album> GetAlbumById(int albumId);
        Task<Album[]> GetAlbumSearchResults(string query, int skip, int limit);
        Task<Album[]> GetAlbumSearchResults(string query, int skip, int limit, CancellationToken token);
        Task<ObservableCollection<Album>> GetFeaturedAlbums(int limit);
        Task<ObservableCollection<Album>> GetNewestAlbums(int limit);
        Task<int> GetNumberOfAlbumsByGenre(int? genreId);
        Task<ObservableCollection<Album>> GetAlbumsByGenre(int? genreId, int skip, int limit);
        Uri GetImage(Guid imageId, bool asThumbnail = false);
        Task<Playlist> AppendToPlaylist(Playlist playlist);
        Task DeletePlaylist(int playlistId);
        Task<ObservableCollection<Playlist>> GetPlaylistsByUserName(string userName, int skip, int limit);
        Task<Playlist> GetPlaylistById(int playlistId, string userName);
        Task<Playlist> GetPlaylistByIdWithNumberOfEntries(int playlistId, string userName);
        Task<ObservableCollection<Guid>> GetPlaylistImageIdsById(int playlistId, string userName, int limit);
        Task<Playlist> InsertPlaylist(Playlist playlist);
        Task<Playlist> UpdatePlaylist(Playlist playlist);
        Task<SystemInfo> GetSystemInfo();
        Task<Track[]> GetTracksByAlbumId(int albumId);
        Task<Track> GetTrackById(int trackId);
        Task<ObservableCollection<int>> GetTrackIdsByGenre(int? genreId = null);
        Task<Track[]> GetTrackSearchResults(string query, int skip, int limit);
        Task<Track[]> GetTrackSearchResults(string query, int skip, int limit, CancellationToken token);
        Task<bool> IsEndPointAccessibleAsync();
        Task<bool> IsEndPointAccessibleAsync(string serviceEndPoint);
        Task<bool> UpdateHistory(History history);
    }
}
