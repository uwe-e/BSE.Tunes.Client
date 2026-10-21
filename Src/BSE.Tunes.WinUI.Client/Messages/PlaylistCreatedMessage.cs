namespace BSE.Tunes.WinUI.Client.Messages;

public class PlaylistCreatedMessage
{
    public int PlaylistId { get; }

    public PlaylistCreatedMessage(int playlistId)
    {
        PlaylistId = playlistId;
    }
}