using BSE.Tunes.Maui.Client.Collections;
using BSE.Tunes.Maui.Client.Models.Contract;
using System.Collections.ObjectModel;

namespace BSE.Tunes.Maui.Client.Services
{
    public interface IMediaManager
    {
        event Action<PlayerState> PlayerStateChanged;
        event Action<MediaState> MediaStateChanged;
        NavigableCollection<int> Playlist { get; set; }
        PlayerMode PlayerMode { get; }
        PlayerState PlayerState { get; }
        Track CurrentTrack { get; }
        bool CanPlay();
        void Play();
        void PlayTracks(PlayerMode playerMode);
        void PlayTracks(ObservableCollection<int> trackIds, PlayerMode playerMode);
        bool CanPlayPreviosTrack();
        void PlayPreviousTrack();
        bool CanPlayNextTrack();
        void PlayNextTrack();
        void Pause();
    }
}
