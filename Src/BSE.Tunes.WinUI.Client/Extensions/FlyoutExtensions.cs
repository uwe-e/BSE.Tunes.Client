using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;

namespace BSE.Tunes.WinUI.Client.Extensions;

public static class FlyoutExtensions
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.RegisterAttached(
            "ItemsSource",
            typeof(IEnumerable),
            typeof(FlyoutExtensions),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.RegisterAttached(
            "ItemTemplate",
            typeof(DataTemplate),
            typeof(FlyoutExtensions),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ItemCommandProperty =
        DependencyProperty.RegisterAttached(
            "ItemCommand",
            typeof(ICommand),
            typeof(FlyoutExtensions),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ParentProperty =
        DependencyProperty.RegisterAttached(
            "Parent",
            typeof(FrameworkElement),
            typeof(FlyoutExtensions),
            new PropertyMetadata(null));

    private static readonly DependencyProperty IsDynamicItemProperty =
        DependencyProperty.RegisterAttached(
            "IsDynamicItem",
            typeof(bool),
            typeof(FlyoutExtensions),
            new PropertyMetadata(false));

    private static readonly DependencyProperty CollectionHandlerProperty =
        DependencyProperty.RegisterAttached(
            "CollectionHandler",
            typeof(NotifyCollectionChangedEventHandler),
            typeof(FlyoutExtensions),
            new PropertyMetadata(null));

    public static IEnumerable GetItemsSource(DependencyObject obj) =>
        (IEnumerable)obj.GetValue(ItemsSourceProperty);

    public static void SetItemsSource(DependencyObject obj, IEnumerable value) =>
        obj.SetValue(ItemsSourceProperty, value);

    public static DataTemplate GetItemTemplate(DependencyObject obj) =>
        (DataTemplate)obj.GetValue(ItemTemplateProperty);

    public static void SetItemTemplate(DependencyObject obj, DataTemplate value) =>
        obj.SetValue(ItemTemplateProperty, value);

    public static ICommand GetItemCommand(DependencyObject obj) =>
        (ICommand)obj.GetValue(ItemCommandProperty);

    public static void SetItemCommand(DependencyObject obj, ICommand value) =>
        obj.SetValue(ItemCommandProperty, value);

    public static FrameworkElement GetParent(DependencyObject obj) =>
        (FrameworkElement)obj.GetValue(ParentProperty);

    public static void SetParent(DependencyObject obj, FrameworkElement value) =>
        obj.SetValue(ParentProperty, value);

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FlyoutBase flyout)
            return;

        // Unsubscribe from old collection
        if (e.OldValue is INotifyCollectionChanged oldCollection)
        {
            var oldHandler = (NotifyCollectionChangedEventHandler)flyout.GetValue(CollectionHandlerProperty);
            if (oldHandler != null)
            {
                oldCollection.CollectionChanged -= oldHandler;
            }
        }

        // Subscribe to new collection
        if (e.NewValue is INotifyCollectionChanged newCollection)
        {
            NotifyCollectionChangedEventHandler handler = (s, args) => UpdateFlyoutItems(flyout);
            flyout.SetValue(CollectionHandlerProperty, handler);
            newCollection.CollectionChanged += handler;
        }

        UpdateFlyoutItems(flyout);
    }

    private static void UpdateFlyoutItems(FlyoutBase flyout)
    {
        var itemsSource = GetItemsSource(flyout);
        var itemTemplate = GetItemTemplate(flyout);
        var itemCommand = GetItemCommand(flyout);

        if (flyout is Flyout standardFlyout)
        {
            UpdateStandardFlyout(standardFlyout, itemsSource, itemTemplate, itemCommand);
        }
        else if (flyout is MenuFlyout menuFlyout)
        {
            UpdateMenuFlyout(menuFlyout, itemsSource, itemTemplate, itemCommand);
        }
    }

    private static void UpdateStandardFlyout(Flyout flyout, IEnumerable? itemsSource, DataTemplate? itemTemplate, ICommand? itemCommand)
    {
        if (itemsSource == null || itemTemplate == null)
            return;

        var stackPanel = new StackPanel();

        foreach (var dataItem in itemsSource)
        {
            var content = itemTemplate.LoadContent();

            if (content is FrameworkElement element)
            {
                element.DataContext = dataItem;

                ProcessElementForCustomMenuFlyoutItem(element, dataItem, itemCommand, flyout);

                stackPanel.Children.Add(element);
            }
        }

        flyout.Content = stackPanel;
    }

    private static void ConfigureCustomMenuFlyoutItem(Controls.MenuFlyoutItem customItem, object dataItem, ICommand? itemCommand, FlyoutBase? flyout = null)
    {
        // If no command is set on the custom item, use the shared command
        if (customItem.Command == null && itemCommand != null)
        {
            customItem.Command = itemCommand;
            customItem.CommandParameter = dataItem;
        }

        // Apply template to ensure internal item is created
        customItem.ApplyTemplate();

        var internalChildItem = customItem.GetInternalMenuFlyoutItem();
        if (internalChildItem != null && flyout != null)
        {
            internalChildItem.Click += (s, e) => flyout.Hide();
        }
    }

    private static void ProcessElementForCustomMenuFlyoutItem(FrameworkElement element, object dataItem, ICommand? itemCommand, FlyoutBase? flyout = null)
    {
        if (element is Controls.MenuFlyoutItem customItem)
        {
            ConfigureCustomMenuFlyoutItem(customItem, dataItem, itemCommand, flyout);
        }
        else if (element is Panel panel)
        {
            var children = panel.Children;
            var count = children.Count;
            
            // Search for MenuFlyoutItem in panel children using index-based access
            for (int i = 0; i < count; i++)
            {
                if (children[i] is Controls.MenuFlyoutItem customChildItem)
                {
                    ConfigureCustomMenuFlyoutItem(customChildItem, dataItem, itemCommand, flyout);
                }
            }
        }
    }

    private static void UpdateMenuFlyout(MenuFlyout menuFlyout, IEnumerable? itemsSource, DataTemplate? itemTemplate, ICommand? itemCommand)
    {
        if (itemsSource == null)
            return;

        // Clear existing dynamic items in reverse order
        var items = menuFlyout.Items;
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].GetValue(IsDynamicItemProperty) is true)
            {
                items.RemoveAt(i);
            }
        }

        if (itemTemplate == null)
            return;

        // Add new items from template
        foreach (var dataItem in itemsSource)
        {
            var content = itemTemplate.LoadContent();

            if (content is FrameworkElement element)
            {
                element.DataContext = dataItem;

                // Apply template to ensure visual tree is created
                if (element is Control control)
                {
                    control.ApplyTemplate();
                }

                var menuItem = ExtractMenuFlyoutItem(element, dataItem, itemCommand);

                if (menuItem != null)
                {
                    menuItem.SetValue(IsDynamicItemProperty, true);
                    items.Add(menuItem);
                }
            }
        }
    }

    private static MenuFlyoutItemBase? ExtractMenuFlyoutItem(FrameworkElement element, object dataItem, ICommand? sharedCommand)
    {
        // Direct MenuFlyoutItem or MenuFlyoutSeparator
        if (element is MenuFlyoutItemBase directItem)
        {
            return directItem;
        }

        // Custom MenuFlyoutItem control
        if (element is Controls.MenuFlyoutItem customItem)
        {
            return ExtractFromCustomItem(customItem, dataItem, sharedCommand);
        }

        // Search in Panel children (Grid, StackPanel, etc.)
        if (element is Panel panel)
        {
            var children = panel.Children;
            var count = children.Count;
            
            for (int i = 0; i < count; i++)
            {
                var child = children[i];
                
                if (child is MenuFlyoutItemBase builtInItem)
                {
                    return builtInItem;
                }

                if (child is Controls.MenuFlyoutItem customChildItem)
                {
                    return ExtractFromCustomItem(customChildItem, dataItem, sharedCommand);
                }
            }
        }

        return null;
    }

    private static MenuFlyoutItemBase? ExtractFromCustomItem(Controls.MenuFlyoutItem customItem, object dataItem, ICommand? sharedCommand)
    {
        customItem.ApplyTemplate();
        var internalItem = customItem.GetInternalMenuFlyoutItem();
        
        if (internalItem == null)
            return null;

        internalItem.DataContext = customItem.DataContext;

        // If no command is set on the custom item, use the shared command
        if (customItem.Command == null && sharedCommand != null)
        {
            internalItem.Command = sharedCommand;
            internalItem.CommandParameter = dataItem;
        }
        else if (customItem.Command != null)
        {
            internalItem.Command = customItem.Command;
            internalItem.CommandParameter = customItem.CommandParameter ?? dataItem;
        }
        
        return internalItem;
    }
}