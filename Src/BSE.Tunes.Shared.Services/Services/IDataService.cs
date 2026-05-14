using BSE.Tunes.Shared.Services.Models.Contract;
using BSEtunes.Contracts.Enums;

namespace BSE.Tunes.Shared.Services
{
    public interface IDataService
    {
        Task<Playlist> CreatePlaylistAsync(string playlistName);
        Task<Album> GetAlbumById(int albumId);
        Task<PagedResult<Album>> GetAlbumSearchResults(string query, int skip, int limit);
        Task<PagedResult<Album>> GetAlbumSearchResults(string query, int skip, int limit, CancellationToken token);
        Task<IList<Album>> GetFeaturedAlbums(int limit);
        Task<IList<Album>> GetNewestAlbums(int limit);
        Task<PagedResult<Album>> GetPagedAlbums(
            string? genre,
            int? artistId,
            string? artistName,
            int? yearFrom,
            int? yearTo,
            int pageNumber,
            int pageSize,
            AlbumSortOption albumSortOption = AlbumSortOption.Title);
        Uri GetAlbumCoverUriById(Guid albumId, bool asThumbnail = false);
        Task<IReadOnlyList<Genre>> GetAvailableGenresAsync();
        Uri GetImage(Guid imageId, bool asThumbnail = false);
        Task AppendToPlaylist(int playlistId, IList<int> trackIds);
        Task DeletePlaylist(int playlistId);
        Task DeletePlaylistEntryAsync(PlaylistEntry playlistEntry);
        Task<PagedResult<Playlist>> GetPagedPlaylistsByOwnerAsync(int pageNumber, int pageSize);
        Task<PagedResult<PlaylistEntry>> GetPagedPlaylistEntriesByIdAsync(int playlistId, int pageNumber, int pageSize);
        Task<Playlist> GetPlaylistById(int playlistId);
        Task<int> GetAvailableTrackCount();
        Task<Track> GetTrackById(int trackId);
        Task<IList<int>> GetTrackIdsByGenre(int? genreId = null);
        Task<IList<int>> GetTrackIdsByPlaylistId(int playlistId, bool randomize = false);
        Task<PagedResult<Track>> GetTrackSearchResults(string query, int skip, int limit);
        Task<PagedResult<Track>> GetTrackSearchResults(string query, int skip, int limit, CancellationToken token);
        Task<bool> IsEndPointAccessibleAsync();
        Task<bool> IsEndPointAccessibleAsync(string serviceEndPoint);
        Task<bool> UpdateHistory(History history);
        Task<IReadOnlyList<PlaylistSummary>> GetAllPlaylists();
        Task DeletePlaylistEntriesAsync(int playlistId, List<int> entryIds);
        Task UpdatePlaylistEntriesSortOrderAsync(int playlistId, List<int> entryIds);
    }
}
