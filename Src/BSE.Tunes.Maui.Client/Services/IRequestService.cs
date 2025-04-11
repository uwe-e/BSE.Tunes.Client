namespace BSE.Tunes.Maui.Client.Services
{
    public interface IRequestService
    {
        Task<T> GetAsync<T>(Uri uri);
        Task<T> GetAsync<T>(Uri uri, CancellationToken token);
        Task<U> PostAsync<U, T>(Uri uri, T from);
        Task<U> PutAsync<U, T>(Uri uri, T from);
        Task DeleteAsync(Uri uri);
        Task<HttpClient> GetHttpClient(bool withRefreshToken = true);
    }
}
