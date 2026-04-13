using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace BSE.Tunes.WinUI.Client.Behaviors
{
    public class StickyHeaderBehavior : Behavior<ListView>
    {
        private ScrollViewer _scrollViewer;
        private string _currentState = "Collapsed";
        public static readonly DependencyProperty HeaderElementProperty =
            DependencyProperty.Register(
                nameof(HeaderElement),
                typeof(FrameworkElement),
                typeof(StickyHeaderBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CoverImageProperty =
            DependencyProperty.Register(
                nameof(CoverImage),
                typeof(FrameworkElement),
                typeof(StickyHeaderBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty MaxHeaderHeightProperty =
            DependencyProperty.Register(
                nameof(MaxHeaderHeight),
                typeof(double),
                typeof(StickyHeaderBehavior),
                new PropertyMetadata(280.0));

        public static readonly DependencyProperty MinHeaderHeightProperty =
            DependencyProperty.Register(
                nameof(MinHeaderHeight),
                typeof(double),
                typeof(StickyHeaderBehavior),
                new PropertyMetadata(120.0));

        public static readonly DependencyProperty MaxCoverSizeProperty =
            DependencyProperty.Register(
                nameof(MaxCoverSize),
                typeof(double),
                typeof(StickyHeaderBehavior),
                new PropertyMetadata(240.0));

        public static readonly DependencyProperty MinCoverSizeProperty =
            DependencyProperty.Register(
                nameof(MinCoverSize),
                typeof(double),
                typeof(StickyHeaderBehavior),
                new PropertyMetadata(80.0));

        public static readonly DependencyProperty ScrollThresholdProperty =
            DependencyProperty.Register(
                nameof(ScrollThreshold),
                typeof(double),
                typeof(StickyHeaderBehavior),
                new PropertyMetadata(200.0));

        public FrameworkElement HeaderElement
        {
            get => (FrameworkElement)GetValue(HeaderElementProperty);
            set => SetValue(HeaderElementProperty, value);
        }

        public FrameworkElement CoverImage
        {
            get => (FrameworkElement)GetValue(CoverImageProperty);
            set => SetValue(CoverImageProperty, value);
        }

        public double MaxHeaderHeight
        {
            get => (double)GetValue(MaxHeaderHeightProperty);
            set => SetValue(MaxHeaderHeightProperty, value);
        }

        public double MinHeaderHeight
        {
            get => (double)GetValue(MinHeaderHeightProperty);
            set => SetValue(MinHeaderHeightProperty, value);
        }

        public double MaxCoverSize
        {
            get => (double)GetValue(MaxCoverSizeProperty);
            set => SetValue(MaxCoverSizeProperty, value);
        }

        public double MinCoverSize
        {
            get => (double)GetValue(MinCoverSizeProperty);
            set => SetValue(MinCoverSizeProperty, value);
        }

        public double ScrollThreshold
        {
            get => (double)GetValue(ScrollThresholdProperty);
            set => SetValue(ScrollThresholdProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= OnLoaded;
            if (_scrollViewer != null)
            {
                _scrollViewer.ViewChanging -= OnViewChanging;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _scrollViewer = FindScrollViewer(AssociatedObject);
            if (_scrollViewer != null)
            {
                _scrollViewer.ViewChanging += OnViewChanging;
            }
        }

        private void OnViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            var verticalOffset = e.NextView.VerticalOffset;
            var scaleFactor = Math.Max(0, Math.Min(1, 1 - (verticalOffset / ScrollThreshold)));

            // Calculate new sizes
            var newHeaderHeight = MinHeaderHeight + ((MaxHeaderHeight - MinHeaderHeight) * scaleFactor);
            var newCoverSize = MinCoverSize + ((MaxCoverSize - MinCoverSize) * scaleFactor);

            // Apply directly - simple and smooth
            if (HeaderElement != null)
            {
                HeaderElement.Height = newHeaderHeight;

                // Trigger Visual State change only when crossing threshold
                var threshold = 0.5;
                var stateName = scaleFactor > threshold ? "Expanded" : "Collapsed";

                // Only call GoToState if the state actually changed
                if (stateName != _currentState)
                {
                    _currentState = stateName;
                    // Get the control that has the VisualStateGroups
                    Control targetControl = null;

                    if (HeaderElement is UserControl userControl)
                    {
                        // When VisualStateGroups are on the UserControl's content (Grid),
                        // we need to pass the UserControl itself as the control parameter
                        var result = VisualStateManager.GoToState(userControl, stateName, true);
                        System.Diagnostics.Debug.WriteLine($"GoToState({stateName}) on UserControl returned: {result}");
                    }
                    else if (HeaderElement is Control control)
                    {
                        var result = VisualStateManager.GoToState(control, stateName, true);
                        System.Diagnostics.Debug.WriteLine($"GoToState({stateName}) on {control.GetType().Name} returned: {result}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"HeaderElement is not a Control! Type: {HeaderElement.GetType().Name}");
                    }
                }

            }

            if (CoverImage != null)
            {
                CoverImage.Width = newCoverSize;
                CoverImage.Height = newCoverSize;
            }
        }

        private ScrollViewer FindScrollViewer(DependencyObject element)
        {
            if (element is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            var childCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childCount; i++)
            {
                var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(element, i);
                var result = FindScrollViewer(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}