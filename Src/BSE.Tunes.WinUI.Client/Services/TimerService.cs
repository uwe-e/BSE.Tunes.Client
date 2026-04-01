using Microsoft.UI.Dispatching;

namespace BSE.Tunes.WinUI.Client.Services;

public class TimerService : ITimerService
{
    private DispatcherQueueTimer _timer;
    
    public event Action TimerElapsed;

    public TimerService()
    {
        _timer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(500);
        _timer.Tick += (s, e) => TimerElapsed?.Invoke();
    }

    public void Start()
    {
        _timer?.Start();
    }

    public void Stop()
    {
        _timer?.Stop();
    }
}