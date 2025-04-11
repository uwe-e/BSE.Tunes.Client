namespace BSE.Tunes.Maui.Client.Services
{
    public interface IImageCacheService
    {
        Task InvalidateCacheEntryAsync(string searchPattern);
    }
}
