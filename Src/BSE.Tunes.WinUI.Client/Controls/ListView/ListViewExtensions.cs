using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Core;

namespace BSE.Tunes.WinUI.Client.Controls;

/// <summary>
/// Extension methods and attached properties for the ListViewBase class.
/// </summary>
public static class ListViewExtensions
{
    #region SelectedItems
    /// <summary>
    /// SelectedItems Attached Dependency Property
    /// </summary>
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(IList),
            typeof(ListViewExtensions),
            new PropertyMetadata(null, OnSelectedItemsChanged));

    /// <summary>
    /// Gets the SelectedItems property. This dependency property 
    /// indicates the list of selected items that is synchronized
    /// with the items selected in the ListView.
    /// </summary>
    public static IList? GetSelectedItems(DependencyObject d)
    {
        return (IList?)d.GetValue(SelectedItemsProperty);
    }

    /// <summary>
    /// Sets the SelectedItems property. This dependency property 
    /// indicates the list of selected items that is synchronized
    /// with the items selected in the ListView.
    /// </summary>
    public static void SetSelectedItems(DependencyObject d, IList? value)
    {
        d.SetValue(SelectedItemsProperty, value);
    }

    /// <summary>
    /// Handles changes to the SelectedItems property.
    /// </summary>
    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListViewBase listView)
            return;

        var oldSelectedItems = e.OldValue;
        var newSelectedItems = e.NewValue;

        if (oldSelectedItems != null)
        {
            var handler = GetSelectedItemsHandler(d);
            SetSelectedItemsHandler(d, null);
            handler?.Detach();
        }

        if (newSelectedItems != null)
        {
            // Wait for the ListView to be loaded before attaching
            if (listView.IsLoaded)
            {
                var handler = new ListViewSelectedItemsHandler(listView, newSelectedItems);
                SetSelectedItemsHandler(d, handler);
            }
            else
            {
                void OnLoaded(object sender, RoutedEventArgs args)
                {
                    listView.Loaded -= OnLoaded;
                    var handler = new ListViewSelectedItemsHandler(listView, newSelectedItems);
                    SetSelectedItemsHandler(d, handler);
                }
                listView.Loaded += OnLoaded;
            }
        }
    }
    #endregion

    #region SelectedItemsHandler
    /// <summary>
    /// SelectedItemsHandler Attached Dependency Property
    /// </summary>
    private static readonly DependencyProperty SelectedItemsHandlerProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItemsHandler",
            typeof(ListViewSelectedItemsHandler),
            typeof(ListViewExtensions),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets the SelectedItemsHandler property.
    /// </summary>
    private static ListViewSelectedItemsHandler? GetSelectedItemsHandler(DependencyObject d)
    {
        return (ListViewSelectedItemsHandler?)d.GetValue(SelectedItemsHandlerProperty);
    }

    /// <summary>
    /// Sets the SelectedItemsHandler property.
    /// </summary>
    private static void SetSelectedItemsHandler(DependencyObject d, ListViewSelectedItemsHandler? value)
    {
        d.SetValue(SelectedItemsHandlerProperty, value);
    }
    #endregion

    #region Command
    /// <summary>
    /// Identifies the Command attached dependency property, which enables binding an ICommand to a ListView item for
    /// handling item interactions.
    /// </summary>
    /// <remarks>This attached property allows developers to associate a command with ListView items,
    /// typically to handle item click or selection events using the command pattern. The property is intended for use
    /// in XAML or code to facilitate MVVM scenarios where command binding is preferred over event handlers.</remarks>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(ListViewExtensions),
            new PropertyMetadata(null, OnCommandChanged));

    /// <summary>
    /// gets the Command property. This dependency property
    /// </summary>
    /// <param name="d">The dependency object on which the property is set.</param>
    /// <returns>The ICommand associated with the dependency object.</returns>
    public static ICommand? GetCommand(DependencyObject d)
    {
        return (ICommand?)d.GetValue(CommandProperty);
    }

    /// <summary>
    /// Sets the SelectedItems property. This dependency property 
    /// indicates the list of selected items that is synchronized
    /// with the items selected in the ListView.
    /// </summary>
    public static void SetCommand(DependencyObject d, ICommand? value)
    {
        d.SetValue(CommandProperty, value);
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListViewBase control)
            return;

        // Remove previous handler if any
        control.ItemClick -= OnItemClick;

        if (e.NewValue is ICommand)
        {
            control.ItemClick += OnItemClick;
        }
    }

    private static void OnItemClick(object sender, Microsoft.UI.Xaml.Controls.ItemClickEventArgs e)
    {
        if (sender is not ListViewBase control)
            return;

        if (!IsCtrlKeyPressed() &&
            (control.SelectionMode == ListViewSelectionMode.Single ||
            control.SelectionMode == ListViewSelectionMode.Extended))
        {
            var command = GetCommand(control);
            if (command?.CanExecute(e.ClickedItem) == true)
            {
                command.Execute(e.ClickedItem);
            }
        }
    }

    private static bool IsCtrlKeyPressed()
    {
        var coreWindow = CoreWindow.GetForCurrentThread();
        if (coreWindow is null)
            return false;

        var ctrlState = coreWindow.GetKeyState(VirtualKey.Control);
        return (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
    }

    #endregion
}

