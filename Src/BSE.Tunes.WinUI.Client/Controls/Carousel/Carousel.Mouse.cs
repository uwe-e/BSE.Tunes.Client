using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace BSE.Tunes.WinUI.Client.Controls
{
    partial class Carousel
    {
        private bool _isArrowVisible = false;
        private bool _isArrowOver = false;
        private DispatcherTimer _fadeTimer = null;

        private void CreateFadeTimer()
        {
            _fadeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1500)
            };
            _fadeTimer.Tick += OnFadeTimerTick;
        }

        private void DisposeFadeTimer()
        {
            var fadeTimer = _fadeTimer;
            _fadeTimer = null;
            fadeTimer?.Stop();
        }

        private void OnArrowPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _isArrowOver = true;
        }

        private void OnArrowPointerExited(object sender, PointerRoutedEventArgs e)
        {
            _isArrowOver = false;
        }

        private void OnLeftClick(object sender, RoutedEventArgs e)
        {
            MoveBack();
        }

        private void OnRightClick(object sender, RoutedEventArgs e)
        {
            MoveForward();
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isArrowVisible)
            {
                _arrows.FadeIn();
                _isArrowVisible = true;
            }
            this._fadeTimer.Start();
        }

        private void OnFadeTimerTick(object sender, object e)
        {
            if (_isArrowVisible && !_isArrowOver)
            {
                _isArrowVisible = false;
                _arrows.FadeOut();
            }
        }
    }
}
