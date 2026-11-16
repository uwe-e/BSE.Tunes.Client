using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;

namespace BSE.Tunes.WinUI.Client.Extensions;

public static class MenuFlyoutSubItemExtensions
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.RegisterAttached(
            "ItemsSource",
            typeof(IEnumerable),
            typeof(MenuFlyoutSubItemExtensions),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.RegisterAttached(
            "ItemTemplate",
            typeof(DataTemplate),
            typeof(MenuFlyoutSubItemExtensions),
            new PropertyMetadata(null, OnItemTemplateChanged));

    public static readonly DependencyProperty ItemCommandProperty =
        DependencyProperty.RegisterAttached(
            "ItemCommand",
            typeof(ICommand),
            typeof(MenuFlyoutSubItemExtensions),
            new PropertyMetadata(null, OnItemCommandChanged));

    public static readonly DependencyProperty CommandContextProperty =
        DependencyProperty.RegisterAttached(
            "CommandContext",
            typeof(object),
            typeof(MenuFlyoutSubItemExtensions),
            new PropertyMetadata(null, OnCommandContextChanged));

    private static readonly DependencyProperty IsDynamicItemProperty =
        DependencyProperty.RegisterAttached(
            "IsDynamicItem",
            typeof(bool),
            typeof(MenuFlyoutSubItemExtensions),
            new PropertyMetadata(false));

    private static readonly DependencyProperty CollectionHandlerProperty =
        DependencyProperty.RegisterAttached(
            "CollectionHandler",
            typeof(NotifyCollectionChangedEventHandler),
            typeof(MenuFlyoutSubItemExtensions),
            new PropertyMetadata(null));

    private static readonly DependencyProperty UnloadedHandlerProperty =
        DependencyProperty.RegisterAttached(
            "UnloadedHandler",
            typeof(RoutedEventHandler),
            typeof(MenuFlyoutSubItemExtensions),
            new PropertyMetadata(null));

    private static readonly DependencyProperty LoadedHandlerProperty =
        DependencyProperty.RegisterAttached(
            "LoadedHandler",
            typeof(RoutedEventHandler),
            typeof(MenuFlyoutSubItemExtensions),
            new PropertyMetadata(null));

    public static IEnumerable? GetItemsSource(DependencyObject element) =>
        (IEnumerable?)element.GetValue(ItemsSourceProperty);

    public static void SetItemsSource(DependencyObject element, IEnumerable? value) =>
        element.SetValue(ItemsSourceProperty, value);

    public static DataTemplate? GetItemTemplate(DependencyObject element) =>
        (DataTemplate?)element.GetValue(ItemTemplateProperty);

    public static void SetItemTemplate(DependencyObject element, DataTemplate? value) =>
        element.SetValue(ItemTemplateProperty, value);

    public static ICommand? GetItemCommand(DependencyObject obj) =>
        (ICommand?)obj.GetValue(ItemCommandProperty);

    public static void SetItemCommand(DependencyObject obj, ICommand? value) =>
        obj.SetValue(ItemCommandProperty, value);

    public static object? GetCommandContext(DependencyObject obj) =>
        obj.GetValue(CommandContextProperty);

    public static void SetCommandContext(DependencyObject obj, object? value) =>
        obj.SetValue(CommandContextProperty, value);

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MenuFlyoutSubItem subItem)
            return;

        if (e.OldValue is INotifyCollectionChanged oldCollection)
        {
            DetachCollectionHandler(subItem, oldCollection);
        }

        EnsureLifecycleHandlers(subItem);

        if (e.NewValue is INotifyCollectionChanged newCollection)
        {
            AttachCollectionHandler(subItem, newCollection);
        }
        else
        {
            subItem.ClearValue(CollectionHandlerProperty);
        }

        RebuildItems(subItem);
    }

    private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuFlyoutSubItem subItem)
        {
            RebuildItems(subItem);
        }
    }
    
    private static void OnItemCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuFlyoutSubItem subItem)
        {
            if (GetItemTemplate(subItem) == null)
            {
                UpdateDynamicMenuItemsCommand(subItem);
            }
            else
            {
                RebuildItems(subItem);
            }
        }
    }

    private static void OnCommandContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuFlyoutSubItem subItem)
        {
            if (GetItemTemplate(subItem) == null)
            {
                UpdateDynamicMenuItemsCommand(subItem);
            }
            else
            {
                RebuildItems(subItem);
            }
        }
    }

    private static void EnsureLifecycleHandlers(MenuFlyoutSubItem subItem)
    {
        if (subItem.GetValue(UnloadedHandlerProperty) is not RoutedEventHandler unloadedHandler)
        {
            unloadedHandler = (sender, _) =>
            {
                if (sender is MenuFlyoutSubItem item && GetItemsSource(item) is INotifyCollectionChanged collection)
                {
                    DetachCollectionHandler(item, collection);
                }
            };

            subItem.SetValue(UnloadedHandlerProperty, unloadedHandler);
            subItem.Unloaded += unloadedHandler;
        }

        if (subItem.GetValue(LoadedHandlerProperty) is not RoutedEventHandler loadedHandler)
        {
            loadedHandler = (sender, _) =>
            {
                if (sender is MenuFlyoutSubItem item
                    && GetItemsSource(item) is INotifyCollectionChanged collection
                    && item.GetValue(CollectionHandlerProperty) is null)
                {
                    AttachCollectionHandler(item, collection);
                }
            };

            subItem.SetValue(LoadedHandlerProperty, loadedHandler);
            subItem.Loaded += loadedHandler;
        }
    }

    private static void AttachCollectionHandler(MenuFlyoutSubItem subItem, INotifyCollectionChanged collection)
    {
        NotifyCollectionChangedEventHandler handler = (_, args) => OnCollectionChanged(subItem, args);
        subItem.SetValue(CollectionHandlerProperty, handler);
        collection.CollectionChanged += handler;
    }

    private static void DetachCollectionHandler(MenuFlyoutSubItem subItem, INotifyCollectionChanged collection)
    {
        if (subItem.GetValue(CollectionHandlerProperty) is NotifyCollectionChangedEventHandler oldHandler)
        {
            collection.CollectionChanged -= oldHandler;
            subItem.ClearValue(CollectionHandlerProperty);
        }
    }

    private static void OnCollectionChanged(MenuFlyoutSubItem subItem, NotifyCollectionChangedEventArgs args)
    {
        if (GetItemTemplate(subItem) != null)
        {
            RebuildItems(subItem);
            return;
        }

        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                InsertDynamicItems(subItem, args.NewStartingIndex, args.NewItems);
                break;
            case NotifyCollectionChangedAction.Remove:
                RemoveDynamicItems(subItem, args.OldStartingIndex, args.OldItems?.Count ?? 0);
                break;
            case NotifyCollectionChangedAction.Replace:
                RemoveDynamicItems(subItem, args.OldStartingIndex, args.OldItems?.Count ?? 0);
                InsertDynamicItems(subItem, args.NewStartingIndex, args.NewItems);
                break;
            case NotifyCollectionChangedAction.Move:
                MoveDynamicItems(subItem, args.OldStartingIndex, args.NewStartingIndex, args.OldItems?.Count ?? 0);
                break;
            case NotifyCollectionChangedAction.Reset:
            default:
                RebuildItems(subItem);
                break;
        }
    }

    private static void RebuildItems(MenuFlyoutSubItem subItem)
    {
        var items = subItem.Items;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].GetValue(IsDynamicItemProperty) is true)
            {
                items.RemoveAt(i);
            }
        }

        var itemsSource = GetItemsSource(subItem);
        if (itemsSource == null)
            return;

        var itemTemplate = GetItemTemplate(subItem);
        var itemCommand = GetItemCommand(subItem);
        var commandContext = GetCommandContext(subItem);

        foreach (var dataItem in itemsSource)
        {
            if (itemTemplate == null)
            {
                items.Add(CreateDefaultDynamicItem(dataItem, itemCommand, commandContext));
                continue;
            }

            var content = itemTemplate.LoadContent();
            AddTemplateContent(subItem, content, dataItem, itemCommand, commandContext);
        }
    }

    private static MenuFlyoutItem CreateDefaultDynamicItem(object? dataItem, ICommand? itemCommand, object? commandContext)
    {
        var item = new MenuFlyoutItem
        {
            Text = dataItem?.ToString() ?? string.Empty,
            Command = itemCommand,
            CommandParameter = BuildCommandParameter(dataItem, commandContext),
            DataContext = dataItem
        };

        item.SetValue(IsDynamicItemProperty, true);
        return item;
    }

    private static void UpdateDynamicMenuItemsCommand(MenuFlyoutSubItem subItem)
    {
        var itemCommand = GetItemCommand(subItem);
        var commandContext = GetCommandContext(subItem);
        var items = subItem.Items;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].GetValue(IsDynamicItemProperty) is not true || items[i] is not MenuFlyoutItem menuItem)
            {
                continue;
            }

            var dataItem = menuItem.DataContext;
            menuItem.Command = itemCommand;
            menuItem.CommandParameter = BuildCommandParameter(dataItem, commandContext);
        }
    }

    private static void InsertDynamicItems(MenuFlyoutSubItem subItem, int sourceIndex, IList? newItems)
    {
        if (newItems == null || newItems.Count == 0)
        {
            return;
        }

        var items = subItem.Items;
        var dynamicStartIndex = GetDynamicStartIndex(items);
        var dynamicCount = GetDynamicItemCount(items);
        var insertionOffset = sourceIndex < 0 ? dynamicCount : sourceIndex;
        var insertionIndex = dynamicStartIndex + insertionOffset;
        var command = GetItemCommand(subItem);
        var commandContext = GetCommandContext(subItem);

        for (int i = 0; i < newItems.Count; i++)
        {
            var item = CreateDefaultDynamicItem(newItems[i], command, commandContext);
            items.Insert(insertionIndex + i, item);
        }
    }

    private static void RemoveDynamicItems(MenuFlyoutSubItem subItem, int sourceIndex, int count)
    {
        if (count <= 0)
        {
            return;
        }

        var items = subItem.Items;
        var dynamicStartIndex = GetDynamicStartIndex(items);
        var removalOffset = sourceIndex < 0 ? 0 : sourceIndex;
        var removalIndex = dynamicStartIndex + removalOffset;

        for (int i = 0; i < count && removalIndex < items.Count; i++)
        {
            if (items[removalIndex].GetValue(IsDynamicItemProperty) is not true)
            {
                break;
            }

            items.RemoveAt(removalIndex);
        }
    }

    private static void MoveDynamicItems(MenuFlyoutSubItem subItem, int oldSourceIndex, int newSourceIndex, int count)
    {
        if (count <= 0 || oldSourceIndex < 0 || newSourceIndex < 0)
        {
            RebuildItems(subItem);
            return;
        }

        var items = subItem.Items;
        var dynamicStartIndex = GetDynamicStartIndex(items);
        var oldMenuIndex = dynamicStartIndex + oldSourceIndex;
        var newMenuIndex = dynamicStartIndex + newSourceIndex;

        if (oldMenuIndex < 0 || oldMenuIndex >= items.Count)
        {
            RebuildItems(subItem);
            return;
        }

        var movedItems = new List<MenuFlyoutItemBase>(count);
        for (int i = 0; i < count && oldMenuIndex < items.Count; i++)
        {
            if (items[oldMenuIndex].GetValue(IsDynamicItemProperty) is not true)
            {
                RebuildItems(subItem);
                return;
            }

            movedItems.Add(items[oldMenuIndex]);
            items.RemoveAt(oldMenuIndex);
        }

        if (newMenuIndex > oldMenuIndex)
        {
            newMenuIndex -= movedItems.Count;
        }

        for (int i = 0; i < movedItems.Count; i++)
        {
            items.Insert(newMenuIndex + i, movedItems[i]);
        }
    }

    private static int GetDynamicStartIndex(IList<MenuFlyoutItemBase> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].GetValue(IsDynamicItemProperty) is true)
            {
                return i;
            }
        }

        return items.Count;
    }

    private static int GetDynamicItemCount(IList<MenuFlyoutItemBase> items)
    {
        var count = 0;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].GetValue(IsDynamicItemProperty) is true)
            {
                count++;
            }
        }

        return count;
    }

    private static void AddTemplateContent(MenuFlyoutSubItem subItem, object content, object? dataItem, ICommand? itemCommand, object? commandContext)
    {
        if (content is FrameworkElement templateRoot)
        {
            templateRoot.DataContext = dataItem;

            if (templateRoot is Control control)
            {
                control.ApplyTemplate();
            }
        }

        if (content is MenuFlyoutItemBase directItem)
        {
            AddMenuItem(subItem, directItem, dataItem, itemCommand, commandContext);
            return;
        }

        if (content is Controls.MenuFlyoutItem customItem)
        {
            AddCustomMenuFlyoutItem(subItem, customItem, dataItem, itemCommand, commandContext);
            return;
        }

        if (content is Panel panel)
        {
            var children = panel.Children;
            var childCount = children.Count;

            for (int i = 0; i < childCount; i++)
            {
                var child = children[i];

                if (child is MenuFlyoutItemBase childItem)
                {
                    AddMenuItem(subItem, childItem, dataItem, itemCommand, commandContext);
                }
                else if (child is Controls.MenuFlyoutItem customChildItem)
                {
                    AddCustomMenuFlyoutItem(subItem, customChildItem, dataItem, itemCommand, commandContext);
                }
            }
        }
    }

    private static void AddMenuItem(MenuFlyoutSubItem subItem, MenuFlyoutItemBase item, object? dataItem, ICommand? itemCommand, object? commandContext)
    {
        MenuFlyoutItemBase itemToAdd = item;

        if (item is FrameworkElement { Parent: not null })
        {
            itemToAdd = CloneMenuItem(item, dataItem, itemCommand, commandContext);
        }

        if (itemToAdd is MenuFlyoutItem menuFlyoutItem && menuFlyoutItem.Command == null && itemCommand != null)
        {
            menuFlyoutItem.Command = itemCommand;
            menuFlyoutItem.CommandParameter = menuFlyoutItem.CommandParameter ?? BuildCommandParameter(dataItem, commandContext);
        }

        itemToAdd.SetValue(IsDynamicItemProperty, true);
        subItem.Items.Add(itemToAdd);
    }

    private static MenuFlyoutItemBase CloneMenuItem(MenuFlyoutItemBase item, object? dataItem, ICommand? itemCommand, object? commandContext)
    {
        if (item is MenuFlyoutSeparator)
        {
            return new MenuFlyoutSeparator();
        }

        if (item is MenuFlyoutItem menuItem)
        {
            return new MenuFlyoutItem
            {
                Text = menuItem.Text,
                Icon = CloneIcon(menuItem.Icon),
                Command = menuItem.Command ?? itemCommand,
                CommandParameter = menuItem.CommandParameter ?? BuildCommandParameter(dataItem, commandContext)
            };
        }

        return new MenuFlyoutItem
        {
            Text = item.ToString() ?? string.Empty,
            Command = itemCommand,
            CommandParameter = BuildCommandParameter(dataItem, commandContext)
        };
    }

    private static void AddCustomMenuFlyoutItem(MenuFlyoutSubItem subItem, Controls.MenuFlyoutItem customItem, object? dataItem, ICommand? itemCommand, object? commandContext)
    {
        var itemToAdd = new MenuFlyoutItem
        {
            Text = customItem.Text,
            Icon = customItem.IsIconEnabled ? CloneIcon(customItem.Icon) : null,
            Command = customItem.Command ?? itemCommand,
            CommandParameter = customItem.CommandParameter ?? BuildCommandParameter(dataItem, commandContext),
            DataContext = customItem.DataContext ?? dataItem
        };

        itemToAdd.SetValue(IsDynamicItemProperty, true);
        subItem.Items.Add(itemToAdd);
    }

    private static object? BuildCommandParameter(object? menuItemData, object? itemDataContext)
    {
        if (itemDataContext == null)
        {
            return menuItemData;
        }

        return new MenuFlyoutCommandParameter(menuItemData, itemDataContext);
    }

    private static IconElement? CloneIcon(IconElement? icon)
    {
        return icon switch
        {
            FontIcon fontIcon => new FontIcon
            {
                Glyph = fontIcon.Glyph,
                FontFamily = fontIcon.FontFamily,
                FontSize = fontIcon.FontSize,
                FontStyle = fontIcon.FontStyle,
                FontWeight = fontIcon.FontWeight,
                MirroredWhenRightToLeft = fontIcon.MirroredWhenRightToLeft
            },
            SymbolIcon symbolIcon => new SymbolIcon { Symbol = symbolIcon.Symbol },
            BitmapIcon bitmapIcon => new BitmapIcon
            {
                UriSource = bitmapIcon.UriSource,
                ShowAsMonochrome = bitmapIcon.ShowAsMonochrome
            },
            PathIcon pathIcon => new PathIcon
            {
                Data = pathIcon.Data
            },
            null => null,
            _ => null
        };
    }
}

public sealed record MenuFlyoutCommandParameter(object? MenuItem, object? ItemDataContext);


