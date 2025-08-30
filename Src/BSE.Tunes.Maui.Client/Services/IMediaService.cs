
using BSE.Tunes.Maui.Client.Models.Contract;
using CommunityToolkit.Maui.Views;

namespace BSE.Tunes.Maui.Client.Services
{
    public interface IMediaService
    {
        void RegisterAsMediaService(MediaElement mediaElement);
        event Action<PlayerState> PlayerStateChanged;
        event Action<MediaState> MediaStateChanged;
        void Disconnect();
        double Progress { get;}
        void Play();
        void Pause();
        void Stop();
        Task SetTrackAsync(Track track, Uri coverUri);
    }
}
