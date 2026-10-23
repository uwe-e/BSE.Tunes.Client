using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Windows.Input;
using System.Collections;
using BSE.Tunes.WinUI.Client.Models;

namespace BSE.Tunes.WinUI.Client.Controls;

public sealed partial class ItemsCarousel : UserControl
{
    public ItemsCarousel()
    {
        this.InitializeComponent();
        UpdateEffectiveItemTemplate();
        UpdateItemDimensions();
    }

    public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
    DependencyProperty.Register(
        nameof(HorizontalScrollBarVisibility),
        typeof(ScrollBarVisibility),
        typeof(ItemsCarousel),
        new PropertyMetadata(ScrollBarVisibility.Auto));

    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    #region ItemsSource
    public object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(object),
            typeof(ItemsCarousel),
            new PropertyMetadata(null));
    #endregion

    #region ItemTemplate
    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(ItemsCarousel),
            new PropertyMetadata(null, OnItemTemplateChanged));

    private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemsCarousel control)
        {
            control.UpdateEffectiveItemTemplate();
        }
    }
    #endregion

    #region EffectiveItemTemplate (Internal)
    public DataTemplate EffectiveItemTemplate
    {
        get => (DataTemplate)GetValue(EffectiveItemTemplateProperty);
        private set => SetValue(EffectiveItemTemplateProperty, value);
    }

    public static readonly DependencyProperty EffectiveItemTemplateProperty =
        DependencyProperty.Register(
            nameof(EffectiveItemTemplate),
            typeof(DataTemplate),
            typeof(ItemsCarousel),
            new PropertyMetadata(null));

    private void UpdateEffectiveItemTemplate()
    {
        EffectiveItemTemplate = ItemTemplate ?? (DataTemplate)Resources["DefaultItemTemplate"];
    }
    #endregion

    #region SelectedItem
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(ItemsCarousel),
            new PropertyMetadata(null, OnSelectedItemChanged));

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemsCarousel control)
        {
            control.ItemSelected?.Invoke(control, new ItemSelectedEventArgs(e.NewValue));
        }
    }
    #endregion

    #region ItemClickCommand
    public ICommand ItemClickCommand
    {
        get => (ICommand)GetValue(ItemClickCommandProperty);
        set => SetValue(ItemClickCommandProperty, value);
    }

    public static readonly DependencyProperty ItemClickCommandProperty =
        DependencyProperty.Register(
            nameof(ItemClickCommand),
            typeof(ICommand),
            typeof(ItemsCarousel),
            new PropertyMetadata(null));
    #endregion

    #region ItemWidth
    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public static readonly DependencyProperty ItemWidthProperty =
        DependencyProperty.Register(
            nameof(ItemWidth),
            typeof(double),
            typeof(ItemsCarousel),
            new PropertyMetadata(300.0));
    #endregion

    #region ItemHeight
    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public static readonly DependencyProperty ItemHeightProperty =
        DependencyProperty.Register(
            nameof(ItemHeight),
            typeof(double),
            typeof(ItemsCarousel),
            new PropertyMetadata(300.0));
    #endregion

    #region ItemSpacing
    public double ItemSpacing
    {
        get => (double)GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public static readonly DependencyProperty ItemSpacingProperty =
        DependencyProperty.Register(
            nameof(ItemSpacing),
            typeof(double),
            typeof(ItemsCarousel),
            new PropertyMetadata(12.0));
    #endregion

    #region AspectRatio
    public double AspectRatio
    {
        get => (double)GetValue(AspectRatioProperty);
        set => SetValue(AspectRatioProperty, value);
    }

    public static readonly DependencyProperty AspectRatioProperty =
        DependencyProperty.Register(
            nameof(AspectRatio),
            typeof(double),
            typeof(ItemsCarousel),
            new PropertyMetadata(1.0, OnDimensionPropertyChanged));
    #endregion

    #region CarouselHeight
    public double CarouselHeight
    {
        get => (double)GetValue(CarouselHeightProperty);
        set => SetValue(CarouselHeightProperty, value);
    }

    public static readonly DependencyProperty CarouselHeightProperty =
        DependencyProperty.Register(
            nameof(CarouselHeight),
            typeof(double),
            typeof(ItemsCarousel),
            new PropertyMetadata(300.0, OnDimensionPropertyChanged));

    private static void OnDimensionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemsCarousel control)
        {
            control.UpdateItemDimensions();
        }
    }

    private void UpdateItemDimensions()
    {
        ItemHeight = CarouselHeight;
        ItemWidth = CarouselHeight * AspectRatio;
    }
    #endregion

    #region IsBusy
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public static readonly DependencyProperty IsBusyProperty =
        DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(ItemsCarousel),
            new PropertyMetadata(false, OnIsBusyChanged));

    private static void OnIsBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemsCarousel control)
        {
            control.UpdateLoadingVisibility();
        }
    }
    #endregion

    #region ShowLoadingIndicator (Internal)
    public Visibility ShowLoadingIndicator
    {
        get => (Visibility)GetValue(ShowLoadingIndicatorProperty);
        private set => SetValue(ShowLoadingIndicatorProperty, value);
    }

    public static readonly DependencyProperty ShowLoadingIndicatorProperty =
        DependencyProperty.Register(
            nameof(ShowLoadingIndicator),
            typeof(Visibility),
            typeof(ItemsCarousel),
            new PropertyMetadata(Visibility.Collapsed));

    private void UpdateLoadingVisibility()
    {
        ShowLoadingIndicator = IsBusy ? Visibility.Visible : Visibility.Collapsed;
    }
    #endregion

    #region Events

    public event EventHandler<ItemSelectedEventArgs>? ItemSelected;
    public event EventHandler<ItemClickEventArgs>? ItemClick;

    #endregion

    #region Event Handlers

    private void OnItemsRepeaterElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs e)
    {
        if (e.Element is FrameworkElement element)
        {
            // Get the item from ItemsSource using the index
            var item = GetItemAtIndex(e.Index);
            
            if (item != null)
            {
                // Set DataContext for the element (works with any template)
                element.DataContext = item;
                
                // Store the item in Tag as backup
                element.Tag = item;
            }
            
            // Attach tap handler
            element.Tapped -= OnItemTapped;
            element.Tapped += OnItemTapped;
        }
    }

    private object? GetItemAtIndex(int index)
    {
        if (ItemsSource == null || index < 0)
        {
            return null;
        }

        // Handle IList (most common)
        if (ItemsSource is IList list)
        {
            return index < list.Count ? list[index] : null;
        }

        // Handle IEnumerable
        if (ItemsSource is IEnumerable enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            try
            {
                for (int i = 0; i <= index; i++)
                {
                    if (!enumerator.MoveNext())
                    {
                        return null;
                    }
                }
                return enumerator.Current;
            }
            finally
            {
                if (enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        return null;
    }

    private void OnItemTapped(object sender, TappedRoutedEventArgs e)
    {
        // Check if we're in dragging mode from the ScrollViewer
        if (Behaviors.ScrollViewerDragBehavior.GetIsDragging(ScrollViewerControl))
        {
            return; // Ignore taps during drag
        }

        if (sender is not FrameworkElement element)
        {
            return;
        }

        // Try to get item from DataContext first, then fallback to Tag
        var item = element.DataContext ?? element.Tag;

        if (item != null)
        {
            // Update selected item
            SelectedItem = item;

            // Raise click event only if it's a CarouselItem
            if (item is CarouselItem carouselItem)
            {
                var clickArgs = new ItemClickEventArgs(carouselItem);
                ItemClick?.Invoke(this, clickArgs);
            }

            // Execute command with the actual item
            if (ItemClickCommand?.CanExecute(item) == true)
            {
                ItemClickCommand.Execute(item);
            }

            e.Handled = true;
        }
    }

    #endregion
}

#region Event Arguments

public class ItemSelectedEventArgs : EventArgs
{
    public object? SelectedItem { get; }

    public ItemSelectedEventArgs(object? selectedItem)
    {
        SelectedItem = selectedItem;
    }
}

public class ItemClickEventArgs : EventArgs
{
    public CarouselItem ClickedItem { get; }

    public ItemClickEventArgs(CarouselItem clickedItem)
    {
        ClickedItem = clickedItem ?? throw new ArgumentNullException(nameof(clickedItem));
    }
}

#endregion