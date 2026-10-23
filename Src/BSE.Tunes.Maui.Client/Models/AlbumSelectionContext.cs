namespace BSE.Tunes.Maui.Client.Models
{
    public class AlbumSelectionContext
    {
        public AlbumSelectionMode Mode { get; set; }
        public UniqueAlbum UniqueAlbum { get; set; }
        public Guid OriginatorId { get; set; }  // To prevent circular triggers
    }
}