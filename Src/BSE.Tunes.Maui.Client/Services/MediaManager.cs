using BSE.Tunes.Maui.Client.Events;

namespace BSE.Tunes.Maui.Client.Services;

/// <summary>
/// MAUI-specific implementation of MediaManager with EventAggregator integration
/// </summary>
public class MediaManager : MediaManagerBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly ITimerService _timerService;

    public MediaManager(
        IDataService dataService,
        IMediaService mediaService,
        IEventAggregator eventAggregator,
        ITimerService timerService)
        : base(dataService, mediaService)
    {
        _eventAggregator = eventAggregator;
        _timerService = timerService;
        
        _timerService.TimerElapsed += OnTimerElapsed;
        _timerService.Start();

        // Subscribe to cleanup event
        _eventAggregator.GetEvent<CleanUpResourcesEvent>().Subscribe(() =>
        {
            Disconnect();
        }, ThreadOption.UIThread);
    }

    protected override async Task OnTrackOpenedAsync(Track track)
    {
        // Update playback history
        await UpdateHistoryAsync(track);
    }

    protected override void OnAudioCacheChangedCore(CacheChangeMode mode)
    {
        // Publish cache change to UI
        _eventAggregator.GetEvent<CacheChangedEvent>().Publish(mode);
    }

    protected override void OnProgressChanged(double progress, TimeSpan position, TimeSpan duration)
    {
        // Publish progress to UI
        _eventAggregator.GetEvent<MediaProgressChangedEvent>().Publish(progress);
    }

    private void OnTimerElapsed()
    {
        // Call base class progress update
        UpdateProgress();
    }

    private async Task UpdateHistoryAsync(Track currentTrack)
    {
        await _dataService.UpdateHistory(new History
        {
            PlayMode = (int)PlayerMode,
            AlbumId = currentTrack.Album.Id,
            TrackId = currentTrack.Id,
            PlayedAt = DateTime.Now
        });
    }
}
