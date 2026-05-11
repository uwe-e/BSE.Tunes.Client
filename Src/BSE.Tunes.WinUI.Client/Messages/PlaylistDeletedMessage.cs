namespace BSE.Tunes.WinUI.Client.Messages;

public class PlaylistDeletedMessage
{
    public int PlaylistId { get; }

    public PlaylistDeletedMessage(int playlistId)
    {
        PlaylistId = playlistId;
    }
}