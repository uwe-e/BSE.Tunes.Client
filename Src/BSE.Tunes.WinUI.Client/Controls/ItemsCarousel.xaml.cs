using BSE.Tunes.WinUI.Client.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Windows.Input;

namespace BSE.Tunes.WinUI.Client.Controls;

public sealed partial class ItemsCarousel : UserControl
{
    public ItemsCarousel()
    {
        this.InitializeComponent();
        UpdateEffectiveItemTemplate();
        UpdateItemDimensions();
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

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is CarouselItem item)
        {
            ItemClickCommand?.Execute(item);
        }
    }

    //private void OnItemsRepeaterElementPrepared(object sender, ItemsRepeaterElementPreparedEventArgs e)
    //{
    //    if (e.Element is FrameworkElement element)
    //    {
    //        element.Tapped -= OnItemsRepeaterElementTapped;
    //        element.Tapped += OnItemsRepeaterElementTapped;
    //    }
    //}

    //private void OnItemsRepeaterElementTapped(object sender, TappedRoutedEventArgs e)
    //{
    //    if (sender is FrameworkElement element && element.DataContext is CarouselItem item)
    //    {
    //        ItemClickCommand?.Execute(item);
    //        //ItemClick?.Invoke(this, new ItemClickEventArgs(item));
    //    }
    //}
    #endregion
}