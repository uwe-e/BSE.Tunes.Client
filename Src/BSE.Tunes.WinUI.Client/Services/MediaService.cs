using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace BSE.Tunes.WinUI.Client.Services
{
    public class MediaService : IMediaService
    {
        // Static Members
        public static readonly DependencyProperty RegisterAsMediaServiceProperty =
            DependencyProperty.RegisterAttached(
                "RegisterAsMediaService",
                typeof(bool),
                typeof(MediaService),
                new PropertyMetadata(false, OnRegisterAsMediaServiceChanged));

        // Fields
        private readonly ISettingsService _settingsService;
        private readonly IRequestService _requestService;
        private readonly IStorageService _storageService;
        private readonly LocalProxyService _localProxyService;
        private MediaPlayerElement _mediaPlayerElement;
        private MediaPlayer _mediaPlayer;
        private PlayerState _currentPlayerState;
        private CancellationTokenSource _prefetchCancellation;

        // Properties
        public double Progress => GetProgress();

        private double GetProgress()
        {
            var session = _mediaPlayer?.PlaybackSession;
            if (session == null)
                return 0.0;

            double totalSeconds = session.NaturalDuration.TotalSeconds;
            return totalSeconds > 0 ? session.Position.TotalSeconds / totalSeconds : 0.0;
        }

        // Events
        public event Action<PlayerState> PlayerStateChanged;
        public event Action<MediaState> MediaStateChanged;
        public event Action<CacheChangeMode> AudioCacheChanged;

        // Constructor
        public MediaService(
            ISettingsService settingsService,
            IRequestService requestService,
            IStorageService storageService,
            LocalProxyService localProxyService)
        {
            _settingsService = settingsService;
            _requestService = requestService;
            _storageService = storageService;
            _localProxyService = localProxyService;

            _ = InitializeProxyAsync();
        }

        // Static Methods
        public static bool GetRegisterAsMediaService(DependencyObject obj)
        {
            return (bool)obj.GetValue(RegisterAsMediaServiceProperty);
        }

        public static void SetRegisterAsMediaService(DependencyObject obj, bool value)
        {
            obj.SetValue(RegisterAsMediaServiceProperty, value);
        }

        private static void OnRegisterAsMediaServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MediaPlayerElement mediaPlayerElement && e.NewValue is true)
            {
                var mediaService = App.GetService<IMediaService>() as MediaService;
                mediaService?.RegisterAsMediaService(mediaPlayerElement);
            }
        }

        // Public Methods
        public void RegisterAsMediaService(MediaPlayerElement mediaPlayerElement)
        {
            if (_mediaPlayerElement == null && mediaPlayerElement != null)
            {
                _mediaPlayerElement = mediaPlayerElement;

                _mediaPlayer = new MediaPlayer
                {
                    AutoPlay = true,
                    AudioCategory = MediaPlayerAudioCategory.Media
                };

                _mediaPlayerElement.SetMediaPlayer(_mediaPlayer);

                _mediaPlayer.MediaOpened += OnMediaOpened;
                _mediaPlayer.MediaEnded += OnMediaEnded;
                _mediaPlayer.CurrentStateChanged += OnMediaStateChanged;
                _mediaPlayer.MediaFailed += OnMediaFailed;
            }
        }

        public void Disconnect()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.MediaOpened -= OnMediaOpened;
                _mediaPlayer.MediaEnded -= OnMediaEnded;
                _mediaPlayer.CurrentStateChanged -= OnMediaStateChanged;
                _mediaPlayer.MediaFailed -= OnMediaFailed;

                _mediaPlayer.Dispose();
                _mediaPlayer = null;
            }
            if (_mediaPlayerElement != null)
            {
                _mediaPlayerElement.SetMediaPlayer(null);
                _mediaPlayerElement = null;
            }
        }

        public void Play()
        {
            _mediaPlayer?.Play();
        }

        public void Pause()
        {
            _mediaPlayer?.Pause();
        }

        public void Stop()
        {
            var session = _mediaPlayer?.PlaybackSession;
            if (session != null)
            {
                _mediaPlayer.Pause();
                session.Position = TimeSpan.Zero;
            }
        }

        public async Task SetTrackAsync(Track track, Uri coverUri)
        {
            if (track?.Guid == Guid.Empty)
                return;

            string filePath = GetCachedTracksFilePath(track);

            if (File.Exists(filePath))
            {
                Uri requestUri = GetRequestUri(track.Guid);
                HttpClient httpClient = await _requestService.GetHttpClientAsync().ConfigureAwait(false);

                if (await IsFileCompleteAsync(httpClient, requestUri, filePath).ConfigureAwait(false))
                {
                    Console.WriteLine("Playing from cache");
                    await _storageService.UpdateFileAccessTimeAsync(filePath).ConfigureAwait(false);
                    await SetMediaSourceAsync(track, coverUri, MediaSource.CreateFromUri(new Uri(filePath))).ConfigureAwait(false);
                    return;
                }

                Console.WriteLine("Incomplete cache, deleting");
                File.Delete(filePath);
            }

            string proxyUrl = _localProxyService.GetProxyUrl(track.Guid);

            try
            {
                Console.WriteLine($"Streaming from proxy: {proxyUrl}");
                await SetMediaSourceAsync(track, coverUri, MediaSource.CreateFromUri(new Uri(proxyUrl))).ConfigureAwait(false);

                _ = CacheTrackInBackgroundAsync(track, filePath, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting media source: {ex.Message}");
            }
        }

        public async Task PrefetchNextTrackAsync(Track nextTrack)
        {
            if (nextTrack?.Guid == Guid.Empty)
                return;

            CancelPrefetch();

            string filePath = GetCachedTracksFilePath(nextTrack);

            if (File.Exists(filePath))
            {
                Uri requestUri = GetRequestUri(nextTrack.Guid);
                HttpClient httpClient = await _requestService.GetHttpClientAsync().ConfigureAwait(false);

                if (await IsFileCompleteAsync(httpClient, requestUri, filePath).ConfigureAwait(false))
                {
                    Console.WriteLine($"Next track already cached: {nextTrack.Name}");
                    return;
                }

                File.Delete(filePath);
            }

            _prefetchCancellation = new CancellationTokenSource();
            await CacheTrackInBackgroundAsync(nextTrack, filePath, _prefetchCancellation.Token, isPrefetch: true).ConfigureAwait(false);
        }

        // Private Methods
        private async Task InitializeProxyAsync()
        {
            try
            {
                await _localProxyService.StartAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start proxy service: {ex.Message}");
            }
        }

        private Task SetMediaSourceAsync(Track track, Uri coverUri, MediaSource mediaSource)
        {
            var dispatcher = _mediaPlayerElement.DispatcherQueue;
            
            dispatcher.TryEnqueue(() =>
            {
                var playbackItem = new MediaPlaybackItem(mediaSource);

                var displayProperties = playbackItem.GetDisplayProperties();
                displayProperties.Type = Windows.Media.MediaPlaybackType.Music;
                displayProperties.MusicProperties.Title = track.Name ?? string.Empty;
                displayProperties.MusicProperties.Artist = track.Album?.Artist?.Name ?? string.Empty;
                displayProperties.MusicProperties.AlbumTitle = track.Album?.Title ?? string.Empty;

                if (coverUri != null)
                {
                    displayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(coverUri);
                }

                playbackItem.ApplyDisplayProperties(displayProperties);

                _mediaPlayer.Source = playbackItem;
                _mediaPlayer.Play();
            });

            return Task.CompletedTask;
        }

        private async Task CacheTrackInBackgroundAsync(Track track, string filePath, CancellationToken cancellationToken, bool isPrefetch = false)
        {
            if (File.Exists(filePath))
                return;

            try
            {
                string operationType = isPrefetch ? "Prefetching" : "Background caching";
                Console.WriteLine($"{operationType}: {track.Name}");

                HttpClient httpClient = await _requestService.GetHttpClientAsync().ConfigureAwait(false);
                Uri requestUri = GetRequestUri(track.Guid);

                using HttpResponseMessage response = await httpClient.GetAsync(
                    requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"{operationType} failed: {response.StatusCode}");
                    return;
                }

                long? expectedLength = response.Content.Headers.ContentLength;

                using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using FileStream fileStream = new(
                    filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

                await contentStream.CopyToAsync(fileStream, 81920, cancellationToken).ConfigureAwait(false);

                if (expectedLength.HasValue && fileStream.Length != expectedLength.Value)
                {
                    Console.WriteLine($"Incomplete {operationType.ToLower()}, deleting");
                    File.Delete(filePath);
                }
                else
                {
                    Console.WriteLine($"✓ {operationType} complete: {track.Name}");
                    await _storageService.CleanupCacheAsync().ConfigureAwait(false);
                }

                AudioCacheChanged?.Invoke(CacheChangeMode.AudioCacheAdded);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Prefetch canceled: {track.Name}");
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
                using var headResponse = await httpClient.SendAsync(headRequest).ConfigureAwait(false);

                if (!headResponse.IsSuccessStatusCode)
                    return false;

                long? expectedLength = headResponse.Content.Headers.ContentLength;
                if (!expectedLength.HasValue)
                    return true;

                long fileLength = new FileInfo(filePath).Length;
                return fileLength == expectedLength.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying file: {ex.Message}");
                return false;
            }
        }

        private void CancelPrefetch()
        {
            if (_prefetchCancellation?.IsCancellationRequested == false)
            {
                _prefetchCancellation.Cancel();
                _prefetchCancellation.Dispose();
                _prefetchCancellation = null;
            }
        }

        private string GetCachedTracksFilePath(Track track)
        {
            return Path.Combine(_storageService.GetAudioDirectory(), track.Guid + track.Extension);
        }

        private Uri GetRequestUri(Guid guid)
        {
            var builder = new UriBuilder(_settingsService.ServiceEndPoint)
            {
                Path = Path.Combine(new Uri(_settingsService.ServiceEndPoint).AbsolutePath, $"api/files/audio/{guid}")
            };
            return builder.Uri;
        }

        // Event Handlers
        private void OnMediaOpened(MediaPlayer sender, object args)
        {
            _mediaPlayerElement?.DispatcherQueue.TryEnqueue(() =>
            {
                MediaStateChanged?.Invoke(MediaState.Opened);
            });
        }

        private void OnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            _mediaPlayerElement?.DispatcherQueue.TryEnqueue(() =>
            {
                Console.WriteLine($"Media playback failed: {args.ErrorMessage}");
                MediaStateChanged?.Invoke(MediaState.BadRequest);
            });
        }

        private void OnMediaEnded(MediaPlayer sender, object args)
        {
            _mediaPlayerElement?.DispatcherQueue.TryEnqueue(() =>
            {
                MediaStateChanged?.Invoke(MediaState.Ended);
            });
        }

        private void OnMediaStateChanged(MediaPlayer sender, object args)
        {
            _mediaPlayerElement?.DispatcherQueue.TryEnqueue(() =>
            {
                _currentPlayerState = sender.CurrentState switch
                {
                    MediaPlayerState.Buffering => PlayerState.Buffering,
                    MediaPlayerState.Opening => PlayerState.Opening,
                    MediaPlayerState.Paused => PlayerState.Paused,
                    MediaPlayerState.Playing => PlayerState.Playing,
                    MediaPlayerState.Stopped => PlayerState.Stopped,
                    _ => PlayerState.Closed,
                };
                PlayerStateChanged?.Invoke(_currentPlayerState);
            });
        }
    }
}
