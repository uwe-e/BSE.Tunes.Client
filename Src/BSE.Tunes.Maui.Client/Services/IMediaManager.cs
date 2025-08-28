using BSE.Tunes.Maui.Client.Collections;
using BSE.Tunes.Maui.Client.Models.Contract;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BSE.Tunes.Maui.Client.Services
{
    public interface IMediaManager
    {
        event Action<PlayerState> PlayerStateChanged;
        event Action<MediaState> MediaStateChanged;
        event NotifyCollectionChangedEventHandler PlaylistCollectionChanged;
        NavigableCollection<int> Playlist { get; set; }
        PlayerMode PlayerMode { get; }
        PlayerState PlayerState { get; }
        Track CurrentTrack { get; }
        bool CanPlay();
        void Play();
        Task PlayTracksAsync(PlayerMode playerMode);
        Task PlayTracksAsync(ObservableCollection<int> trackIds, PlayerMode playerMode);
        bool CanPlayPreviousTrack();
        Task PlayPreviousTrackAsync();
        bool CanPlayNextTrack();
        Task PlayNextTrackAsync();
        void Pause();
        Task InsertTracksToPlayQueueAsync(ObservableCollection<int> trackIds, PlayerMode playerMode);
        void Disconnect();
    }
}
