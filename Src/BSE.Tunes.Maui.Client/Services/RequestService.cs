using BSE.Tunes.Maui.Client.Extensions;
using System.Text;
using System.Text.Json;

namespace BSE.Tunes.Maui.Client.Services
{
    public class RequestService(
        IAuthenticationService authenticationService,
        ISettingsService settingsService) : IRequestService
    {
        private static readonly JsonSerializerOptions _defaultJsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IAuthenticationService _authenticationService = authenticationService;
        private readonly ISettingsService _settingsService = settingsService;

        public async Task DeleteAsync(string path)
        {
            var builder = new UriBuilder(_settingsService.ServiceEndPoint);
            builder.AppendToPath(path);

            using (var client = await GetHttpClientAsync())
                await client.DeleteAsync(builder.Uri);

        }

        public async Task DeleteAsync(Uri uri)
        {
            using var client = await GetHttpClientAsync();
            var responseMessage = await client.DeleteAsync(uri);
        }

        public async Task<T> GetAsync<T>(string path)
        {
            var builder = new UriBuilder(this._settingsService.ServiceEndPoint);
            builder.AppendToPath(path);

            return await GetAsync<T>(builder.Uri);
        }

        public async Task<T> GetAsync<T>(string path, CancellationToken token)
        {
            var builder = new UriBuilder(this._settingsService.ServiceEndPoint);
            builder.AppendToPath(path);

            return await GetAsync<T>(builder.Uri, token);
        }

        public async Task<T> GetAsync<T>(string path, Dictionary<string, string> parameters)
        {
            var builder = new UriBuilder(this._settingsService.ServiceEndPoint);
            builder.AppendToPath(path);
            builder.AppendQueryParameters(parameters);

            return await GetAsync<T>(builder.Uri);
        }

        public async Task<T> GetAsync<T>(string path, Dictionary<string, string> parameters, CancellationToken token)
        {
            var builder = new UriBuilder(this._settingsService.ServiceEndPoint);
            builder.AppendToPath(path);
            builder.AppendQueryParameters(parameters);

            return await GetAsync<T>(builder.Uri, token);
        }

        public async Task<TResult> GetAsync<TResult>(Uri uri)
        {
            TResult result;
            using (var client = await GetHttpClientAsync())
            using (var responseMessage = await client.GetAsync(uri))
            {
                var stream = await responseMessage.Content.ReadAsStreamAsync();
                result = await JsonSerializer.DeserializeAsync<TResult>(stream, _defaultJsonOptions);
            }
            return result;
        }

        public async Task<T> GetAsync<T>(Uri uri, CancellationToken token)
        {
            T result;
            using (var client = await GetHttpClientAsync())
            using (var responseMessage = await client.GetAsync(uri, token))
            {
                var serialized = await responseMessage.Content.ReadAsStringAsync(token);
                result = JsonSerializer.Deserialize<T>(serialized, _defaultJsonOptions);
            }
            return result;
        }

        public async Task<T> PostAsync<T, TRequest>(Uri uri, TRequest from)
        {
            T result = default;
            using (var client = await GetHttpClientAsync())
            {
                var serialized = await Task.Run(() => JsonSerializer.Serialize(from, _defaultJsonOptions));
                using var responseMessage = await client.PostAsync(uri, new StringContent(serialized, Encoding.UTF8, "application/json"));
                var responseData = await responseMessage.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<T>(responseData, _defaultJsonOptions);
            }
            return result;
        }

        public async Task<TResult> PutAsync<TResult, TRequest>(Uri uri, TRequest from)
        {
            TResult result = default;
            using (var client = await GetHttpClientAsync())
            {
                var serialized = await Task.Run(() => JsonSerializer.Serialize(from, _defaultJsonOptions));
                using var responseMessage = await client.PutAsync(uri, new StringContent(serialized, Encoding.UTF8, "application/json"));
                var responseData = await responseMessage.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<TResult>(responseData, _defaultJsonOptions);
            }
            return result;
        }

        public async Task<HttpClient> GetHttpClientAsync(bool withRefreshToken = true)
        {
            var httpClient = new HttpClient();
            if (withRefreshToken)
            {
                var accessToken = await _authenticationService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    httpClient.SetBearerToken(accessToken);
                }
            }
            return httpClient;
        }
    }
}
