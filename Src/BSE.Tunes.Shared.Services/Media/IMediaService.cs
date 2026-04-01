using BSE.Tunes.Shared.Services.Enums;
using BSE.Tunes.Shared.Services.Models.Contract;

namespace BSE.Tunes.Shared.Services.Media
{
    public interface IMediaService
    {
        event Action<PlayerState> PlayerStateChanged;
        event Action<MediaState> MediaStateChanged;
        event Action<CacheChangeMode> AudioCacheChanged;
        
        double Progress { get;}
        
        void Play();
        void Pause();
        void Stop();
        void Disconnect();

        Task SetTrackAsync(Track track, Uri coverUri);
        Task PrefetchNextTrackAsync(Track nextTrack);
    }
}