public class ListViewSelectedItemsHandler
{
    private ListViewBase? _listView;
    private IList? _boundSelection;
    private readonly NotifyCollectionChangedEventHandler _notifyCollectionChangedHandler;

    public ListViewSelectedItemsHandler(ListViewBase listView, object boundSelection)
    {
        _notifyCollectionChangedHandler = OnBoundSelectionChanged;
        Attach(listView, boundSelection as IList);
    }

    private void Attach(ListViewBase listView, IList? boundSelection)
    {
        if (boundSelection == null)
            return;

        _listView = listView;
        _boundSelection = boundSelection;
        
        // Subscribe to events first
        _listView.SelectionChanged += OnListViewSelectionChanged;
        
        if (_boundSelection is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged += _notifyCollectionChangedHandler;
        }

        // Then sync existing items
        SyncToListView();
    }

    private void SyncToListView()
    {
        if (_listView == null || _boundSelection == null)
            return;

        try
        {
            _listView.SelectedItems.Clear();

            foreach (object item in _boundSelection)
            {
                if (!_listView.SelectedItems.Contains(item))
                {
                    _listView.SelectedItems.Add(item);
                }
            }
        }
        catch
        {
            // Ignore COM exceptions during initial sync
        }
    }

    private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_boundSelection == null || _listView == null)
            return;

        /*
         * All selection modes except ListViewSelectionMode.Multiple should be filtered out
         * to prevent a System.InvalidOperationException exception.
         * 
         * System.InvalidOperationException: Cannot change ObservableCollection during a CollectionChanged event.
         */
        if (_listView.SelectionMode == ListViewSelectionMode.Multiple)
        {
            foreach (var item in e.RemovedItems)
            {
                if (_boundSelection.Contains(item))
                {
                    _boundSelection.Remove(item);
                }
            }

            foreach (var item in e.AddedItems)
            {
                if (!_boundSelection.Contains(item))
                {
                    _boundSelection.Add(item);
                }
            }
        }
        else if (_listView.SelectionMode == ListViewSelectionMode.Extended)
        {
            if (_listView.SelectedItems.Count > 0)
            {
                _listView.SelectedItems.Clear();
            }
        }
    }

    private void OnBoundSelectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_listView == null || _boundSelection == null)
            return;

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            SyncToListView();
            return;
        }

        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (_listView.SelectedItems.Contains(item))
                {
                    _listView.SelectedItems.Remove(item);
                }
            }
        }

        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (!_listView.SelectedItems.Contains(item))
                {
                    _listView.SelectedItems.Add(item);
                }
            }
        }
    }

    internal void Detach()
    {
        if (_listView != null)
        {
            _listView.SelectionChanged -= OnListViewSelectionChanged;
            _listView = null;
        }

        if (_boundSelection is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged -= _notifyCollectionChangedHandler;
        }

        _boundSelection = null;
    }
}