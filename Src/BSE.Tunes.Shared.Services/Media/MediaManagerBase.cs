using BSE.Tunes.Shared.Services.Collections;
using BSE.Tunes.Shared.Services.Enums;
using BSE.Tunes.Shared.Services.Extensions;
using BSE.Tunes.Shared.Services.Models.Contract;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BSE.Tunes.Shared.Services.Media;

/// <summary>
/// Platform-agnostic base implementation of IMediaManager.
/// Contains all business logic for playlist management, navigation, and playback orchestration.
/// </summary>
public abstract class MediaManagerBase : IMediaManager
{
    protected readonly IDataService _dataService;
    protected readonly IMediaService _mediaService;
    private double _oldProgress;
    private NavigableCollection<int> _playlist;
    private bool _hasTriggeredPrefetch;

    public event Action<PlayerState> PlayerStateChanged;
    public event Action<MediaState> MediaStateChanged;
    public event NotifyCollectionChangedEventHandler PlaylistCollectionChanged;

    public NavigableCollection<int> Playlist
    {
        get => _playlist;
        set
        {
            if (_playlist != null)
            {
                _playlist.CollectionChanged -= OnPlaylistCollectionChanged;
            }

            _playlist = value;
            if (_playlist != null)
            {
                _playlist.CollectionChanged += OnPlaylistCollectionChanged;
            }
        }
    }

    private void OnPlaylistCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        PlaylistCollectionChanged?.Invoke(this, e);
    }

    public PlayerMode PlayerMode { get; private set; }

    public PlayerState PlayerState { get; private set; } = PlayerState.Closed;

    public Track CurrentTrack { get; private set; }

    protected MediaManagerBase(
        IDataService dataService,
        IMediaService mediaService)
    {
        _dataService = dataService;
        _mediaService = mediaService;

        _mediaService.PlayerStateChanged += OnPlayerStateChanged;
        _mediaService.MediaStateChanged += OnMediaStateChanged;
        _mediaService.AudioCacheChanged += OnAudioCacheChanged;
    }

    public void Disconnect()
    {
        _mediaService?.Disconnect();
    }

    public void Pause()
    {
        _mediaService.Pause();
    }

    public bool CanPlay()
    {
        return Playlist?.Count > 0;
    }

    public void Play()
    {
        _mediaService.Play();
    }

    public async Task PlayTracksAsync(PlayerMode playerMode)
    {
        PlayerMode = playerMode;
        int trackId = Playlist?.FirstOrDefault() ?? 0;
        await PlayTrackAsync(trackId);
    }

    public async Task PlayTracksAsync(ObservableCollection<int> trackIds, PlayerMode playerMode)
    {
        _mediaService.Stop();
        Playlist = trackIds.ToNavigableCollection();
        await PlayTracksAsync(playerMode);
    }

    public bool CanPlayPreviousTrack()
    {
        return Playlist?.CanMovePrevious ?? false;
    }

    public async Task PlayPreviousTrackAsync()
    {
        if (CanPlayPreviousTrack())
        {
            if (Playlist.MovePrevious())
            {
                await PlayTrackAsync(Playlist.Current);
            }
        }
    }

    public bool CanPlayNextTrack()
    {
        return Playlist?.CanMoveNext ?? false;
    }

    public async Task PlayNextTrackAsync()
    {
        if (CanPlayNextTrack())
        {
            if (Playlist.MoveNext())
            {
                await PlayTrackAsync(Playlist.Current);
            }
        }
    }

    public async Task InsertTracksToPlayQueueAsync(ObservableCollection<int> trackIds, PlayerMode playerMode)
    {
        if (trackIds == null || trackIds.Count == 0)
            return;

        if ((PlayerState == PlayerState.Playing || PlayerState == PlayerState.Paused)
            && Playlist != null && Playlist.Count > 0)
        {
            int index = Playlist.IndexOf(Playlist.Current);
            int insertIndex = index + 1;

            for (int i = 0; i < trackIds.Count; i++)
            {
                Playlist.Insert(insertIndex + i, trackIds[i]);
            }
        }
        else
        {
            await PlayTracksAsync(trackIds, playerMode);
        }
    }

    private async Task PlayTrackAsync(int trackId)
    {
        _mediaService.Stop();
        _hasTriggeredPrefetch = false; // Reset flag for new track

        if (trackId > 0)
        {
            Track track = await _dataService.GetTrackById(trackId);
            if (track != null)
            {
                await _mediaService.SetTrackAsync(track, _dataService.GetImage(track.Album.AlbumId, true));
            }
        }
    }

    private async Task PrefetchNextTrackInPlaylistAsync()
    {
        if (Playlist == null || !Playlist.CanMoveNext)
            return;

        try
        {
            // Get the next track ID without moving the playlist position
            int currentIndex = Playlist.IndexOf(Playlist.Current);
            if (currentIndex >= 0 && currentIndex + 1 < Playlist.Count)
            {
                int nextTrackId = Playlist[currentIndex + 1];

                if (nextTrackId > 0)
                {
                    Track nextTrack = await _dataService.GetTrackById(nextTrackId);
                    if (nextTrack != null)
                    {
                        Console.WriteLine($"Triggering prefetch for: {nextTrack.Name}");
                        await _mediaService.PrefetchNextTrackAsync(nextTrack);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error prefetching next track: {ex.Message}");
        }
    }

    private void OnPlayerStateChanged(PlayerState state)
    {
        if (PlayerState != state)
        {
            PlayerState = state;
            PlayerStateChanged?.Invoke(state);
        }
    }

    private async void OnMediaStateChanged(MediaState state)
    {
        switch (state)
        {
            case MediaState.Opened:
                var trackId = Playlist.Current;
                if (trackId > 0)
                {
                    CurrentTrack = await _dataService.GetTrackById(trackId);

                    // Platform-specific hook for history tracking
                    await OnTrackOpenedAsync(CurrentTrack);

                    if (!_hasTriggeredPrefetch)
                    {
                        _hasTriggeredPrefetch = true;
                        _ = PrefetchNextTrackInPlaylistAsync();
                    }
                }
                break;

            case MediaState.Ended:
                if (PlayerMode != PlayerMode.None && CanPlayNextTrack())
                {
                    await PlayNextTrackAsync();
                }
                break;
        }

        MediaStateChanged?.Invoke(state);
    }

    private void OnAudioCacheChanged(CacheChangeMode mode)
    {
        // Platform-specific hook for cache change notifications
        OnAudioCacheChangedCore(mode);
    }

    /// <summary>
    /// Called when a track is successfully opened. Override to implement platform-specific logic
    /// such as updating playback history or analytics.
    /// </summary>
    protected virtual Task OnTrackOpenedAsync(Track track)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when audio cache changes. Override to implement platform-specific notifications.
    /// </summary>
    protected virtual void OnAudioCacheChangedCore(CacheChangeMode mode)
    {
        // Platform implementations can override this
    }

    /// <summary>
    /// Called periodically with playback progress. Override to implement platform-specific
    /// progress tracking or UI updates.
    /// </summary>
    protected virtual void OnProgressChanged(double progress)
    {
        // Platform implementations can override this
    }

    /// <summary>
    /// Should be called periodically by platform-specific timer implementations
    /// </summary>
    protected void UpdateProgress()
    {
        var newProgress = _mediaService.Progress;
        if (newProgress != _oldProgress && newProgress < 1.0)
        {
            OnProgressChanged(newProgress);
            _oldProgress = newProgress;
        }
    }
}