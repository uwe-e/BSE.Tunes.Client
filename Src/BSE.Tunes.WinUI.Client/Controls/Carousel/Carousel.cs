using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace BSE.Tunes.WinUI.Client.Controls
{
    public sealed partial class Carousel : Control
    {
        private const double ANIMATION_DURATION = 150.0;
        private const double SLIDE_DELTA = 0.01;
        
        private Panel? _frame;
        private CarouselPanel? _panel;
        private Grid? _arrows;
        private Button? _left;
        private Button? _right;
        private LinearGradientBrush? _gradient;
        private RectangleGeometry? _clip;

        public Carousel()
        {
            this.DefaultStyleKey = typeof(Carousel);
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            this.SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CreateFadeTimer();
            if (_slideTimer is not null && this.SlideInterval > ANIMATION_DURATION)
            {
                _slideTimer.Start();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UnsubscribeEvents();
            DisposeFadeTimer();
            _slideTimer?.Stop();
        }

        protected override void OnApplyTemplate()
        {
            // Unsubscribe from old template elements
            UnsubscribeEvents();

            // Get new template elements
            _frame = GetTemplateChild("frame") as Panel;
            _panel = GetTemplateChild("panel") as CarouselPanel;
            _arrows = GetTemplateChild("arrows") as Grid;
            _left = GetTemplateChild("PreviousButtonHorizontal") as Button;
            _right = GetTemplateChild("NextButtonHorizontal") as Button;
            _gradient = GetTemplateChild("gradient") as LinearGradientBrush;
            _clip = GetTemplateChild("clip") as RectangleGeometry;

            // Subscribe to new template elements
            SubscribeEvents();

            base.OnApplyTemplate();
        }

        private void SubscribeEvents()
        {
            if (_frame is not null)
            {
                _frame.ManipulationDelta += OnManipulationDelta;
                _frame.ManipulationCompleted += OnManipulationCompleted;
                _frame.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.System;
                _frame.PointerMoved += OnPointerMoved;
            }

            if (_left is not null)
            {
                _left.Click += OnLeftClick;
                _left.PointerEntered += OnArrowPointerEntered;
                _left.PointerExited += OnArrowPointerExited;
            }

            if (_right is not null)
            {
                _right.Click += OnRightClick;
                _right.PointerEntered += OnArrowPointerEntered;
                _right.PointerExited += OnArrowPointerExited;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_frame is not null)
            {
                _frame.ManipulationDelta -= OnManipulationDelta;
                _frame.ManipulationCompleted -= OnManipulationCompleted;
                _frame.PointerMoved -= OnPointerMoved;
            }

            if (_left is not null)
            {
                _left.Click -= OnLeftClick;
                _left.PointerEntered -= OnArrowPointerEntered;
                _left.PointerExited -= OnArrowPointerExited;
            }

            if (_right is not null)
            {
                _right.Click -= OnRightClick;
                _right.PointerEntered -= OnArrowPointerEntered;
                _right.PointerExited -= OnArrowPointerExited;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = NormalizeSize(availableSize);

            double width = availableSize.Width / this.MaxItems;
            double height = width / this.AspectRatio;

            if (height < MinHeight)
            {
                height = MinHeight;
                width = height * this.AspectRatio;
            }

            if (height > MaxHeight)
            {
                height = MaxHeight;
                width = height * this.AspectRatio;
            }

            if (_panel is not null)
            {
                _panel.ItemWidth = Math.Round(width);
                _panel.ItemHeight = Math.Round(height);
                this.Position = -this.Index * width;
                
                return base.MeasureOverride(new Size(availableSize.Width, height));
            }
            
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(new Size(finalSize.Width, _panel?.ItemHeight ?? finalSize.Height));
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_clip is not null && _gradient is not null)
            {
                _clip.Rect = new Rect(new Point(), e.NewSize);
                ApplyGradient();
            }
        }

        private void ApplyGradient()
        {
            if (_gradient is not null && this.MaxItems > 2)
            {
                double factor = 1.0 / this.MaxItems;
                int index = this.MaxItems / 2;
                int count = 1;
                
                if (this.MaxItems % 2 == 0)
                {
                    index--;
                    count++;
                }
                
                _gradient.GradientStops[1].Offset = factor * index;
                _gradient.GradientStops[2].Offset = factor * (index + count);
            }
        }

        private Size NormalizeSize(Size size)
        {
            double width = size.Width;
            double height = size.Height;

            // WinUI 3: Use XamlRoot instead of Window.Current
            if (double.IsInfinity(width) || double.IsInfinity(height))
            {
                if (this.XamlRoot is not null)
                {
                    var xamlRootSize = this.XamlRoot.Size;
                    
                    if (double.IsInfinity(width))
                    {
                        width = xamlRootSize.Width;
                    }
                    if (double.IsInfinity(height))
                    {
                        height = xamlRootSize.Height;
                    }
                }
                else
                {
                    // Fallback to reasonable defaults
                    width = double.IsInfinity(width) ? 1920 : width;
                    height = double.IsInfinity(height) ? 1080 : height;
                }
            }

            return new Size(width, height);
        }

        public void MoveBack()
        {
            if (_isBusy || _panel is null)
                return;
                
            _panel.TranslateDeltaX(SLIDE_DELTA);
            AnimatePrev();
        }

        public void MoveForward()
        {
            if (_isBusy || _panel is null)
                return;
                
            _panel.TranslateDeltaX(-SLIDE_DELTA);
            AnimateNext();
        }
    }
}
