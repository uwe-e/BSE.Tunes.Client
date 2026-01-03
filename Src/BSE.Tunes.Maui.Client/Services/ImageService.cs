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
            //read all images into memory
            List<SKBitmap> images = [];
            SKImage finalImage = null;

            try
            {
                foreach (var id in albumIds)
                {
                    string imageUri = GetImageUrl(asThumbnail, id).AbsoluteUri;
                    if (imageUri != null)
                    {
                        var bitmap = await CreateBitmapFromStream(imageUri);
                        if (bitmap != null)
                        {
                            images.Add(bitmap);
                        }
                    }
                }

                //get a surface so we can draw an image
                using (var tempSurface = SKSurface.Create(new SKImageInfo(width, height)))
                {
                    //get the drawing canvas of the surface
                    var canvas = tempSurface.Canvas;

                    //set background color
                    canvas.Clear(SKColors.Transparent);

                    if (images.Count == 1)
                    {
                        canvas.DrawBitmap(images[0], SKRect.Create(0, 0, width, height));
                    }
                    else
                    {
                        var innerWidth = width / 2;
                        var innerHeight = innerWidth;
                        int index = 0;

                        foreach (SKBitmap image in images)
                        {
                            int x = 0;
                            int y = 0;

                            if (index == 1 || index == 2)
                            {
                                x += innerWidth;
                            }
                            if (index == 1 || index == 3)
                            {
                                y += innerHeight;
                            }

                            canvas.DrawBitmap(image, SKRect.Create(x, y, innerWidth, innerHeight));
                            index++;
                        }
                    }
                    // return the surface as a manageable image
                    finalImage = tempSurface.Snapshot();
                }

                //return the image that was just drawn
                return finalImage;
            }
            finally
            {
                //clean up memory
                foreach (SKBitmap image in images)
                {
                    image.Dispose();
                }
            }
        }

        private async Task CreateAndSaveBitmapAsync(string imageUri, string fileName, bool asThumbnail)
        {
            SKBitmap bitmap = await CreateBitmapFromStream(imageUri);
            if (bitmap != null)
            {
                if (!asThumbnail)
                {
                    bitmap = bitmap.Resize(new SKImageInfo(300, 300), SKFilterQuality.Medium);
                }
                using SKImage image = SKImage.FromBitmap(bitmap);
                using (SKData encoded = image.Encode(SKEncodedImageFormat.Jpeg, 90))
                {
                    using System.IO.Stream outFile = System.IO.File.OpenWrite(fileName);
                    encoded.SaveTo(outFile);
                }
                _eventAggregator.GetEvent<CacheChangedEvent>().Publish(CacheChangeMode.Added);
            }
        }

        private async Task<SKBitmap> CreateBitmapFromStream(string imageUri)
        {
            SKBitmap bitmap = default;

            if (imageUri != null)
            {
                using var httpClient = await _requestService.GetHttpClient();
                try
                {
                    var stream = await httpClient.GetStreamAsync(imageUri);
                    if (stream != null)
                    {
                        //create a bitmap from the file and add it to the list
                        //bitmap = SKBitmap.Decode(stream);
                        bitmap = await Task.Run(() =>
                        {
                            return SKBitmap.Decode(stream);
                        });
                    }
                }
                //if there´s no image
                catch (Exception ex)
                {
                    var t = "";
                }
            }

            return bitmap;
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
