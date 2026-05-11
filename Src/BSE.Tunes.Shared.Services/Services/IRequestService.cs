namespace BSE.Tunes.Shared.Services
{
    public interface IRequestService
    {
        Task DeleteAsync(string path);
        Task DeleteAsync<T>(string path, T content);
        Task<U?> DeleteAsync<U, T>(string path, T content);
        Task<U?> DeleteAsync<U, T>(Uri uri, T content);
        Task<T?> GetAsync<T>(string path);
        Task<T?> GetAsync<T>(string path, CancellationToken token);
        Task<T?> GetAsync<T>(string path, Dictionary<string, string> parameters);
        Task<T?> GetAsync<T>(string path, Dictionary<string, string> parameters, CancellationToken token);
        Task<T?> GetAsync<T>(Uri uri);
        Task<T?> GetAsync<T>(Uri uri, CancellationToken token);
        Task<HttpClient> GetHttpClientAsync(bool withRefreshToken = true);
        Task PostAsync<T>(string path, T content);
        Task<U?> PostAsync<U, T>(string path, T content);
        Task<U?> PostAsync<U, T>(Uri uri, T content);
    }
}
