namespace BSE.Tunes.Shared.Services.Models.Contract
{
    public class PlaylistSummary
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string? Name { get; set; }
        public string? Owner { get; set; }
    }
}
