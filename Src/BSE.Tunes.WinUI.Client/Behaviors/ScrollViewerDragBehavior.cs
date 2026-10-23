using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace BSE.Tunes.WinUI.Client.Behaviors;

public static class ScrollViewerDragBehavior
{
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
                scrollViewer.Unloaded += OnScrollViewerUnloaded;
            }
            else
            {
                scrollViewer.Loaded -= OnScrollViewerLoaded;
                scrollViewer.Unloaded -= OnScrollViewerUnloaded;
                DetachEvents(scrollViewer);
                ClearDragState(scrollViewer);
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

    #region Private State Attached Properties

    private static bool GetIsPointerPressed(DependencyObject obj)
        => (bool)obj.GetValue(IsPointerPressedProperty);

    private static void SetIsPointerPressed(DependencyObject obj, bool value)
        => obj.SetValue(IsPointerPressedProperty, value);

    private static readonly DependencyProperty IsPointerPressedProperty =
        DependencyProperty.RegisterAttached(
            "IsPointerPressed",
            typeof(bool),
            typeof(ScrollViewerDragBehavior),
            new PropertyMetadata(false));

    private static double GetStartX(DependencyObject obj)
        => (double)obj.GetValue(StartXProperty);

    private static void SetStartX(DependencyObject obj, double value)
        => obj.SetValue(StartXProperty, value);

    private static readonly DependencyProperty StartXProperty =
        DependencyProperty.RegisterAttached(
            "StartX",
            typeof(double),
            typeof(ScrollViewerDragBehavior),
            new PropertyMetadata(0.0));

    private static double GetStartHorizontalOffset(DependencyObject obj)
        => (double)obj.GetValue(StartHorizontalOffsetProperty);

    private static void SetStartHorizontalOffset(DependencyObject obj, double value)
        => obj.SetValue(StartHorizontalOffsetProperty, value);

    private static readonly DependencyProperty StartHorizontalOffsetProperty =
        DependencyProperty.RegisterAttached(
            "StartHorizontalOffset",
            typeof(double),
            typeof(ScrollViewerDragBehavior),
            new PropertyMetadata(0.0));

    #endregion

    #region Event Handlers

    private static void OnScrollViewerLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            AttachEvents(scrollViewer);
        }
    }

    private static void OnScrollViewerUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            DetachEvents(scrollViewer);
            ClearDragState(scrollViewer);
        }
    }

    private static void AttachEvents(ScrollViewer scrollViewer)
    {
        scrollViewer.PointerPressed += OnPointerPressed;
        scrollViewer.PointerMoved += OnPointerMoved;
        scrollViewer.PointerReleased += OnPointerReleased;
        scrollViewer.PointerCanceled += OnPointerCanceled;
        scrollViewer.PointerCaptureLost += OnPointerCaptureLost;
        scrollViewer.PointerExited += OnPointerExited;
    }

    private static void DetachEvents(ScrollViewer scrollViewer)
    {
        scrollViewer.PointerPressed -= OnPointerPressed;
        scrollViewer.PointerMoved -= OnPointerMoved;
        scrollViewer.PointerReleased -= OnPointerReleased;
        scrollViewer.PointerCanceled -= OnPointerCanceled;
        scrollViewer.PointerCaptureLost -= OnPointerCaptureLost;
        scrollViewer.PointerExited -= OnPointerExited;
    }

    private static void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

        var point = e.GetCurrentPoint(scrollViewer);

        if (point.Properties.IsLeftButtonPressed)
        {
            SetIsPointerPressed(scrollViewer, true);
            SetIsDragging(scrollViewer, false);
            SetStartX(scrollViewer, point.Position.X);
            SetStartHorizontalOffset(scrollViewer, scrollViewer.HorizontalOffset);
            
            scrollViewer.CapturePointer(e.Pointer);
            // Don't mark as handled yet - allow click events to process
        }
    }

    private static void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;
        if (!GetIsPointerPressed(scrollViewer)) return;

        var point = e.GetCurrentPoint(scrollViewer);
        var startX = GetStartX(scrollViewer);
        var deltaX = startX - point.Position.X;
        var isDragging = GetIsDragging(scrollViewer);

        // Only start dragging if we've moved beyond the threshold
        if (!isDragging && System.Math.Abs(deltaX) > DragThreshold)
        {
            SetIsDragging(scrollViewer, true);
            isDragging = true;
        }

        if (isDragging)
        {
            var startOffset = GetStartHorizontalOffset(scrollViewer);
            scrollViewer.ChangeView(startOffset + deltaX, null, null, true);
            e.Handled = true;
        }
    }

    private static void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

        var wasDragging = GetIsDragging(scrollViewer);
        ResetDragState(scrollViewer, e);

        if (wasDragging)
        {
            e.Handled = true; // Prevent click if we were dragging
        }
    }

    private static void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            ResetDragState(scrollViewer, e);
        }
    }

    private static void OnPointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            ResetDragState(scrollViewer, e);
        }
    }

    private static void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        // Don't reset on exit - user might drag back in
        // Only reset when pointer is actually released or canceled
    }

    private static void ResetDragState(ScrollViewer scrollViewer, PointerRoutedEventArgs e)
    {
        SetIsPointerPressed(scrollViewer, false);
        SetIsDragging(scrollViewer, false);
        scrollViewer.ReleasePointerCaptures();
    }

    private static void ClearDragState(ScrollViewer scrollViewer)
    {
        SetIsPointerPressed(scrollViewer, false);
        SetIsDragging(scrollViewer, false);
        SetStartX(scrollViewer, 0.0);
        SetStartHorizontalOffset(scrollViewer, 0.0);
    }

    #endregion
}