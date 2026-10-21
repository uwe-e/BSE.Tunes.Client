namespace BSE.Tunes.WinUI.Client.Messages;

public class TrackChangedMessage
{
    public Track Track { get; }

    public TrackChangedMessage(Track track)
    {
        Track = track;
    }
}