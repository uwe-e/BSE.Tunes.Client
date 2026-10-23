namespace BSE.Tunes.WinUI.Client.Messages;

public class MediaProgressChangedMessage(double progress, TimeSpan position, TimeSpan duration)
{
    public double Progress { get; } = progress;
    public TimeSpan Position { get; } = position;
    public TimeSpan Duration { get; } = duration;
}