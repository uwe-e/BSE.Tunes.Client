namespace BSE.Tunes.Shared.Services.Models.Contract
{
    public class PlaylistEntry
    {
        public int Id { get; set; }

        public int PlaylistId { get; set; }

        public int TrackId { get; set; }

        public string? Name { get; set; }

        public string? Artist { get; set; }

        public int? Year { get; set; }

        public Guid AlbumId { get; set; }

        public TimeSpan Duration { get; set; }

        public Guid Guid { get; set; }

        public int SortOrder { get; set; }

        public Track? Track { get; set; }
    }
}
