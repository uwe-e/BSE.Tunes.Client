namespace BSE.Tunes.Maui.Client.Models
{
    public class PlaylistActionContext
    {
        public PlaylistActionMode ActionMode { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to display album info from a dialog like the NowPlayingPage.
        /// </summary>
        public bool DisplayAlbumInfoFromDialog { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to display the ShowAlbum menu item.
        /// </summary>
        public bool DisplayAlbumInfo { get; set; }

        public Playlist PlaylistTo { get; set; }

        public object Data { get; set; }
    }
}
