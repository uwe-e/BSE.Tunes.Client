using BSE.Tunes.Maui.Client.Models.Contract;

namespace BSE.Tunes.Maui.Client.Models
{
    public class UniqueAlbum
    {
        public Guid UniqueId
        {
            get; set;
        }

        public Album? Album
        {
            get; set;
        }
    }
}
