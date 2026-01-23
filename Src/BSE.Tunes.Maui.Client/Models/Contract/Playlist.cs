using System.Text.Json.Serialization;

namespace BSE.Tunes.Maui.Client.Models.Contract
{
    public class Playlist
    {
        public int Id
        {
            get; set;
        }
        public Guid Guid
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        [property: JsonPropertyName("entrycount")]
        public int NumberEntries
        {
            get;
            set;
        }
        [property: JsonPropertyName("owner")]
        public string UserName
        {
            get;
            set;
        }
        public IList<string> CoverAlbumIds { get; set; }
        public IList<PlaylistEntry> Entries { get; set; }
    }
}
