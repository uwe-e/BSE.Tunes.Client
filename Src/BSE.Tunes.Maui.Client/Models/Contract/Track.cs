namespace BSE.Tunes.Maui.Client.Models.Contract
{
    public class Track
    {
        public int Id
        {
            get; set;
        }

        public int TrackNumber
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public TimeSpan Duration
        {
            get; set;
        }

        public Guid Guid
        {
            get; set;
        }

        public Album Album
        {
            get; set;
        }
    }
}
