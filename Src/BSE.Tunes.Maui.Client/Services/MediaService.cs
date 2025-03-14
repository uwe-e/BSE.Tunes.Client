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
                    playerService?.RegisterAsMediaService(mediaElement);
                }
            }
        }

        public event Action<PlayerState> PlayerStateChanged;
        public event Action<MediaState> MediaStateChanged;

        private readonly IDataService _dataService;
        private readonly ISettingsService _settingsService;
        private readonly IRequestService _requestService;
        private readonly IStorageService _storageService;
        private MediaElement _mediaElement;

        private PlayerState _currentPlayerState;

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

        public void Disconnect()
        {
            // Stop and cleanup MediaElement when we navigate away
            _mediaElement?.Handler?.DisconnectHandler();
            /*
             * The attribute android:stopWithTask="true" in AndroidManifest.xml prevents the 
             * exception "Cannot access a disposed object" when trying to restart the closed app  
             */
        }

#nullable enable
        private async void OnMediaStateChanged(object? sender, MediaStateChangedEventArgs e)
        {
            await _mediaElement.Dispatcher.DispatchAsync(() =>
            {
                _currentPlayerState = e.NewState switch
                {
                    MediaElementState.Buffering => PlayerState.Buffering,
                    MediaElementState.Opening => PlayerState.Opening,
                    MediaElementState.Paused => PlayerState.Paused,
                    MediaElementState.Playing => PlayerState.Playing,
                    MediaElementState.Stopped => PlayerState.Stopped,
                    _ => PlayerState.Closed,
                };
                PlayerStateChanged?.Invoke(_currentPlayerState);
            });
        }

        private void OnMediaEnded(object? sender, EventArgs e)
        {
            MediaStateChanged?.Invoke(MediaState.Ended);
        }

        private void OnMediaOpened(object? sender, EventArgs e)
        {
            MediaStateChanged?.Invoke(MediaState.Opened);
        }

        public void Pause()
        {
            _mediaElement?.Pause();
        }

        public void Play()
        {
            _mediaElement?.Play();
        }

        public void Stop()
        {
            _mediaElement?.Stop();
        }

        public void SetTrack(Track track)
        {

        }

        public async Task SetTrackAsync(Track track, Uri coverUri)
        {
            if (track != null)
            {
                var requestUri = GetRequestUri(track.Guid);
                using var httpClient = await _requestService.GetHttpClient();
                using var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    var filePath = Path.Combine(FileSystem.CacheDirectory, track.Guid.ToString() + track.Extension);
                    using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await response.Content.CopyToAsync(fileStream);
                    _mediaElement.MetadataArtist = track.Album?.Artist?.Name ?? string.Empty;
                    _mediaElement.MetadataTitle = track.Name ?? string.Empty;
                    _mediaElement.MetadataArtworkUrl = coverUri?.ToString() ?? string.Empty;
                    _mediaElement.Source = MediaSource.FromFile(filePath);
                }
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
