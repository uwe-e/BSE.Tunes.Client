using BSE.Tunes.Maui.Client.Models.Contract;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;

namespace BSE.Tunes.Maui.Client.Services
{
    public class MediaService : IMediaService
    {
        public static readonly BindableProperty RegisterAsMediaServiceProperty =
            BindableProperty.Create("RegisterAsMediaService",
                typeof(bool),
                typeof(MediaService),
                false,
                propertyChanged: RegisterAsMediaServicePropertyChanged);
        private readonly IDataService _dataService;
        private readonly ISettingsService _settingsService;
        private readonly IRequestService _requestService;
        private readonly IStorageService _storageService;

        public static bool GetRegisterAsMediaService(MediaElement target)
        {
            return (bool)target.GetValue(RegisterAsMediaServiceProperty);
        }

        public static void SetRegisterAsMediaService(MediaElement target, bool value)
        {
            target.SetValue(RegisterAsMediaServiceProperty, value);
        }

        private static void RegisterAsMediaServicePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MediaElement mediaElement)
            {
                bool toRegister = Convert.ToBoolean(newValue);
                if (toRegister)
                {

                    var playerService = Application.Current?.Handler.MauiContext?.Services.GetService<IMediaService>();
                    if (playerService != null)
                    {
                        playerService.RegisterAsMediaService(mediaElement);
                    }
                }
            }
        }

        private MediaElement _mediaElement;

        public MediaService(IDataService dataService,
            ISettingsService settingsService,
            IRequestService requestService,
            IStorageService storageService)
        {
            _dataService = dataService;
            _settingsService = settingsService;
            _requestService = requestService;
            _storageService = storageService;
        }

        public void RegisterAsMediaService(MediaElement mediaElement)
        {
            if (_mediaElement == null && mediaElement != null)
            {
                _mediaElement = mediaElement;
                _mediaElement.ShouldAutoPlay = true;
                _mediaElement.MediaOpened += OnMediaOpened;
                _mediaElement.MediaEnded += OnMediaEnded;
                _mediaElement.StateChanged += OnMediaStateChanged;
            }

        }

        private void OnMediaStateChanged(object? sender, MediaStateChangedEventArgs e)
        {
            var mediaElementState = e.NewState;
        }

        private void OnMediaEnded(object? sender, EventArgs e)
        {

        }

        private void OnMediaOpened(object? sender, EventArgs e)
        {

        }

        public void Stop()
        {
            _mediaElement?.Stop();
        }

        public void SetTrack(Track track)
        {

        }

        public async void SetTrack(Track track, Uri coverUri)
        {
            if (track != null)
            {
                var uri = _settingsService.ServiceEndPoint;
                var requestUri = GetRequestUri(track.Guid);
                //var requestUri = GetRequestUri(new Guid("3755174e-5381-44ef-8195-00676260b45a"));
                using (var httpClient = await _requestService.GetHttpClient())
                {
                    using (var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead))
                        if (response.IsSuccessStatusCode)
                        {
                            //var stream = await response.Content.ReadAsStreamAsync();


                            //_storageService.
                            var filePath = Path.Combine(FileSystem.CacheDirectory, track.Guid.ToString());
                            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await response.Content.CopyToAsync(fileStream);
                            }

                            _mediaElement.Source = MediaSource.FromFile(filePath);


                            //_mediaElement.Source = new Uri("http://test.com");
                        }
                }
                //_mediaElement.Source = track.
                //_dataService.
            }
        }

        private Uri GetRequestUri(Guid guid)
        {
            var builder = new UriBuilder(_settingsService.ServiceEndPoint);
            builder.Path = Path.Combine(builder.Path, $"/api/files/audio/{guid}");
            return builder.Uri;
        }
    }
}
