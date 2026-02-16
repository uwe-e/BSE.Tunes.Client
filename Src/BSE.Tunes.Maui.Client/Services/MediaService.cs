using BSE.Tunes.Maui.Client.Models.Contract;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

namespace BSE.Tunes.Maui.Client.Services
{
    public class MediaService : IMediaService
    {
        public static readonly BindableProperty RegisterAsMediaServiceProperty =
            BindableProperty.Create(nameof(RegisterAsMediaService),
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
            if (bindable is MediaElement mediaElement && newValue is bool toRegister && toRegister)
            {
                var playerService = Application.Current?.Handler.MauiContext?.Services.GetService<IMediaService>();
                playerService?.RegisterAsMediaService(mediaElement);
            }
        }

        public event Action<PlayerState> PlayerStateChanged;
        public event Action<MediaState> MediaStateChanged;
        public event Action<CacheChangeMode> AudioCacheChanged;

        private readonly ISettingsService _settingsService;
        private readonly IRequestService _requestService;
        private readonly IStorageService _storageService;
        private readonly LocalProxyService _proxyService;
        private MediaElement _mediaElement;

        private PlayerState _currentPlayerState;
        private CancellationTokenSource _prefetchCancellation;

        public double Progress => GetProgress();

        private double GetProgress()
        {
            if (_mediaElement?.Duration != null && _mediaElement.Duration.TotalSeconds > 0)
            {
                return _mediaElement.Position.TotalSeconds / _mediaElement.Duration.TotalSeconds;
            }
            return 0.0;
        }

        public MediaService(IDataService dataService,
            ISettingsService settingsService,
            IRequestService requestService,
            IStorageService storageService,
            LocalProxyService proxyService)
        {
            _settingsService = settingsService;
            _requestService = requestService;
            _storageService = storageService;
            _proxyService = proxyService;

            // Start proxy when service is created
            _ = InitializeProxyAsync();
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
            // Stop and cleanup MediaElement when we close the app
            _mediaElement?.Handler?.DisconnectHandler();
            /*
             * The attribute android:stopWithTask="true" in AndroidManifest.xml prevents the 
             * exception "Cannot access a disposed object" when trying to restart the closed app  
             */
            _proxyService?.Dispose();
        }

