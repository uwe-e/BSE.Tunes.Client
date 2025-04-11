using BSE.Tunes.Maui.Client.Models.Contract;

namespace BSE.Tunes.Maui.Client.Models
{
    public class PlaylistActionContext
    {
        public PlaylistActionMode ActionMode { get; set; }

        public bool DisplayAlbumInfo { get; set; }

        public Playlist PlaylistTo { get; set; }

        public object Data { get; set; }
    }
}
