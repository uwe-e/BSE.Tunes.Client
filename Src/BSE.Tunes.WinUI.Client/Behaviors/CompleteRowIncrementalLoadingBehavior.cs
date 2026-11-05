using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System.Windows.Input;

namespace BSE.Tunes.WinUI.Client.Behaviors;

public class CompleteRowIncrementalLoadingBehavior : Behavior<ListView>
{
    private long _itemsSourcePropertyToken;

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(CompleteRowIncrementalLoadingBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ItemWidthProperty =
        DependencyProperty.Register(
            nameof(ItemWidth),
            typeof(double),
            typeof(CompleteRowIncrementalLoadingBehavior),
            new PropertyMetadata(150d));

    public static readonly DependencyProperty ItemRightMarginProperty =
        DependencyProperty.Register(
            nameof(ItemRightMargin),
            typeof(double),
            typeof(CompleteRowIncrementalLoadingBehavior),
            new PropertyMetadata(15d));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public double ItemRightMargin
    {
        get => (double)GetValue(ItemRightMarginProperty);
        set => SetValue(ItemRightMarginProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.SizeChanged += OnSizeChanged;
            _itemsSourcePropertyToken = AssociatedObject.RegisterPropertyChangedCallback(ItemsControl.ItemsSourceProperty, OnItemsSourceChanged);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject != null)
        {
            AssociatedObject.SizeChanged -= OnSizeChanged;
            AssociatedObject.UnregisterPropertyChangedCallback(ItemsControl.ItemsSourceProperty, _itemsSourcePropertyToken);
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Execute();
    }

    private void OnItemsSourceChanged(DependencyObject sender, DependencyProperty property)
    {
        Execute();
    }

    private void Execute()
    {
        if (AssociatedObject == null || ItemWidth <= 0)
        {
            return;
        }

        int visibleCount = CalculateVisibleItemCount(AssociatedObject.ActualWidth, ItemWidth, ItemRightMargin);

        if (Command?.CanExecute(visibleCount) == true)
        {
            Command.Execute(visibleCount);
        }
    }

    private static int CalculateVisibleItemCount(double availableWidth, double itemWidth, double itemRightMargin)
    {
        double itemWidthWithSpacing = itemWidth + itemRightMargin;
        int count = (int)Math.Floor((availableWidth) / itemWidthWithSpacing) ;
        return Math.Max(1, count);
    }
}
