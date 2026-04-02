namespace BSE.Tunes.WinUI.Client.Messages;

public class MediaProgressChangedMessage
{
    public double Progress { get; }

    public MediaProgressChangedMessage(double progress)
    {
        Progress = progress;
    }
}