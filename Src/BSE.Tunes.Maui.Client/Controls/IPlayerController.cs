namespace BSE.Tunes.Maui.Client.Controls
{
    public interface IPlayerController
    {
        void SendPlayClicked();
        void SendPauseClicked();
        void SendPlayNextClicked();
        void SendPlayPreviousClicked();
        void SendSelectTrackClicked();
    }

}
