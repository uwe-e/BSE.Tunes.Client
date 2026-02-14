using BSE.Tunes.Maui.Client.Events;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace BSE.Tunes.Maui.Client.Services
{
    public class ImageService(
        IDataService dataService,
        IEventAggregator eventAggregator,
        IRequestService requestService,
        ISettingsService settingsService,
        IStorageService storageService,
        IImageCacheService imageCacheService) : IImageService
    {
        private const string ThumbnailPart = "_thumb";
        private const string ImageExtension = "img";

        // Limit concurrent downloads to reduce memory pressure
        private static readonly SemaphoreSlim DownloadSemaphore = new(4, 4); // Max 4 concurrent

        private readonly IDataService _dataService = dataService;
        private readonly IEventAggregator _eventAggregator = eventAggregator;
        private readonly IRequestService _requestService = requestService;
        private readonly ISettingsService _settingsService = settingsService;
        private readonly IStorageService _storageService = storageService;
        private readonly IImageCacheService _imageCacheService = imageCacheService;

        public string GetBitmapSource(Guid albumId, bool asThumbnail = false)
        {
            string fileName = asThumbnail ? $"{albumId}_{ThumbnailPart}" : $"{albumId}";
            fileName = $"{fileName}.{ImageExtension}";

            if (_storageService.TryToGetImagePath(fileName, out string fileFullName))
            {
                return fileFullName;
            }

            string absoluteUri = GetImageUrl(asThumbnail, albumId).AbsoluteUri;

            //Fire and forget
            Task.Run(async() =>
            {
                //we create and save the image into the file system to use it next time.
                await CreateAndSaveBitmapAsync(absoluteUri, fileFullName, asThumbnail);
            }).ConfigureAwait(false);

            return absoluteUri;

        }
        
        public async Task<string> GetStitchedBitmapSourceAsync(int playlistId, IList<string> albumCoverIds, int width = 300, bool asThumbnail = false)
        {
            if (playlistId <= 0)
            {
                return null;
            }

            string fileName = asThumbnail ? $"{playlistId}{ThumbnailPart}" : $"{playlistId}";
            fileName += $"_{width}.png";
            
            if (_storageService.TryToGetImagePath(fileName, out string fullName))
            {
                return fullName;
            }

            // Parse GUIDs efficiently without LINQ/ObservableCollection overhead
            Guid[] albumIds = new Guid[albumCoverIds.Count];
            for (int i = 0; i < albumCoverIds.Count; i++)
            {
                albumIds[i] = Guid.Parse(albumCoverIds[i]);
            }

            SKImage stitchedImage = await Combine(albumIds, width, width, asThumbnail);

            using SKData encoded = stitchedImage.Encode(SKEncodedImageFormat.Png, 100);
            using System.IO.Stream outFile = System.IO.File.OpenWrite(fullName);
            encoded.SaveTo(outFile);

            return fullName;
        }

        public async Task<string> GetStitchedBitmapSourceAsync(int playlistId, int width = 300, bool asThumbnail = false)
        {
            if (playlistId > 0)
            {
                //string fileName = $"{playlistId}_{width}.{ImageExtension}";
                string fileName = $"{playlistId}_{width}.png";
                if (!_storageService.TryToGetImagePath(fileName, out string fullName))
                {
                    int height = width;

                    ObservableCollection<Guid> albumIds = await GetImageIds(playlistId);

                    SKImage stitchedImage = await Combine(albumIds, width, height, asThumbnail);

                    using SKData encoded = stitchedImage.Encode(SKEncodedImageFormat.Png, 100);
                    using System.IO.Stream outFile = System.IO.File.OpenWrite(fullName);
                    encoded.SaveTo(outFile);
                }
                return fullName;
            }
            return null;
        }

        private async Task<ObservableCollection<Guid>> GetImageIds(int playlistId)
        {
            return await _dataService.GetPlaylistImageIdsById(playlistId, _settingsService.User.UserName, 4);
        }

        private async Task<SKImage> Combine(IEnumerable<Guid> albumIds, int width, int height, bool asThumbnail = false)
        {
            // Pre-calculate dimensions
            int innerWidth = width / 2;
            int innerHeight = innerWidth;
            
            // Pre-create SKRect array to avoid Span across await boundary
            SKRect[] positions = new SKRect[4]
            {
                new SKRect(0, 0, innerWidth, innerHeight),                    // index 0
                new SKRect(innerWidth, innerHeight, width, height),           // index 1
                new SKRect(innerWidth, 0, width, innerHeight),                // index 2
                new SKRect(0, innerHeight, innerWidth, height)                // index 3
            };

            SKRect fullRect = new SKRect(0, 0, width, height);

            // Use array with known capacity to reduce allocations
            List<SKBitmap> images = new List<SKBitmap>(4);
            SKImage finalImage = null;

            try
            {
                // Download images in parallel for better performance
                var downloadTasks = new List<Task<SKBitmap>>(4);
                
                foreach (var id in albumIds)
                {
                    string imageUri = GetImageUrl(asThumbnail, id).AbsoluteUri;
                    if (imageUri != null)
                    {
                        downloadTasks.Add(CreateBitmapFromStream(imageUri));
                    }
                }

                var bitmaps = await Task.WhenAll(downloadTasks);
                
                // Filter out nulls
                for (int i = 0; i < bitmaps.Length; i++)
                {
                    if (bitmaps[i] != null)
                    {
                        images.Add(bitmaps[i]);
                    }
                }

                // Create surface and draw
                using (var tempSurface = SKSurface.Create(new SKImageInfo(width, height)))
                {
                    var canvas = tempSurface.Canvas;
                    canvas.Clear(SKColors.Transparent);

                    if (images.Count == 1)
                    {
                        canvas.DrawBitmap(images[0], fullRect);
                    }
                    else
                    {
                        int count = Math.Min(images.Count, 4);
                        for (int i = 0; i < count; i++)
                        {
                            canvas.DrawBitmap(images[i], positions[i]);
                        }
                    }
                    
                    finalImage = tempSurface.Snapshot();
                }

                return finalImage;
            }
            finally
            {
                // Clean up memory
                for (int i = 0; i < images.Count; i++)
                {
                    images[i].Dispose();
                }
            }
        }

        private async Task CreateAndSaveBitmapAsync(string imageUri, string fileName, bool asThumbnail)
        {
            SKBitmap originalBitmap = null;
            SKBitmap resizedBitmap = null;

            try
            {
                originalBitmap = await CreateBitmapFromStream(imageUri);
                if (originalBitmap == null) return;

                if (!asThumbnail)
                {
                    resizedBitmap = originalBitmap.Resize(
                        new SKImageInfo(300, 300),
                        new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));

                    // Dispose original after resize
                    originalBitmap.Dispose();
                    originalBitmap = null;
                }
                else
                {
                    resizedBitmap = originalBitmap;
                }

                using SKImage image = SKImage.FromBitmap(resizedBitmap);
                using SKData encoded = image.Encode(SKEncodedImageFormat.Jpeg, 90);
                using System.IO.Stream outFile = System.IO.File.OpenWrite(fileName);
                encoded.SaveTo(outFile);

                _eventAggregator.GetEvent<CacheChangedEvent>().Publish(CacheChangeMode.Added);
            }
            finally
            {
                originalBitmap?.Dispose();
                resizedBitmap?.Dispose();
            }
            //SKBitmap bitmap = await CreateBitmapFromStream(imageUri);
            //if (bitmap != null)
            //{
            //    if (!asThumbnail)
            //    {
            //        bitmap = bitmap.Resize(
            //            new SKImageInfo(300, 300),
            //            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            //    }
            //    using SKImage image = SKImage.FromBitmap(bitmap);
            //    using (SKData encoded = image.Encode(SKEncodedImageFormat.Jpeg, 90))
            //    {
            //        using System.IO.Stream outFile = System.IO.File.OpenWrite(fileName);
            //        encoded.SaveTo(outFile);
            //    }
            //    _eventAggregator.GetEvent<CacheChangedEvent>().Publish(CacheChangeMode.Added);
            //}
        }

        private async Task<SKBitmap> CreateBitmapFromStream(string imageUri)
        {
            if (imageUri == null) return null;

            await DownloadSemaphore.WaitAsync();
            try
            {
                using var httpClient = await _requestService.GetHttpClientAsync();
                using var stream = await httpClient.GetStreamAsync(imageUri);
                if (stream != null)
                {
                    // Decode on background thread to avoid UI blocking
                    return await Task.Run(() => SKBitmap.Decode(stream));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load image: {ex.Message}");
            }
            finally
            {
                DownloadSemaphore.Release();
            }

            return null;


            //SKBitmap bitmap = default;

            //if (imageUri != null)
            //{
            //    using var httpClient = await _requestService.GetHttpClientAsync();
            //    try
            //    {
            //        var stream = await httpClient.GetStreamAsync(imageUri);
            //        if (stream != null)
            //        {
            //            //create a bitmap from the file and add it to the list
            //            //bitmap = SKBitmap.Decode(stream);
            //            bitmap = await Task.Run(() =>
            //            {
            //                return SKBitmap.Decode(stream);
            //            });
            //        }
            //    }
            //    //if there´s no image
            //    catch (Exception ex)
            //    {
            //        var t = "";
            //    }
            //}

            //return bitmap;
        }

        private Uri GetImageUrl(bool asThumbnail, Guid id)
        {
            return _dataService.GetAlbumCoverUriById(id, asThumbnail);
        }

        public async Task RemoveStitchedBitmaps(int playlistId)
        {
            string searchPattern = $"{playlistId}_*.png";
            // should clear the ffimageloading cache when a playlist changed
            await _imageCacheService.InvalidateCacheEntryAsync(searchPattern);
            await _storageService.DeleteCachedImagesAsync(searchPattern);
        }

        
    }
}
