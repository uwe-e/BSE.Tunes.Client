using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSE.Tunes.Maui.Client.Services
{
    public class StorageService : IStorageService
    {
        private const string CacheFolderName = "cache";
        private const string ImageFolderName = "img";
        
        public Task DeleteCachedImagesAsync(string searchPattern = null)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public Task<long> GetUsedDiskSpaceAsync()
        {
            throw new NotImplementedException();
        }

        public bool TryToGetImagePath(string fileName, out string filePath)
        {
            var imageFolderPath = GetImageFolder();
            filePath = Path.Combine(imageFolderPath, fileName);
            return File.Exists(filePath);
        }
    }
}
