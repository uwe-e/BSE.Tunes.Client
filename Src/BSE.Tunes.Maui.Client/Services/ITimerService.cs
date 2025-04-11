namespace BSE.Tunes.Maui.Client.Services
{
    public interface ITimerService
    {
        event Action TimerElapsed;
        void Start();
        void Stop();
    }
}
