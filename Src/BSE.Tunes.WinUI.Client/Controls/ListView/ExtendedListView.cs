using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Windows.Input;

namespace BSE.Tunes.WinUI.Client.Controls;

public class ExtendedListView : ListView
{
    public event EventHandler<ItemPreSelectionClickEventArgs>? ItemPreSelectionClick;

    public static readonly DependencyProperty AlternatingRowProperty =
        DependencyProperty.Register(
            nameof(AlternatingRow),
            typeof(Brush),
            typeof(ExtendedListView),
            new PropertyMetadata(null, OnAlternatingRowChanged));

    public Brush? AlternatingRow
    {
        get => (Brush?)GetValue(AlternatingRowProperty);
        set => SetValue(AlternatingRowProperty, value);
    }

    public static readonly DependencyProperty EnablePreSelectionProperty =
        DependencyProperty.RegisterAttached(
            nameof(EnablePreSelection),
            typeof(bool),
            typeof(ExtendedListView),
            new PropertyMetadata(false, OnEnablePreSelectionChanged));

    public bool EnablePreSelection
    {
        get => (bool)GetValue(EnablePreSelectionProperty);
        set => SetValue(EnablePreSelectionProperty, value);
    }

    public static readonly DependencyProperty PreSelectionCommandProperty =
        DependencyProperty.RegisterAttached(
            nameof(PreSelectionCommand),
            typeof(ICommand),
            typeof(ExtendedListView),
            new PropertyMetadata(null));

    public ICommand? PreSelectionCommand
    {
        get => (ICommand?)GetValue(PreSelectionCommandProperty);
        set => SetValue(PreSelectionCommandProperty, value);
    }

    public ExtendedListView()
    {
        //DefaultStyleKey = typeof(ExtendedListView);
    }

    protected override void OnDrop(DragEventArgs e)
    {
        base.OnDrop(e);
        if (AlternatingRow != null)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (ContainerFromIndex(i) is ListViewItem lvi)
                {
                    SetAlternatingBackground(lvi, i);
                }
            }
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ExtendedListViewItem();
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        int index = IndexFromContainer(element);
        SetAlternatingBackground(element, index);

        if (element is ExtendedListViewItem lvi)
        {
            lvi.ParentListView = this;
        }
    }

    protected virtual void OnItemPreSelectionClick(ItemPreSelectionClickEventArgs e)
    {
        PreSelectionCommand?.Execute(e.SelectedItem);
        ItemPreSelectionClick?.Invoke(this, e);
    }

    internal void RaisePreSelection(object selectedItem)
    {
        if (selectedItem != null)
        {
            OnItemPreSelectionClick(new ItemPreSelectionClickEventArgs
            {
                Source = this,
                SelectedItem = selectedItem
            });
        }
    }

    private void SetAlternatingBackground(DependencyObject element, int index)
    {
        if (AlternatingRow != null && element is ListViewItem lvi)
        {
            lvi.Background = index % 2 == 0 ? AlternatingRow : Background;
        }
    }

    private static void OnAlternatingRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ExtendedListView listView)
        {
            listView.AlternatingRow = e.NewValue as Brush;
        }
    }

    private static void OnEnablePreSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ExtendedListView listView)
        {
            listView.ContainerContentChanging += OnListContainerContentChanging;
        }
    }

    private static void OnListContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.ItemContainer is ExtendedListViewItem itemContainer && sender is ExtendedListView listView)
        {
            itemContainer.EnablePreSelection = listView.EnablePreSelection;
        }
    }
}