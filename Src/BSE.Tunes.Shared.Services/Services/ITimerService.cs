namespace BSE.Tunes.Shared.Services.Services
{
    public interface ITimerService
    {
        event Action TimerElapsed;
        void Start();
        void Stop();
    }
}
