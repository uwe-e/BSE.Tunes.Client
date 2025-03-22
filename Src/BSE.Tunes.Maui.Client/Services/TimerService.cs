namespace BSE.Tunes.Maui.Client.Services
{
    public class TimerService : ITimerService, IDisposable
    {
        private Timer _timer;
        private bool _isRunning;
        public event Action TimerElapsed;

        public void Start()
        {
            if (_isRunning)
            {
                return;
            }

            _timer = new Timer(OnTimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            _isRunning = true;
        }

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
            _isRunning = false;
        }

        public void Dispose()
        {
            Stop();
        }

        private void OnTimerCallback(object state)
        {
            TimerElapsed?.Invoke();
        }
    }
}
