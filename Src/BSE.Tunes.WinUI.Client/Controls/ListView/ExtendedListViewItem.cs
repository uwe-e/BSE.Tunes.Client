using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace BSE.Tunes.WinUI.Client.Controls;

public class ExtendedListViewItem : ListViewItem
{
    private const string PreSelectionCheckName = "PreSelectCheck";
    private FontIcon? _preSelectCheck;

    public static readonly DependencyProperty ParentListViewProperty =
        DependencyProperty.Register(
            nameof(ParentListView),
            typeof(ExtendedListView),
            typeof(ExtendedListViewItem),
            new PropertyMetadata(null));

    public ExtendedListView? ParentListView
    {
        get => (ExtendedListView?)GetValue(ParentListViewProperty);
        set => SetValue(ParentListViewProperty, value);
    }

    public static readonly DependencyProperty EnablePreSelectionProperty =
        DependencyProperty.RegisterAttached(
            nameof(EnablePreSelection),
            typeof(bool),
            typeof(ExtendedListViewItem),
            new PropertyMetadata(false, OnEnablePreSelectionChanged));

    public bool EnablePreSelection
    {
        get => (bool)GetValue(EnablePreSelectionProperty);
        set => SetValue(EnablePreSelectionProperty, value);
    }

    public ExtendedListViewItem()
    {
        DefaultStyleKey = typeof(ExtendedListViewItem);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_preSelectCheck != null)
        {
            _preSelectCheck.Tapped -= OnPreSelectCheckTapped;
            _preSelectCheck.PointerPressed -= OnPreSelectCheckPointerPressed;
        }

        _preSelectCheck = GetTemplateChild(PreSelectionCheckName) as FontIcon;

        if (_preSelectCheck != null)
        {
            _preSelectCheck.Tapped += OnPreSelectCheckTapped;
            _preSelectCheck.PointerPressed += OnPreSelectCheckPointerPressed;
        }
    }

    private static void OnEnablePreSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ExtendedListViewItem lvi)
        {
            VisualStateManager.GoToState(lvi, 
                lvi.EnablePreSelection ? "EnablePreSelection" : "DisablePreSelection", 
                true);
        }
    }

    private void OnPreSelectCheckPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void OnPreSelectCheckTapped(object sender, TappedRoutedEventArgs e)
    {
        ParentListView?.RaisePreSelection(Content);
        e.Handled = true;
    }
}