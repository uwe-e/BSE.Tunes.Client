using Microsoft.Graphics.Canvas;

namespace BSE.Tunes.WinUI.Client.Services
{
    public class ImageService : IImageService
    {
        private const string ThumbnailPart = "_thumb";
        private const string ImageExtension = "img";

        // Limit concurrent downloads to reduce memory pressure
        private static readonly SemaphoreSlim DownloadSemaphore = new(4, 4);

        private readonly IDataService _dataService;
        private readonly IRequestService _requestService;
        private readonly IStorageService _storageService;

        public ImageService(
            IDataService dataService,
            IRequestService requestService,
            IStorageService storageService)
        {
            _dataService = dataService;
            _requestService = requestService;
            _storageService = storageService;
        }

        public string GetBitmapSource(Guid albumId, bool asThumbnail = false)
        {
            string fileName = asThumbnail ? $"{albumId}_{ThumbnailPart}" : $"{albumId}";
            fileName = $"{fileName}.{ImageExtension}";

            if (_storageService.TryToGetImagePath(fileName, out string fileFullName))
            {
                return fileFullName;
            }

            string absoluteUri = GetImageUrl(asThumbnail, albumId).AbsoluteUri;

            // Fire and forget - download and cache for next time
            _ = CreateAndSaveBitmapAsync(absoluteUri, fileFullName, asThumbnail);

            return absoluteUri;
        }

        public async Task<string> GetComposedBitmapSourceAsync(int playlistId, IList<string> albumCoverIds, int width = 300, bool asThumbnail = false)
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

            await CombineImages(albumIds, width, width, fullName, asThumbnail);

            return fullName;
        }

        private async Task CombineImages(IEnumerable<Guid> albumIds, int width, int height, string outputPath, bool asThumbnail = false)
        {
            var device = CanvasDevice.GetSharedDevice();

            using var renderTarget = new CanvasRenderTarget(device, width, height, 96);

            int innerWidth = width / 2;
            int innerHeight = height / 2;

            // Define quadrant positions
            var positions = new Windows.Foundation.Rect[]
            {
                new(0, 0, innerWidth, innerHeight),                    // Top-left
                new(innerWidth, innerHeight, innerWidth, innerHeight), // Bottom-right
                new(innerWidth, 0, innerWidth, innerHeight),           // Top-right
                new(0, innerHeight, innerWidth, innerHeight)           // Bottom-left
            };

            var bitmaps = new List<CanvasBitmap>();

            try
            {
                // Load images in parallel for better performance
                var loadTasks = new List<Task<CanvasBitmap>>(4);

                foreach (var id in albumIds.Take(4))
                {
                    string imageUri = GetImageUrl(asThumbnail, id).AbsoluteUri;
                    if (imageUri != null)
                    {
                        loadTasks.Add(LoadCanvasBitmapAsync(device, imageUri));
                    }
                }

                var loadedBitmaps = await Task.WhenAll(loadTasks);

                // Filter out nulls
                for (int i = 0; i < loadedBitmaps.Length; i++)
                {
                    if (loadedBitmaps[i] != null)
                    {
                        bitmaps.Add(loadedBitmaps[i]);
                    }
                }

                // Draw on the render target
                using (var drawingSession = renderTarget.CreateDrawingSession())
                {
                    drawingSession.Clear(Windows.UI.Color.FromArgb(0, 0, 0, 0));

                    if (bitmaps.Count == 1)
                    {
                        // Single image - fill entire canvas
                        drawingSession.DrawImage(bitmaps[0], new Windows.Foundation.Rect(0, 0, width, height));
                    }
                    else
                    {
                        // Multiple images - 2x2 grid
                        int count = Math.Min(bitmaps.Count, 4);
                        for (int i = 0; i < count; i++)
                        {
                            drawingSession.DrawImage(bitmaps[i], positions[i]);
                        }
                    }
                }

                // Save to file
                using var fileStream = File.Create(outputPath);
                await renderTarget.SaveAsync(fileStream.AsRandomAccessStream(), CanvasBitmapFileFormat.Png);
            }
            finally
            {
                // Clean up memory
                foreach (var bitmap in bitmaps)
                {
                    bitmap.Dispose();
                }
            }
        }

        private async Task<CanvasBitmap> LoadCanvasBitmapAsync(CanvasDevice device, string imageUri)
        {
            if (imageUri == null) return null;

            await DownloadSemaphore.WaitAsync();
            try
            {
                using var httpClient = await _requestService.GetHttpClientAsync();
                using var stream = await httpClient.GetStreamAsync(imageUri);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                return await CanvasBitmap.LoadAsync(device, memoryStream.AsRandomAccessStream());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load image: {ex.Message}");
                return null;
            }
            finally
            {
                DownloadSemaphore.Release();
            }
        }

        private async Task CreateAndSaveBitmapAsync(string imageUri, string fileName, bool asThumbnail)
        {
            var device = CanvasDevice.GetSharedDevice();
            CanvasBitmap originalBitmap = null;
            CanvasRenderTarget resizedBitmap = null;

            try
            {
                originalBitmap = await LoadCanvasBitmapAsync(device, imageUri);
                if (originalBitmap == null) return;

                if (!asThumbnail)
                {
                    // Resize to 300x300
                    resizedBitmap = new CanvasRenderTarget(device, 300, 300, 96);
                    using (var drawingSession = resizedBitmap.CreateDrawingSession())
                    {
                        drawingSession.DrawImage(originalBitmap, new Windows.Foundation.Rect(0, 0, 300, 300));
                    }

                    // Dispose original after resize
                    originalBitmap.Dispose();
                    originalBitmap = null;
                }
                else
                {
                    // Use original size for thumbnail
                    resizedBitmap = new CanvasRenderTarget(device, (float)originalBitmap.Size.Width, (float)originalBitmap.Size.Height, 96);
                    using (var drawingSession = resizedBitmap.CreateDrawingSession())
                    {
                        drawingSession.DrawImage(originalBitmap);
                    }
                }

                // Save as JPEG
                using var fileStream = File.Create(fileName);
                await resizedBitmap.SaveAsync(fileStream.AsRandomAccessStream(), CanvasBitmapFileFormat.Jpeg, 0.9f);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create and save bitmap: {ex.Message}");
            }
            finally
            {
                originalBitmap?.Dispose();
                resizedBitmap?.Dispose();
            }
        }

        private Uri GetImageUrl(bool asThumbnail, Guid id)
        {
            return _dataService.GetAlbumCoverUriById(id, asThumbnail);
        }

        public async Task RemoveComposedBitmaps(int playlistId)
        {
            string searchPattern = $"{playlistId}_*.png";
            await _storageService.DeleteCachedImagesAsync(searchPattern);
        }
    }
}