using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSE.Tunes.WinUI.Client.Models
{
    public class PlaylistEntryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Genre { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string FormattedDuration => Duration.ToString(@"mm\:ss");
        public object? Data { get; set; }

        public static PlaylistEntryItem FromPlaylistEntry(PlaylistEntry entry)
        {
            return new PlaylistEntryItem
            {
                Id = entry.Id,
                Name = entry.Name ?? "Unknown Entry",
                Artist = entry.Track?.Album?.Artist?.Name ?? "Unknown Artist",
                Album = entry.Track?.Album?.Title ?? string.Empty,
                Year = entry.Track?.Album?.Year ?? 0,
                Genre = entry.Track?.Album?.Genre?.Name ?? string.Empty,
                Duration = entry.Duration,
                Data = entry
            };
        }

    }
}
