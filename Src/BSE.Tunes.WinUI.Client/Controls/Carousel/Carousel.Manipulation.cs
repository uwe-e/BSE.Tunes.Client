using Microsoft.UI.Xaml.Input;

namespace BSE.Tunes.WinUI.Client.Controls
{
    partial class Carousel
    {
        private bool _isBusy = false;

        public double Position
        {
            get { return _panel.GetTranslateX(); }
            set
            {
                _panel.TranslateX(value);
            }
        }

        public double Offset
        {
            get
            {
                double position = this.Position % this.ItemWidth;
                if (Math.Sign(position) > 0)
                {
                    return this.ItemWidth - position;
                }
                return -position;
            }
        }

        private int _direction = 0;

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            _direction = Math.Sign(e.Delta.Translation.X);
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (_direction > 0)
            {
                MoveBack();
            }
            else
            {
                MoveForward();
            }
        }

        private async void AnimateNext(double duration = 150)
        {
            _isBusy = true;
            double delta = this.ItemWidth - this.Offset;
            double position = Position - delta;

            await _panel.AnimateXAsync(position, duration);

            this.Index = (int)(-position / this.ItemWidth);
            _isBusy = false;
        }

        private async void AnimatePrev(double duration = 150)
        {
            _isBusy = true;
            double delta = this.Offset;
            double position = Position + delta;

            await _panel.AnimateXAsync(position, duration);

            this.Index = (int)(-position / this.ItemWidth);
            _isBusy = false;
        }
    }
}
