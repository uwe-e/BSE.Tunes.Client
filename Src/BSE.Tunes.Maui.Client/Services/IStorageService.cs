namespace BSE.Tunes.Maui.Client.Services
{
    public interface IStorageService
    {
        Task<long> GetUsedCacheSizeAsync();
        string GetImageDirectory();
        bool TryToGetImagePath(string fileName, out string filePath);
        Task<long> GetUsedImageCacheSizeAsync();
        long GetUsedImageCacheSize();
        Task DeleteCachedImagesAsync(string searchPattern = null);
        Task DeleteCacheAsync();
        string GetAudioDirectory();
        Task<long> GetAudioCacheSizeAsync();
        Task DeleteCachedAudioFilesAsync();
        Task CleanupCacheAsync(long maxCacheSizeBytes = 10L * 1024 * 1024 * 1024);
        Task UpdateFileAccessTimeAsync(string filePath);
    }
}
