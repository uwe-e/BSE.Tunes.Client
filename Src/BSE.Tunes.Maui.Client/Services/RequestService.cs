using System.Text;
using System.Text.Json;

namespace BSE.Tunes.Maui.Client.Services
{
    public class RequestService : IRequestService
    {
        private readonly IAuthenticationService _authenticationService;

        public RequestService(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<TResult?> GetAsync<TResult>(Uri uri)
        {
            TResult? result = default;
            using (var client = await GetHttpClient())
            {
                var responseMessage = await client.GetAsync(uri);
                //responseMessage.EnsureExtendedSuccessStatusCode();
                var serialized = await responseMessage.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<TResult>(serialized);
            }
            return result;
        }

        public async Task<T> GetAsync<T>(Uri uri, CancellationToken token)
        {
            T result = default;
            using (var client = await GetHttpClient())
            {
                var responseMessage = await client.GetAsync(uri, token);
                //responseMessage.EnsureExtendedSuccessStatusCode();
                var serialized = await responseMessage.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<T>(serialized);
                
            }
            return result;
        }

        public async Task<TResult> PostAsync<TResult, TRequest>(Uri uri, TRequest from)
        {
            TResult? result = default;
            using (var client = await GetHttpClient())
            {
                var serialized = await Task.Run(() => JsonSerializer.Serialize(from));
                using (var responseMessage = await client.PostAsync(uri,
                                                                    new StringContent(serialized, Encoding.UTF8, "application/json")))
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    result = JsonSerializer.Deserialize<TResult>(responseData);
                }
            }
            return result;
        }

        public async Task<TResult> PutAsync<TResult, TRequest>(Uri uri, TRequest from)
        {
            TResult? result = default;
            using (var client = await GetHttpClient())
            {
                var serialized = await Task.Run(() => JsonSerializer.Serialize(from));
                using (var responseMessage = await client.PutAsync(uri,
                                                                    new StringContent(serialized, Encoding.UTF8, "application/json")))
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    result = JsonSerializer.Deserialize<TResult>(responseData);
                }
            }
            return result;
        }

        public async Task DeleteAsync(Uri uri)
        {
            using (var client = await GetHttpClient())
            {
                var responseMessage = await client.DeleteAsync(uri);
            }
        }

        public async Task<HttpClient> GetHttpClient(bool withRefreshToken = true)
        {
            //if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            //{
            //    throw new ConnectivityException();
            //}
            var httpClient = new HttpClient();
            var tokenResponse = _authenticationService.TokenResponse;
            if (withRefreshToken && tokenResponse != null)
            {
                tokenResponse = await _authenticationService.RequestRefreshTokenAsync(tokenResponse.RefreshToken);
                httpClient.SetBearerToken(tokenResponse.AccessToken);
            }
            return httpClient;
        }
    }
}