        private async Task InitializeProxyAsync()
        {
            try
            {
                await _proxyService.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start proxy service: {ex.Message}");
            }
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

        public async Task SetTrackAsync(Track track, Uri coverUri)
        {
            if (track == null || track.Guid == Guid.Empty)
                return;

            var filePath = GetCachedTracksFilePath(track);//Path.Combine(FileSystem.CacheDirectory, track.Guid + track.Extension);

            if (File.Exists(filePath))
            {
                var requestUri = GetRequestUri(track.Guid);
                HttpClient httpClient = await _requestService.GetHttpClientAsync();

                if (await IsFileCompleteAsync(httpClient, requestUri, filePath))
                {
                    Console.WriteLine("Playing from cache");
                    await _storageService.UpdateFileAccessTimeAsync(filePath);
                    await SetMediaElementSourceAsync(track, coverUri, MediaSource.FromFile(filePath));
                    return;
                }
                else
                {
                    Console.WriteLine("Incomplete cache, deleting");
                    File.Delete(filePath);
                }
            }

            // Stream via local proxy with authentication
            var proxyUrl = _proxyService.GetProxyUrl(track.Guid);

            try
            {
                Console.WriteLine($"Streaming from proxy: {proxyUrl}");
                await SetMediaElementSourceAsync(track, coverUri, MediaSource.FromUri(proxyUrl));

                // Cache in background for future playback
                _ = CacheTrackInBackgroundAsync(track, filePath, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting media source: {ex.Message}");
            }
        }
        
        public async Task PrefetchNextTrackAsync(Track nextTrack)
        {
            if (nextTrack == null || nextTrack.Guid == Guid.Empty)
                return;

            // Cancel any existing prefetch operation
            CancelPrefetch();

            var filePath = GetCachedTracksFilePath(nextTrack); //Path.Combine(FileSystem.CacheDirectory, nextTrack.Guid + nextTrack.Extension);

            // Check if already cached
            if (File.Exists(filePath))
            {
                var requestUri = GetRequestUri(nextTrack.Guid);
                HttpClient httpClient = await _requestService.GetHttpClientAsync();

                if (await IsFileCompleteAsync(httpClient, requestUri, filePath))
                {
                    Console.WriteLine($"Next track already cached: {nextTrack.Name}");
                    return;
                }
                else
                {
                    File.Delete(filePath);
                }
            }

            // Start new prefetch with cancellation support
            _prefetchCancellation = new CancellationTokenSource();
            await CacheTrackInBackgroundAsync(nextTrack, filePath, _prefetchCancellation.Token, isPrefetch: true);
        }

        private void CancelPrefetch()
        {
            if (_prefetchCancellation != null && !_prefetchCancellation.IsCancellationRequested)
            {
                _prefetchCancellation.Cancel();
                _prefetchCancellation.Dispose();
                _prefetchCancellation = null;
            }
        }

        private async Task CacheTrackInBackgroundAsync(
            Track track, string filePath, CancellationToken cancellationToken, bool isPrefetch = false)
        {
            if (File.Exists(filePath))
                return;

            try
            {
                string operationType = isPrefetch ? "Prefetching" : "Background caching";
                Console.WriteLine($"{operationType}: {track.Name}");

                HttpClient httpClient = await _requestService.GetHttpClientAsync();
                var requestUri = GetRequestUri(track.Guid);

                Console.WriteLine($"Background caching: {track.Name}");

                using HttpResponseMessage response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"{operationType} failed: {response.StatusCode}");
                    return;
                }

                long? expectedLength = response.Content.Headers.ContentLength;

                using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

                // Copy with cancellation support
                await contentStream.CopyToAsync(fileStream, 81920, cancellationToken);

                // Verify complete download
                if (expectedLength.HasValue && fileStream.Length != expectedLength.Value)
                {
                    Console.WriteLine($"Incomplete {operationType.ToLower()}, deleting");
                    File.Delete(filePath);
                }
                else
                {
                    Console.WriteLine($"✓ {operationType} complete: {track.Name}");
                    
                    // Clean up cache if size exceeds limit
                    await _storageService.CleanupCacheAsync();
                }
                AudioCacheChanged?.Invoke(CacheChangeMode.AudioCacheAdded);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Prefetch canceled: {track.Name}");
                // Clean up partial file
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caching failed: {ex.Message}");
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        private async Task<bool> IsFileCompleteAsync(HttpClient httpClient, Uri requestUri, string filePath)
        {
            try
            {
                using var headRequest = new HttpRequestMessage(HttpMethod.Head, requestUri);
                using var headResponse = await httpClient.SendAsync(headRequest);

                if (!headResponse.IsSuccessStatusCode)
                    return false;

                long? expectedLength = headResponse.Content.Headers.ContentLength;
                if (!expectedLength.HasValue)
                    return true; // Can't verify, assume complete

                var fileInfo = new FileInfo(filePath);
                return fileInfo.Length == expectedLength.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying file: {ex.Message}");
                return false;
            }
        }

        private async Task SetMediaElementSourceAsync(Track track, Uri coverUri, MediaSource mediaSource)
        {
            try
            {
                await _mediaElement.Dispatcher.DispatchAsync(() =>
                {
                    _mediaElement.MetadataArtist = track.Album?.Artist?.Name ?? string.Empty;
                    _mediaElement.MetadataTitle = track.Name ?? string.Empty;
                    _mediaElement.MetadataArtworkUrl = coverUri?.ToString() ?? string.Empty;
                    _mediaElement.Source = mediaSource;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting media metadata: {ex.Message}");
                throw;
            }
        }

        private Uri GetRequestUri(Guid guid)
        {
            var builder = new UriBuilder(_settingsService.ServiceEndPoint);
            builder.Path = Path.Combine(builder.Path, $"/api/files/audio/{guid}");
            return builder.Uri;
        }

        private string GetCachedTracksFilePath(Track track)
        {
            return Path.Combine(_storageService.GetAudioDirectory() , track.Guid + track.Extension);
        }
        
    }
}
