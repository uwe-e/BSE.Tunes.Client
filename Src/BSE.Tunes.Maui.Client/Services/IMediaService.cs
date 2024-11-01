
using BSE.Tunes.Maui.Client.Models.Contract;
using CommunityToolkit.Maui.Views;

namespace BSE.Tunes.Maui.Client.Services
{
    public interface IMediaService
    {
        void RegisterAsMediaService(MediaElement mediaElement);
        void Stop();
        void SetTrack(Track track);
        void SetTrack(Track track, Uri coverUri);
    }
}
