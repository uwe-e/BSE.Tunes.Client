using BSE.Tunes.Shared.Services.Extensions;
using System.Text;
using System.Text.Json;

namespace BSE.Tunes.Shared.Services
{
    public class RequestService(
        IAuthenticationService authenticationService,
        Abstractions.ISettingsService settingsService) : IRequestService
    {
        private static readonly JsonSerializerOptions _defaultJsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IAuthenticationService _authenticationService = authenticationService;
        private readonly Abstractions.ISettingsService _settingsService = settingsService;

        public async Task DeleteAsync(string path)
        {
            Uri uri = BuildUri(path);
            using var client = await GetHttpClientAsync();
            await client.DeleteAsync(uri);
        }

        public async Task DeleteAsync<T>(string path, T content)
        {
            Uri uri = BuildUri(path);
            using var client = await GetHttpClientAsync();
            var serialized = JsonSerializer.Serialize(content, _defaultJsonOptions);
            var request = new HttpRequestMessage(HttpMethod.Delete, uri)
            {
                Content = new StringContent(serialized, Encoding.UTF8, "application/json")
            };
            var res = await client.SendAsync(request);
        }

        public async Task<U?> DeleteAsync<U, T>(string path, T content)
        {
            Uri uri = BuildUri(path);
            return await DeleteAsync<U, T>(uri, content);
        }

        public async Task<U?> DeleteAsync<U, T>(Uri uri, T content)
        {
            U? result;
            using (var client = await GetHttpClientAsync())
            {
                var serialized = JsonSerializer.Serialize(content, _defaultJsonOptions);
                var request = new HttpRequestMessage(HttpMethod.Delete, uri)
                {
                    Content = new StringContent(serialized, Encoding.UTF8, "application/json")
                };
                using var responseMessage = await client.SendAsync(request);

                if (responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return default;
                }

                using var stream = await responseMessage.Content.ReadAsStreamAsync();
                result = await JsonSerializer.DeserializeAsync<U>(stream, _defaultJsonOptions);
            }
            return result;
        }

        public async Task<T?> GetAsync<T>(string path)
        {
            Uri uri = BuildUri(path);
            return await GetAsync<T>(uri);
        }

        public async Task<T?> GetAsync<T>(string path, CancellationToken token)
        {
            Uri uri = BuildUri(path);
            return await GetAsync<T>(uri, token);
        }

        public async Task<T?> GetAsync<T>(string path, Dictionary<string, string> parameters)
        {
            Uri uri = BuildUri(path, parameters);
            return await GetAsync<T>(uri);
        }

        public async Task<T?> GetAsync<T>(string path, Dictionary<string, string> parameters, CancellationToken token)
        {
            Uri uri = BuildUri(path, parameters);
            return await GetAsync<T>(uri, token);
        }

        public async Task<T?> GetAsync<T>(Uri uri)
        {
            T? result;
            using (var client = await GetHttpClientAsync())
            using (var responseMessage = await client.GetAsync(uri))
            {
                var stream = await responseMessage.Content.ReadAsStreamAsync();
                result = await JsonSerializer.DeserializeAsync<T>(stream, _defaultJsonOptions);
            }
            return result;
        }

        public async Task<T?> GetAsync<T>(Uri uri, CancellationToken token)
        {
            T? result;
            using (var client = await GetHttpClientAsync())
            using (var responseMessage = await client.GetAsync(uri, token))
            {
                var serialized = await responseMessage.Content.ReadAsStringAsync(token);
                result = JsonSerializer.Deserialize<T>(serialized, _defaultJsonOptions);
            }
            return result;
        }
        
        public async Task PostAsync<T>(string path, T content)
        {
            Uri uri = BuildUri(path);
            await PostAsync<T, T>(uri, content);
        }

        public async Task<U?> PostAsync<U, T>(string path, T content)
        {
            Uri uri = BuildUri(path);
            return await PostAsync<U, T>(uri, content);
        }

        public async Task<U?> PostAsync<U, T>(Uri uri, T content)
        {
            U? result;
            using (var client = await GetHttpClientAsync())
            {
                var serialized = JsonSerializer.Serialize(content, _defaultJsonOptions);
                using var responseMessage = await client.PostAsync(uri, new StringContent(serialized, Encoding.UTF8, "application/json"));

                if (responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return default;
                }

                using var stream = await responseMessage.Content.ReadAsStreamAsync();
                result = await JsonSerializer.DeserializeAsync<U>(stream, _defaultJsonOptions);
            }
            return result;
        }

        public async Task PutAsync<T>(string path, T content)
        {
            Uri uri = BuildUri(path);
            await PutAsync<T, T>(uri, content);
        }

        public async Task<U?> PutAsync<U, T>(string path, T content)
        {
            Uri uri = BuildUri(path);
            return await PostAsync<U, T>(uri, content);
        }

        public async Task<U?> PutAsync<U, T>(Uri uri, T content)
        {
            U? result;
            using (var client = await GetHttpClientAsync())
            {
                var serialized = JsonSerializer.Serialize(content, _defaultJsonOptions);
                using var responseMessage = await client.PutAsync(uri, new StringContent(serialized, Encoding.UTF8, "application/json"));

                if (responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return default;
                }

                using var stream = await responseMessage.Content.ReadAsStreamAsync();
                result = await JsonSerializer.DeserializeAsync<U>(stream, _defaultJsonOptions);
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

        private Uri BuildUri(string path, Dictionary<string, string>? parameters = null)
        {
            UriBuilder builder = new UriBuilder(_settingsService.ServiceEndPoint);
            builder.AppendToPath(path);
            if (parameters != null)
            {
                builder.AppendQueryParameters(parameters);
            }
            return builder.Uri;
        }
    }
}
