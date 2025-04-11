namespace BSE.Tunes.Maui.Client.Services
{
    public interface IStorageService
    {
        string GetImageFolder();

        bool TryToGetImagePath(string fileName, out string filePath);

        Task<long> GetUsedDiskSpaceAsync();

        long GetUsedDiskSpace();

        Task DeleteCachedImagesAsync(string searchPattern = null);
    }
}
