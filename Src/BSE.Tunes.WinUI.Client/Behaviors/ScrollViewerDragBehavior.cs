using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace BSE.Tunes.WinUI.Client.Behaviors;

public static class ScrollViewerDragBehavior
{
    private static bool _isDragging;
    private static double _startHorizontalOffset;
    private static double _startX;
    private const double DragThreshold = 5.0; // Minimum pixels to consider it a drag

    #region IsEnabled Attached Property

    public static bool GetIsEnabled(DependencyObject obj)
        => (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value)
        => obj.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ScrollViewerDragBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer)
        {
            if ((bool)e.NewValue)
            {
                scrollViewer.Loaded += OnScrollViewerLoaded;
            }
            else
            {
                scrollViewer.Loaded -= OnScrollViewerLoaded;
                DetachEvents(scrollViewer);
            }
        }
    }

    #endregion

    #region IsDragging Attached Property

    public static bool GetIsDragging(DependencyObject obj)
        => (bool)obj.GetValue(IsDraggingProperty);

    internal static void SetIsDragging(DependencyObject obj, bool value)
        => obj.SetValue(IsDraggingProperty, value);

    public static readonly DependencyProperty IsDraggingProperty =
        DependencyProperty.RegisterAttached(
            "IsDragging",
            typeof(bool),
            typeof(ScrollViewerDragBehavior),
            new PropertyMetadata(false));

    #endregion

    #region Event Handlers

    private static void OnScrollViewerLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            AttachEvents(scrollViewer);
        }
    }

    private static void AttachEvents(ScrollViewer scrollViewer)
    {
        scrollViewer.PointerPressed += OnPointerPressed;
        scrollViewer.PointerMoved += OnPointerMoved;
        scrollViewer.PointerReleased += OnPointerReleased;
        scrollViewer.PointerCanceled += OnPointerReleased;
        scrollViewer.PointerCaptureLost += OnPointerReleased;
    }

    private static void DetachEvents(ScrollViewer scrollViewer)
    {
        scrollViewer.PointerPressed -= OnPointerPressed;
        scrollViewer.PointerMoved -= OnPointerMoved;
        scrollViewer.PointerReleased -= OnPointerReleased;
        scrollViewer.PointerCanceled -= OnPointerReleased;
        scrollViewer.PointerCaptureLost -= OnPointerReleased;
    }

    private static void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

        var point = e.GetCurrentPoint(scrollViewer);

        if (point.Properties.IsLeftButtonPressed)
        {
            _isDragging = false; // Not dragging yet until threshold is met
            _startX = point.Position.X;
            _startHorizontalOffset = scrollViewer.HorizontalOffset;
            scrollViewer.CapturePointer(e.Pointer);
            SetIsDragging(scrollViewer, false);
            // Don't mark as handled yet - allow click events to process
        }
    }

    private static void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

        var point = e.GetCurrentPoint(scrollViewer);
        var deltaX = _startX - point.Position.X;

        // Only start dragging if we've moved beyond the threshold
        if (!_isDragging && System.Math.Abs(deltaX) > DragThreshold)
        {
            _isDragging = true;
            SetIsDragging(scrollViewer, true);
        }

        if (_isDragging)
        {
            scrollViewer.ChangeView(_startHorizontalOffset + deltaX, null, null, true);
            e.Handled = true;
        }
    }

    private static void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            var wasDragging = _isDragging;
            _isDragging = false;
            SetIsDragging(scrollViewer, false);
            scrollViewer.ReleasePointerCaptures();
            
            if (wasDragging)
            {
                e.Handled = true; // Prevent click if we were dragging
            }
        }
    }

    #endregion
}