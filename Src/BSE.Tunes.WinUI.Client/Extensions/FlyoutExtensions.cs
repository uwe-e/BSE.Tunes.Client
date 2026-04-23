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

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.RegisterAttached(
            "IsOpen",
            typeof(bool),
            typeof(FlyoutExtensions),
            new PropertyMetadata(false, OnIsOpenChanged));

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

    public static bool GetIsOpen(DependencyObject obj) =>
        (bool)obj.GetValue(IsOpenProperty);

    public static void SetIsOpen(DependencyObject obj, bool value) =>
        obj.SetValue(IsOpenProperty, value);

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
            oldCollection.CollectionChanged -= (s, args) => UpdateFlyoutItems(flyout);
        }

        // Subscribe to new collection
        if (e.NewValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += (s, args) => UpdateFlyoutItems(flyout);
        }

        UpdateFlyoutItems(flyout);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FlyoutBase flyout)
            return;

        bool isOpen = (bool)e.NewValue;
        var parent = GetParent(flyout);

        if (isOpen && parent != null)
        {
            flyout.ShowAt(parent);
        }
        else if (!isOpen)
        {
            flyout.Hide();
        }

        // Subscribe to flyout closing to update binding
        flyout.Closed -= OnFlyoutClosed;
        flyout.Closed += OnFlyoutClosed;
    }

    private static void OnFlyoutClosed(object? sender, object e)
    {
        if (sender is FlyoutBase flyout)
        {
            SetIsOpen(flyout, false);
        }
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

                // Apply template to ensure visual tree is created
                if (element is Control control)
                {
                    control.ApplyTemplate();
                }

                // If the element contains a custom MenuFlyoutItem, extract and configure it
                if (element is Controls.MenuFlyoutItem customItem)
                {
                    // If no command is set on the custom item, use the shared command
                    if (customItem.Command == null && itemCommand != null)
                    {
                        customItem.Command = itemCommand;
                        customItem.CommandParameter = dataItem;
                    }
                }
                else if (element is Panel panel)
                {
                    // Search for MenuFlyoutItem in panel children
                    foreach (var child in panel.Children)
                    {
                        if (child is Controls.MenuFlyoutItem customChildItem)
                        {
                            if (customChildItem.Command == null && itemCommand != null)
                            {
                                customChildItem.Command = itemCommand;
                                customChildItem.CommandParameter = dataItem;
                            }
                        }
                    }
                }

                stackPanel.Children.Add(element);
            }
        }

        flyout.Content = stackPanel;
    }

    private static void UpdateMenuFlyout(MenuFlyout menuFlyout, IEnumerable? itemsSource, DataTemplate? itemTemplate, ICommand? itemCommand)
    {
        if (itemsSource == null)
            return;

        // Clear existing dynamic items
        var dynamicItems = menuFlyout.Items
            .Where(item => item.GetValue(IsDynamicItemProperty) is true)
            .ToList();

        foreach (var item in dynamicItems)
        {
            menuFlyout.Items.Remove(item);
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
                    menuFlyout.Items.Add(menuItem);
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
            customItem.ApplyTemplate();
            var internalItem = customItem.GetInternalMenuFlyoutItem();
            if (internalItem != null)
            {
                // Copy DataContext and Tag
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

        // Search in Panel children (Grid, StackPanel, etc.)
        if (element is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is MenuFlyoutItemBase builtInItem)
                {
                    return builtInItem;
                }

                if (child is Controls.MenuFlyoutItem customChildItem)
                {
                    customChildItem.ApplyTemplate();
                    var internalItem = customChildItem.GetInternalMenuFlyoutItem();
                    if (internalItem != null)
                    {
                        internalItem.DataContext = customChildItem.DataContext;

                        // If no command is set on the custom item, use the shared command
                        if (customChildItem.Command == null && sharedCommand != null)
                        {
                            internalItem.Command = sharedCommand;
                            internalItem.CommandParameter = dataItem;
                        }
                        else if (customChildItem.Command != null)
                        {
                            internalItem.Command = customChildItem.Command;
                            internalItem.CommandParameter = customChildItem.CommandParameter ?? dataItem;
                        }
                        return internalItem;
                    }
                }
            }
        }

        return null;
    }
}