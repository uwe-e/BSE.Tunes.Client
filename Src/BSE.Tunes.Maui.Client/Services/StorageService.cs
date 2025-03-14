namespace BSE.Tunes.Maui.Client.Services
{
    public class StorageService : IStorageService
    {
        private const string CacheFolderName = "cache";
        private const string ImageFolderName = "img";
        
        public Task DeleteCachedImagesAsync(string searchPattern = null)
        {
            string imageFolderPath = GetImageFolder();
            DirectoryInfo directoryInfo = new(imageFolderPath);
            if (directoryInfo.Exists)
            {
                //var files = directoryInfo.GetFiles(searchPattern ?? "*");
                foreach (var fileInfo in directoryInfo.GetFiles(searchPattern ?? "*"))
                {
                    fileInfo.Delete();
                }
            }
            return Task.CompletedTask;
        }

        public string GetImageFolder()
        {
            var cacheFolder = FileSystem.CacheDirectory;
            var imageFolderPath = Path.Combine(cacheFolder, ImageFolderName);
            if (!Directory.Exists(imageFolderPath))
            {
                Directory.CreateDirectory(imageFolderPath);
            }
            return imageFolderPath;
        }

        public long GetUsedDiskSpace()
        {
            long length = default;
            string imageFolderPath = GetImageFolder();
            DirectoryInfo directoryInfo = new(imageFolderPath);
            if (directoryInfo.Exists)
            {
                foreach (var fileInfo in directoryInfo.GetFiles())
                {
                    length += fileInfo.Length;
                }
            }
            return length;
        }

        public Task<long> GetUsedDiskSpaceAsync()
        {
            return Task.Run(() =>
            {
                return GetUsedDiskSpace();
            });
        }

        public bool TryToGetImagePath(string fileName, out string filePath)
        {
            var imageFolderPath = GetImageFolder();
            filePath = Path.Combine(imageFolderPath, fileName);
            return File.Exists(filePath);
        }
    }
}
