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

    public static IEnumerable? GetItemsSource(DependencyObject element) =>
        (IEnumerable?)element.GetValue(ItemsSourceProperty);

    public static void SetItemsSource(DependencyObject element, IEnumerable? value) =>
        element.SetValue(ItemsSourceProperty, value);

    public static DataTemplate? GetItemTemplate(DependencyObject element) =>
        (DataTemplate?)element.GetValue(ItemTemplateProperty);

    public static void SetItemTemplate(DependencyObject element, DataTemplate? value) =>
        element.SetValue(ItemTemplateProperty, value);

    public static ICommand GetItemCommand(DependencyObject obj) =>
        (ICommand)obj.GetValue(ItemCommandProperty);

    public static void SetItemCommand(DependencyObject obj, ICommand value) =>
        obj.SetValue(ItemCommandProperty, value);

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MenuFlyoutSubItem subItem)
            return;

        if (e.OldValue is INotifyCollectionChanged oldCollection)
        {
            var oldHandler = (NotifyCollectionChangedEventHandler?)subItem.GetValue(CollectionHandlerProperty);
            if (oldHandler != null)
            {
                oldCollection.CollectionChanged -= oldHandler;
            }
        }

        if (e.NewValue is INotifyCollectionChanged newCollection)
        {
            NotifyCollectionChangedEventHandler handler = (_, _) => RebuildItems(subItem);
            subItem.SetValue(CollectionHandlerProperty, handler);
            newCollection.CollectionChanged += handler;
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
            RebuildItems(subItem);
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

        foreach (var dataItem in itemsSource)
        {
            if (itemTemplate == null)
            {
                var defaultItem = new MenuFlyoutItem
                {
                    Text = dataItem?.ToString() ?? string.Empty,
                    Command = itemCommand,
                    CommandParameter = dataItem
                };
                defaultItem.SetValue(IsDynamicItemProperty, true);
                items.Add(defaultItem);
                continue;
            }

            var content = itemTemplate.LoadContent();
            AddTemplateContent(subItem, content, dataItem, itemCommand);
        }
    }

    private static void AddTemplateContent(MenuFlyoutSubItem subItem, object content, object? dataItem, ICommand? itemCommand)
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
            AddMenuItem(subItem, directItem, dataItem, itemCommand);
            return;
        }

        if (content is Controls.MenuFlyoutItem customItem)
        {
            AddCustomMenuFlyoutItem(subItem, customItem, dataItem, itemCommand);
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
                    AddMenuItem(subItem, childItem, dataItem, itemCommand);
                }
                else if (child is Controls.MenuFlyoutItem customChildItem)
                {
                    AddCustomMenuFlyoutItem(subItem, customChildItem, dataItem, itemCommand);
                }
            }
        }
    }

    private static void AddMenuItem(MenuFlyoutSubItem subItem, MenuFlyoutItemBase item, object? dataItem, ICommand? itemCommand)
    {
        MenuFlyoutItemBase itemToAdd = item;

        if (item is FrameworkElement { Parent: not null })
        {
            itemToAdd = CloneMenuItem(item, dataItem, itemCommand);
        }

        if (itemToAdd is MenuFlyoutItem menuFlyoutItem && menuFlyoutItem.Command == null && itemCommand != null)
        {
            menuFlyoutItem.Command = itemCommand;
            menuFlyoutItem.CommandParameter = menuFlyoutItem.CommandParameter ?? dataItem;
        }

        itemToAdd.SetValue(IsDynamicItemProperty, true);
        subItem.Items.Add(itemToAdd);
    }

    private static MenuFlyoutItemBase CloneMenuItem(MenuFlyoutItemBase item, object? dataItem, ICommand? itemCommand)
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
                CommandParameter = menuItem.CommandParameter ?? dataItem
            };
        }

        return new MenuFlyoutItem
        {
            Text = item.ToString() ?? string.Empty,
            Command = itemCommand,
            CommandParameter = dataItem
        };
    }

    private static void AddCustomMenuFlyoutItem(MenuFlyoutSubItem subItem, Controls.MenuFlyoutItem customItem, object? dataItem, ICommand? itemCommand)
    {
        var itemToAdd = new MenuFlyoutItem
        {
            Text = customItem.Text,
            Icon = customItem.IsIconEnabled ? CloneIcon(customItem.Icon) : null,
            Command = customItem.Command ?? itemCommand,
            CommandParameter = customItem.CommandParameter ?? dataItem,
            DataContext = customItem.DataContext ?? dataItem
        };

        itemToAdd.SetValue(IsDynamicItemProperty, true);
        subItem.Items.Add(itemToAdd);
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


