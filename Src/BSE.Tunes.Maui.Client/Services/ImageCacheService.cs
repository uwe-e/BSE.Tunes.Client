
namespace BSE.Tunes.Maui.Client.Services
{
    public class ImageCacheService : IImageCacheService
    {
        private readonly FFImageLoading.IImageService _imageLoadingService;
        private readonly IStorageService _storageService;

        public ImageCacheService(FFImageLoading.IImageService imageLoadingService,
            IStorageService storageService)
        {
            _imageLoadingService = imageLoadingService;
            _storageService = storageService;
        }

        public async Task InvalidateCacheEntryAsync(string searchPattern)
        {
            string imageFolder = _storageService.GetImageFolder();
            DirectoryInfo directoryInfo = new DirectoryInfo(imageFolder);
            if (directoryInfo.Exists)
            {
                foreach (var fileInfo in directoryInfo.GetFiles(searchPattern ?? "*"))
                {
                    await _imageLoadingService.InvalidateCacheEntryAsync(fileInfo.FullName, FFImageLoading.Cache.CacheType.All);
                }
            }
        }
    }
}
