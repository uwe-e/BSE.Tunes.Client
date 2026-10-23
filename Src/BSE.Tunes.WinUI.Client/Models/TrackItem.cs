namespace BSE.Tunes.WinUI.Client.Models;

public class TrackItem
{
    public int Id { get; set; }
    public int TrackNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string FormattedDuration => Duration.ToString(@"mm\:ss");
    
    // Keep reference to original contract model
    public object? Data { get; set; }
    
    // Factory method for safe conversion
    public static TrackItem FromTrack(Track track)
    {
        return new TrackItem
        {
            Id = track.Id,
            TrackNumber = track.TrackNumber,
            Title = track.Name ?? "Unknown Track",
            Artist = track.Album?.Artist?.Name ?? "Unknown Artist",
            Album = track.Album?.Title ?? string.Empty,
            Duration = track.Duration,
            Data = track
        };
    }
    
    // Get original model when needed (e.g., for playback)
    public Track? GetTrack() => Data as Track;
}