using BSE.Tunes.Shared.Services.Collections;
using BSE.Tunes.Shared.Services.Enums;
using BSE.Tunes.Shared.Services.Models.Contract;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BSE.Tunes.Shared.Services.Media
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
