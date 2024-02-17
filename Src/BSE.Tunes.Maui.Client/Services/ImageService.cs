using BSE.Tunes.Maui.Client.Events;
using Prism.Events;
using SkiaSharp;

namespace BSE.Tunes.Maui.Client.Services
{
    public class ImageService : IImageService
    {
        private const string ThumbnailPart = "_thumb";
        private const string ImageExtension = "img";
        private readonly IDataService _dataService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRequestService _requestService;
        private readonly IStorageService _storageService;

        public ImageService(
            IDataService dataService,
            IEventAggregator eventAggregator,
            IRequestService requestService,
            IStorageService storageService) {
            _dataService = dataService;
            _eventAggregator = eventAggregator;
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

            //Fire and forget
            Task.Run(() =>
            {
                //we create and save the image into the file system to use it next time.
                CreateAndSaveBitmap(absoluteUri, fileFullName, asThumbnail);
            }).ConfigureAwait(false);

            return absoluteUri;

        }

        private async void CreateAndSaveBitmap(string imageUri, string fileName, bool asThumbnail)
        {
            SKBitmap? bitmap = await CreateBitmapFromStream(imageUri);
            if (bitmap != null)
            {
                if (!asThumbnail)
                {
                    bitmap = bitmap.Resize(new SKImageInfo(300, 300), SKFilterQuality.Medium);
                }
                using (SKImage image = SKImage.FromBitmap(bitmap))
                {
                    using (SKData encoded = image.Encode(SKEncodedImageFormat.Jpeg, 100))
                    {
                        using (System.IO.Stream outFile = System.IO.File.OpenWrite(fileName))
                        {
                            encoded.SaveTo(outFile);
                        }
                    }
                    _eventAggregator.GetEvent<CacheChangedEvent>().Publish(CacheChangeMode.Added);
                }
            }
        }

        private async Task<SKBitmap?> CreateBitmapFromStream(string imageUri)
        {
            SKBitmap? bitmap = default;

            if (imageUri != null)
            {
                using (var httpClient = await _requestService.GetHttpClient())
                {
                    try
                    {
                        var stream = await httpClient.GetStreamAsync(imageUri);
                        if (stream != null)
                        {
                            //create a bitmap from the file and add it to the list
                            bitmap = SKBitmap.Decode(stream);
                        }
                    }
                    //if there´s no image
                    catch (Exception) { }
                }
            }

            return bitmap;
        }

        private Uri GetImageUrl(bool asThumbnail, Guid id)
        {
            return _dataService.GetImage(id, asThumbnail);
        }
    }
}
