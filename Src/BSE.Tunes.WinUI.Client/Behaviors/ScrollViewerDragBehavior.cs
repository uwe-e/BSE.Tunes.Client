using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace BSE.Tunes.WinUI.Client.Behaviors;

public static class ScrollViewerDragBehavior
{
    private static bool _isDragging;
    private static double _startHorizontalOffset;
    private static double _startX;

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
    }

    private static void DetachEvents(ScrollViewer scrollViewer)
    {
        scrollViewer.PointerPressed -= OnPointerPressed;
        scrollViewer.PointerMoved -= OnPointerMoved;
        scrollViewer.PointerReleased -= OnPointerReleased;
        scrollViewer.PointerCanceled -= OnPointerReleased;
    }

    private static void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

        var point = e.GetCurrentPoint(scrollViewer);

        if (point.Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _startX = point.Position.X;
            _startHorizontalOffset = scrollViewer.HorizontalOffset;
            scrollViewer.CapturePointer(e.Pointer);
            e.Handled = true;
        }
    }

    private static void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDragging || sender is not ScrollViewer scrollViewer) return;

        var point = e.GetCurrentPoint(scrollViewer);
        var deltaX = _startX - point.Position.X;

        scrollViewer.ChangeView(_startHorizontalOffset + deltaX, null, null, true);
        e.Handled = true;
    }

    private static void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_isDragging && sender is ScrollViewer scrollViewer)
        {
            _isDragging = false;
            scrollViewer.ReleasePointerCaptures();
            e.Handled = true;
        }
    }

    #endregion
}