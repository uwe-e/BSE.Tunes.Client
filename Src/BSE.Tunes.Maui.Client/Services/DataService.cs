using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services.Mappers;
using BSEtunes.Contracts.DTOs.Albums;
using BSEtunes.Contracts.DTOs.Common;
using BSEtunes.Contracts.DTOs.Playlists;
using BSEtunes.Contracts.Enums;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace BSE.Tunes.Maui.Client.Services
{
    public class DataService : IDataService
    {
        private readonly IRequestService _requestService;
        private readonly ISettingsService _settingsService;
        private readonly IMapper _mapper;

        public DataService(
            IRequestService requestService,
            ISettingsService settingsService,
            IMapper mapper)
        {
            _requestService = requestService;
            _settingsService = settingsService;
            _mapper = mapper;
        }

        public Task<bool> IsEndPointAccessibleAsync()
        {
            return IsEndPointAccessibleAsync(_settingsService.ServiceEndPoint);
        }

        public async Task<bool> IsEndPointAccessibleAsync(string serviceEndPoint)
        {
            var builder = new UriBuilder(serviceEndPoint);
            builder.AppendToPath("api/system/is-host-accessible");

            using var client = await _requestService.GetHttpClientAsync(false).ConfigureAwait(false);
            // CancellationTokenSource that will be canceled after the specified delay in seconds.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var response = await client.GetAsync(builder.Uri, cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Health check failed with status code {(int)response.StatusCode} ({response.StatusCode}).");
            }

            var serialized = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(serialized))
            {
                throw new FormatException("Health endpoint returned empty response; expected boolean \"true\".");
            }

            try
            {
                return JsonSerializer.Deserialize<bool>(serialized);
            }
            catch (JsonException jsonEx)
            {
                throw new FormatException("Health endpoint returned invalid response; expected boolean \"true\".", jsonEx);
            }
        }

        public Task<ObservableCollection<Album>> GetAlbumsByArtist(int artistId, int skip = 0, int limit = 10)
        {
            string strUrl = $"{this._settingsService.ServiceEndPoint}/api/v2/artists/{artistId}/albums?skip={skip}&limit={limit}";
            return _requestService.GetAsync<ObservableCollection<Album>>(new UriBuilder(strUrl).Uri);
        }

        public async Task<IList<Album>> GetFeaturedAlbums(int limit)
        {
            var parameters = new Dictionary<string, string> {
                { "sortBy", AlbumSortOption.Random.ToString() },
                { "limit", limit.ToString() }
            };

            var dtoResult = await _requestService.GetAsync<List<AlbumDto>>("api/albums", parameters);
            return _mapper.MapCollection<Album>(dtoResult).ToList();
        }

        public async Task<IList<Album>> GetNewestAlbums(int limit)
        {
            var parameters = new Dictionary<string, string> {
                { "sortBy", AlbumSortOption.NewestDesc.ToString()  },
                { "limit", limit.ToString() }
            };

            var dtoResult = await _requestService.GetAsync<IList<AlbumDto>>("api/albums", parameters);
            return _mapper.MapCollection<Album>(dtoResult).ToList();
        }

        public Uri GetImage(Guid imageId, bool asThumbnail = false)
        {
            var builder = new UriBuilder(_settingsService.ServiceEndPoint);
            builder.AppendToPath(string.Format("/api/files/image/{0}", imageId.ToString()));
            if (asThumbnail)
            {
                builder.AppendToPath($"{asThumbnail}");
            }
            return builder.Uri;
        }

        public async Task<Album> GetAlbumById(int albumId)
        {
            var dtoResult = await _requestService.GetAsync<AlbumDto>($"api/albums/{albumId}");
            return _mapper.Map<Album>(dtoResult)!;
        }
        
        public Task<Album[]> GetAlbumSearchResults(string query, int skip, int limit)
        {
            query = System.Web.HttpUtility.UrlEncode(query);
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/search/albums/search/?query={query}&skip={skip}&limit={limit}";
            return _requestService.GetAsync<Album[]>(new UriBuilder(strUrl).Uri);
        }

        public Task<Album[]> GetAlbumSearchResults(string query, int skip, int limit, CancellationToken token)
        {
            query = System.Web.HttpUtility.UrlEncode(query);
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/search/albums/search/?query={query}&skip={skip}&limit={limit}";
            return _requestService.GetAsync<Album[]>(new UriBuilder(strUrl).Uri, token);
        }
        
        public Uri GetAlbumCoverUriById(Guid albumId, bool asThumbnail = false)
        {
            var builder = new UriBuilder(_settingsService.ServiceEndPoint);
            builder.AppendToPath(string.Format($"api/albums/{albumId}/cover/"));
            if (asThumbnail)
            {
                builder.AppendToPath($"{asThumbnail}");
            }
            return builder.Uri;
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
        
        public async Task<SystemInfo> GetAvailableTrackCount()
        {
            var trackCount = await _requestService.GetAsync<int>($"api/tracks/count");
            return new SystemInfo
            {
                NumberTracks = trackCount
            };
        }
        public async Task<PagedResult<Album>> GetPagedAlbums(
            string genre,
            int? artistId,
            string artistName,
            int? yearFrom,
            int? yearTo,
            int pageNumber,
            int pageSize,
            AlbumSortOption albumSortOption = AlbumSortOption.Title)
        {
            // Use Dictionary<string, string> for parameters as per IRequestService signature
            var parameters = new Dictionary<string, string>(3)
            {
                ["sortBy"] = albumSortOption.ToString(),
                ["pageNumber"] = pageNumber.ToString(),
                ["pageSize"] = pageSize.ToString()
            };

            if (!string.IsNullOrWhiteSpace(genre))
            {
                parameters["genre"] = genre;
            }
            if (artistId.HasValue)
            {
                parameters["artistId"] = artistId.Value.ToString();
            }
            if (!string.IsNullOrWhiteSpace(artistName))
            {
                parameters["artistName"] = artistName;
            }
            if (yearFrom.HasValue)
            {
                parameters["yearFrom"] = yearFrom.Value.ToString();
            }
            if (yearTo.HasValue)
            {
                parameters["yearTo"] = yearTo.Value.ToString();
            }

            var dtoResult = await _requestService.GetAsync<PagedResultDto<AlbumDto>>("api/albums/paged", parameters);
            return new PagedResult<Album>
            {
                Items = _mapper.MapCollection<Album>(dtoResult.Items).ToList() ?? new List<Album>(),
                TotalCount = dtoResult.TotalCount,
                PageNumber = dtoResult.PageNumber,
                PageSize = dtoResult.PageSize,
                TotalPages = dtoResult.TotalPages,
                HasPreviousPage = dtoResult.HasPreviousPage,
                HasNextPage = dtoResult.HasNextPage,
            };
        }

        public Task<Track[]> GetTracksByAlbumId(int albumId)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/albums/{albumId}/tracks";
            return _requestService.GetAsync<Track[]>(new UriBuilder(strUrl).Uri);
        }
        public async Task<Track> GetTrackById(int trackId)
        {
            var dtoResult = await _requestService.GetAsync<TrackDto>($"api/tracks/{trackId}");
            return _mapper.Map<Track>(dtoResult);
        }

        public Task<IList<int>> GetTrackIdsByGenre(int? genreId = null)
        {
            return _requestService.GetAsync<IList<int>>($"api/tracks/genre/{genreId}");
        }

        public Task<IList<int>> GetTrackIdsByPlaylistId(int playlistId, bool randomize = false)
        {
            var parameters = new Dictionary<string, string>(1)
            {
                ["randomize"] = randomize.ToString(),
            };

            return _requestService.GetAsync<IList<int>>($"api/playlists/{playlistId}/trackids", parameters);
        }

        public Task<Track[]> GetTrackSearchResults(string query, int skip, int limit)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/search/tracks/search/?query={query}&skip={skip}&limit={limit}";
            return _requestService.GetAsync<Track[]>(new UriBuilder(strUrl).Uri);
        }

        public Task<Track[]> GetTrackSearchResults(string query, int skip, int limit, CancellationToken token)
        {
            string strUrl = $"{_settingsService.ServiceEndPoint}/api/search/tracks/search/?query={query}&skip={skip}&limit={limit}";
            return _requestService.GetAsync<Track[]>(new UriBuilder(strUrl).Uri, token);
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

        public async Task<PagedResult<Playlist>> GetPagedPlaylistsByOwnerAsync(int pageNumber, int pageSize)
        {
            var parameters = new Dictionary<string, string>(2) {
                { "pageNumber", pageNumber.ToString() },
                { "pageSize", pageSize.ToString() }
            };
            var dtoResult = await _requestService.GetAsync<PagedResultDto<PlaylistDto>>("api/playlists/paged", parameters);
            
            return new PagedResult<Playlist>
            {
                Items = _mapper.MapCollection<Playlist>(dtoResult.Items).ToList() ?? new List<Playlist>(),
                TotalCount = dtoResult.TotalCount,
                PageNumber = dtoResult.PageNumber,
                PageSize = dtoResult.PageSize,
                TotalPages = dtoResult.TotalPages,
                HasPreviousPage = dtoResult.HasPreviousPage,
                HasNextPage = dtoResult.HasNextPage,
            };
        }
        //public Task<ObservableCollection<Playlist>> GetPlaylistsByUserName(string userName, int skip, int limit)
        //{
        //    string strUrl = $"{_settingsService.ServiceEndPoint}/api/v2/playlists/{userName}/?skip={skip}&limit={limit}";
        //    return _requestService.GetAsync<ObservableCollection<Playlist>>(new UriBuilder(strUrl).Uri);
        //}
        public async Task<PagedResult<PlaylistEntry>> GetPagedPlaylistEntriesByIdAsync(int playlistId, int pageNumber, int pageSize)
        {
            var parameters = new Dictionary<string, string>(2) {
                { "pageNumber", pageNumber.ToString() },
                { "pageSize", pageSize.ToString() }
            };

            var dtoResult = await _requestService.GetAsync<PagedResultDto<PlaylistEntryDto>>($"api/playlists/{playlistId}/entries", parameters);
            return new PagedResult<PlaylistEntry>
            {
                Items = _mapper.MapCollection<PlaylistEntry>(dtoResult.Items).ToList() ?? new List<PlaylistEntry>(),
                TotalCount = dtoResult.TotalCount,
                PageNumber = dtoResult.PageNumber,
                PageSize = dtoResult.PageSize,
                TotalPages = dtoResult.TotalPages,
                HasPreviousPage = dtoResult.HasPreviousPage,
                HasNextPage = dtoResult.HasNextPage,
            };
        }

        public async Task<Playlist> GetPlaylistById(int playlistId, string userName)
        {
            var dtoResult = await _requestService.GetAsync<PlaylistDto>($"api/playlists/{playlistId}");
            return _mapper.Map<Playlist>(dtoResult)!;
        }

        [Obsolete("Use GetPlaylistById instead.")]
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
