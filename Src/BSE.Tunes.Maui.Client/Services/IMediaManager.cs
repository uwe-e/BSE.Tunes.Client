using BSE.Tunes.Maui.Client.Collections;
using System.Collections.ObjectModel;

namespace BSE.Tunes.Maui.Client.Services
{
    public interface IMediaManager
    {

        NavigableCollection<int> Playlist { get; set; }
        PlayerMode PlayerMode { get; }
        PlayerState PlayerState { get; }
        void PlayTracks(PlayerMode playerMode);
        void PlayTracks(ObservableCollection<int> trackIds, PlayerMode playerMode);
    }
}
