namespace BSE.Tunes.Maui.Client.Services
{
    public class StorageService : IStorageService
    {
        private const string ImageFolderName = "img";
        private const string AudioFolderName = "audio";
        private const long MaxCacheSizeBytes = 5L * 1024 * 1024 * 1024; // 5GB
        
        public async Task<long> GetUsedCacheSizeAsync()
        {
            var audioCacheTask = GetAudioCacheSizeAsync();
            var imageCacheTask = GetUsedImageCacheSizeAsync();

            await Task.WhenAll(audioCacheTask, imageCacheTask);

            return audioCacheTask.Result + imageCacheTask.Result;
        }
        
        public Task DeleteCachedImagesAsync(string searchPattern = null)
        {
            string imageFolderPath = GetImageDirectory();
            if (Directory.Exists(imageFolderPath))
            {
                var pattern = searchPattern ?? "*";
                foreach (var filePath in Directory.EnumerateFiles(imageFolderPath, pattern))
                {
                    File.Delete(filePath);
                }
            }
            return Task.CompletedTask;
        }

        public string GetImageDirectory()
        {
            var imageFolderPath = Path.Combine(FileSystem.CacheDirectory, ImageFolderName);
            Directory.CreateDirectory(imageFolderPath);
            return imageFolderPath;
        }

        public long GetUsedImageCacheSize()
        {
            long length = 0;
            string imageFolderPath = GetImageDirectory();
            if (Directory.Exists(imageFolderPath))
            {
                foreach (var filePath in Directory.EnumerateFiles(imageFolderPath))
                {
                    length += new FileInfo(filePath).Length;
                }
            }
            return length;
        }

        public Task<long> GetUsedImageCacheSizeAsync()
        {
            return Task.Run(GetUsedImageCacheSize);
        }

        public bool TryToGetImagePath(string fileName, out string filePath)
        {
            filePath = Path.Combine(GetImageDirectory(), fileName);
            return File.Exists(filePath);
        }

        public string GetAudioDirectory()
        {
            var audioFolderPath = Path.Combine(FileSystem.CacheDirectory, AudioFolderName);
            Directory.CreateDirectory(audioFolderPath);
            return audioFolderPath;
        }

        public async Task<long> GetAudioCacheSizeAsync()
        {
            return await Task.Run(() =>
            {
                var audioDirectory = GetAudioDirectory();
                if (!Directory.Exists(audioDirectory))
                    return 0L;

                long totalSize = 0;
                foreach (var filePath in Directory.EnumerateFiles(audioDirectory))
                {
                    if (!filePath.Contains(ImageFolderName))
                    {
                        totalSize += new FileInfo(filePath).Length;
                    }
                }
                return totalSize;
            });
        }

        public async Task DeleteCachedAudioFilesAsync()
        {
            await Task.Run(() =>
            {
                var audioDirectory = GetAudioDirectory();
                if (!Directory.Exists(audioDirectory))
                    return;

                foreach (var file in Directory.EnumerateFiles(audioDirectory))
                {
                    try
                    {
                        File.Delete(file);
                        Console.WriteLine($"Deleted audio cache file: {Path.GetFileName(file)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting audio cache file {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
            });
        }

        public async Task CleanupCacheAsync(long maxCacheSizeBytes = MaxCacheSizeBytes)
        {
            await Task.Run(() =>
            {
                string audioDirectory = GetAudioDirectory();
                
                if (!Directory.Exists(audioDirectory))
                    return;

                // Get files sorted by access time (oldest first)
                var filePaths = Directory.GetFiles(audioDirectory)
                    .Select(path => (Path: path, AccessTime: File.GetLastAccessTime(path)))
                    .OrderBy(f => f.AccessTime)
                    .Select(f => f.Path)
                    .ToList();

                long totalSize = 0;
                
                foreach (var filePath in filePaths)
                {
                    var fileInfo = new FileInfo(filePath);
                    if (!fileInfo.Exists) continue; // File deleted between enumeration and now
                    
                    totalSize += fileInfo.Length;
                    
                    if (totalSize <= maxCacheSizeBytes)
                        continue;

                    try
                    {
                        var fileSize = fileInfo.Length;
                        fileInfo.Delete();
                        totalSize -= fileSize;
                        Console.WriteLine($"Removed old cache file: {fileInfo.Name} ({fileSize / (1024.0 * 1024.0):F2} MB)");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error removing cache file {fileInfo.Name}: {ex.Message}");
                    }
                }

                Console.WriteLine($"Cache cleanup complete. New size: {totalSize / (1024.0 * 1024.0):F2} MB");
            });
        }

        public async Task UpdateFileAccessTimeAsync(string filePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.SetLastAccessTime(filePath, DateTime.Now);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating file access time: {ex.Message}.");
                }
            });
        }

        public async Task DeleteCacheAsync()
        {
            var deleteCachedImagesTask = DeleteCachedImagesAsync();
            var deleteCachedAudioFilesAsync = DeleteCachedAudioFilesAsync();

            await Task.WhenAll(deleteCachedImagesTask, deleteCachedAudioFilesAsync);
        }
    }
}
