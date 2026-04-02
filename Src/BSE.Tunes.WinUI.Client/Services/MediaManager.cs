using BSE.Tunes.WinUI.Client.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace BSE.Tunes.WinUI.Client.Services;

/// <summary>
/// WinUI-specific implementation of MediaManager
/// </summary>
public class MediaManager : MediaManagerBase
{
    private readonly IMessenger _messenger;
    private readonly ITimerService _timerService;

    public MediaManager(
        IDataService dataService,
        IMediaService mediaService,
        IMessenger messenger,
        ITimerService timerService)
        : base(dataService, mediaService)
    {
        _messenger = messenger;
        _timerService = timerService;

        _timerService.TimerElapsed += OnTimerElapsed;
        _timerService.Start();
    }

    protected override async Task OnTrackOpenedAsync(Track track)
    {
        // Update playback history
        await UpdateHistoryAsync(track);

        // WinUI-specific: Could update system media transport controls here
        // UpdateSystemMediaTransportControls(track);
    }

    protected override void OnAudioCacheChangedCore(CacheChangeMode mode)
    {
        // WinUI-specific: Could raise notifications or update UI
        // For now, could use a messenger pattern or leave empty
    }

    protected override void OnProgressChanged(double progress)
    {
        // WinUI-specific: Update UI via messenger or events
        _messenger.Send(new MediaProgressChangedMessage(progress));
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