namespace BSE.Tunes.WinUI.Client.Messages
{
    public class PlaylistChangedMessage
    {
        public int PlaylistId { get; }

        public PlaylistChangedMessage(int playlistId)
        {
            PlaylistId = playlistId;
        }
    }
}
