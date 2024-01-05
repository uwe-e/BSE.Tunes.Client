using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models.Contract;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace BSE.Tunes.Maui.Client.Services
{
    public class DataService : IDataService
    {
        private readonly IRequestService _requestService;
        private readonly ISettingsService _settingsService;

        public DataService(IRequestService requestService, ISettingsService settingsService)
        {
            _requestService = requestService;
            _settingsService = settingsService;
        }

        public Task<bool> IsEndPointAccessibleAsync()
        {
            return IsEndPointAccessibleAsync(_settingsService.ServiceEndPoint);
        }

        public async Task<bool> IsEndPointAccessibleAsync(string serviceEndPoint)
        {
            bool isAccessible = false;
            var builder = new UriBuilder(serviceEndPoint);
            builder.AppendToPath("api/tunes/IsHostAccessible");

            using (var client = await _requestService.GetHttpClient(false))
            {
                try
                {
                    var responseMessage = await client.GetAsync(builder.Uri);
                    var serialized = await responseMessage.Content.ReadAsStringAsync();
                    isAccessible = await Task.Run(() => JsonSerializer.Deserialize<bool>(serialized));
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return isAccessible;
        }

        public Task<ObservableCollection<Album>> GetAlbumsByArtist(int artistId, int skip = 0, int limit = 10)
        {
            string strUrl = $"{this._settingsService.ServiceEndPoint}/api/v2/artists/{artistId}/albums?skip={skip}&limit={limit}";
            return _requestService.GetAsync<ObservableCollection<Album>>(new UriBuilder(strUrl).Uri);
        }

        public Task<ObservableCollection<Album>> GetFeaturedAlbums(int limit)
        {
            string strUrl = string.Format("{0}/api/v2/albums/featured?limit={1}", this._settingsService.ServiceEndPoint, limit);
            return _requestService.GetAsync<ObservableCollection<Album>>(new UriBuilder(strUrl).Uri);
        }

        public Task<ObservableCollection<Album>> GetNewestAlbums(int limit)
        {
            string strUrl = string.Format("{0}/api/v2/albums/newest?limit={0}", this._settingsService.ServiceEndPoint, limit);
            return _requestService.GetAsync<ObservableCollection<Album>>(new UriBuilder(strUrl).Uri);
        }

        public Uri GetImage(Guid imageId, bool asThumbnail = false)
        {
            var builder = new UriBuilder(_settingsService.ServiceEndPoint);
            builder.AppendToPath(string.Format("/api/files/image/{0}", imageId.ToString()));

            return builder.Uri;
        }

        public Task<Album> GetAlbumById(int albumId)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/albums/{albumId}";
            return _requestService.GetAsync<Album>(new UriBuilder(strUrl).Uri);
        }
        
        public Task<Album[]> GetAlbumSearchResults(string query, int skip, int limit)
        {
            query = System.Web.HttpUtility.UrlEncode(query);
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/search/albums/search/?query={query}&skip={skip}&limit={limit}";
            return _requestService.GetAsync<Album[]>(new UriBuilder(strUrl).Uri);
        }

        public Task<int> GetNumberOfAlbumsByGenre(int? genreId)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/albums/genre/{genreId ?? 0}/count";
            return _requestService.GetAsync<int>(new UriBuilder(strUrl).Uri);
        }

        public Task<ObservableCollection<Album>> GetAlbumsByGenre(int? genreId, int skip, int limit)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/albums/genre/{genreId ?? 0}/?skip={skip}&limit={limit}";
            return _requestService.GetAsync<ObservableCollection<Album>>(new UriBuilder(strUrl).Uri);
        }
        
        public Task<SystemInfo> GetSystemInfo()
        {
             string strUrl = $"{_settingsService.ServiceEndPoint}/api/system";
            return _requestService.GetAsync<SystemInfo>(new UriBuilder(strUrl).Uri);
        }

        public Task<Track> GetTrackById(int trackId)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/tracks/{trackId}";
            return _requestService.GetAsync<Track>(new UriBuilder(strUrl).Uri);
        }

        public Task<ObservableCollection<int>> GetTrackIdsByGenre(int? genreId = null)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/tracks/genre/{genreId ?? 0}";
            return _requestService.GetAsync<ObservableCollection<int>>(new UriBuilder(strUrl).Uri);
        }

        public Task<Track[]> GetTrackSearchResults(string query, int skip, int limit)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/search/tracks/search/?query={query}&skip={skip}&limit={limit}";
            return _requestService.GetAsync<Track[]>(new UriBuilder(strUrl).Uri);
        }

        public Task<bool> UpdateHistory(History history)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/tunes/UpdateHistory";
            return _requestService.PostAsync<bool, History>(new UriBuilder(strUrl).Uri, history);
        }

        public Task<Playlist> AppendToPlaylist(Playlist playlist)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/playlists/playlist/append";
            return _requestService.PutAsync<Playlist, Playlist>(new UriBuilder(strUrl).Uri, playlist);
        }

        public Task DeletePlaylist(int playlistId)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/playlists/{playlistId}";
            return _requestService.DeleteAsync(new UriBuilder(strUrl).Uri);
        }
        public Task<ObservableCollection<Playlist>> GetPlaylistsByUserName(string userName, int skip, int limit)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/playlists/{userName}/?skip={skip}&limit={limit}";
            return _requestService.GetAsync<ObservableCollection<Playlist>>(new UriBuilder(strUrl).Uri);
        }
        
        public Task<Playlist> GetPlaylistById(int playlistId, string userName)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/playlists/{userName}/{playlistId}";
            return _requestService.GetAsync<Playlist>(new UriBuilder(strUrl).Uri);
        }

        public Task<Playlist> GetPlaylistByIdWithNumberOfEntries(int playlistId, string userName)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/playlists/{userName}/{playlistId}/$count";
            return _requestService.GetAsync<Playlist>(new UriBuilder(strUrl).Uri);
        }

        public Task<ObservableCollection<Guid>> GetPlaylistImageIdsById(int playlistId, string userName, int limit)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/playlists/{userName}/{playlistId}/imageids/?limit={limit}";
            return _requestService.GetAsync<ObservableCollection<Guid>>(new UriBuilder(strUrl).Uri);
        }

        public Task<Playlist> InsertPlaylist(Playlist playlist)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/playlist/insert";
            return _requestService.PostAsync<Playlist, Playlist>(new UriBuilder(strUrl).Uri, playlist);
        }

        public Task<Playlist> UpdatePlaylist(Playlist playlist)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/playlists/playlist/update";
            return _requestService.PutAsync<Playlist, Playlist>(new UriBuilder(strUrl).Uri, playlist);
        }
    }
}
