using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

namespace BSE.Tunes.WinUI.Client.Controls;

[TemplatePart(Name = PART_MenuFlyoutItem, Type = typeof(Microsoft.UI.Xaml.Controls.MenuFlyoutItem))]
public sealed class MenuFlyoutItem : Control
{
    private const string PART_MenuFlyoutItem = "PART_MenuFlyoutItem";
    private Microsoft.UI.Xaml.Controls.MenuFlyoutItem? _menuFlyoutItem;

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(MenuFlyoutItem),
            new PropertyMetadata(string.Empty, OnTextChanged));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(IconElement),
            typeof(MenuFlyoutItem),
            new PropertyMetadata(null, OnIconChanged));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(MenuFlyoutItem),
            new PropertyMetadata(null, OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(MenuFlyoutItem),
            new PropertyMetadata(null, OnCommandParameterChanged));

    public static readonly DependencyProperty IsIconEnabledProperty =
        DependencyProperty.Register(
            nameof(IsIconEnabled),
            typeof(bool),
            typeof(MenuFlyoutItem),
            new PropertyMetadata(true, OnIsIconEnabledChanged));

    public MenuFlyoutItem()
    {
        DefaultStyleKey = typeof(MenuFlyoutItem);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IconElement? Icon
    {
        get => (IconElement?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public bool IsIconEnabled
    {
        get => (bool)GetValue(IsIconEnabledProperty);
        set => SetValue(IsIconEnabledProperty, value);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_menuFlyoutItem != null)
        {
            _menuFlyoutItem.Click -= OnMenuFlyoutItemClick;
        }

        _menuFlyoutItem = GetTemplateChild(PART_MenuFlyoutItem) as Microsoft.UI.Xaml.Controls.MenuFlyoutItem;

        if (_menuFlyoutItem != null)
        {
            _menuFlyoutItem.Click += OnMenuFlyoutItemClick;
            UpdateMenuFlyoutItemProperties();
        }
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuFlyoutItem menuFlyoutItem)
        {
            menuFlyoutItem.UpdateMenuFlyoutItemProperties();
        }
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuFlyoutItem menuFlyoutItem)
        {
            menuFlyoutItem.UpdateMenuFlyoutItemProperties();
        }
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuFlyoutItem menuFlyoutItem)
        {
            menuFlyoutItem.UpdateMenuFlyoutItemProperties();
        }
    }

    private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuFlyoutItem menuFlyoutItem)
        {
            menuFlyoutItem.UpdateMenuFlyoutItemProperties();
        }
    }

    private static void OnIsIconEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuFlyoutItem menuFlyoutItem)
        {
            menuFlyoutItem.UpdateMenuFlyoutItemProperties();
        }
    }

    private void UpdateMenuFlyoutItemProperties()
    {
        if (_menuFlyoutItem == null)
            return;

        _menuFlyoutItem.Text = Text;
        _menuFlyoutItem.Icon = IsIconEnabled ? Icon : null;
    }

    private void OnMenuFlyoutItemClick(object sender, RoutedEventArgs e)
    {
        if (Command != null && Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
        }
    }

    internal Microsoft.UI.Xaml.Controls.MenuFlyoutItem? GetInternalMenuFlyoutItem() => _menuFlyoutItem;
}