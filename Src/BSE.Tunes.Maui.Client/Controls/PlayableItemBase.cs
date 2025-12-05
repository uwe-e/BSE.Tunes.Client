using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.Controls;

public abstract class PlayableItemBase : ContentView
{
    public static readonly BindableProperty TitleProperty
        = BindableProperty.Create(nameof(Title), typeof(string), typeof(PlayableItemBase), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty SubTitleProperty
        = BindableProperty.Create(nameof(SubTitle), typeof(string), typeof(PlayableItemBase), string.Empty);

    public string SubTitle
    {
        get => (string)GetValue(SubTitleProperty);
        set => SetValue(SubTitleProperty, value);
    }

    public static readonly BindableProperty PlayCommandProperty
        = BindableProperty.Create(
            nameof(PlayCommand),
            typeof(ICommand),
            typeof(PlayableItemBase),
            null,
            propertyChanging: OnPlayCommandChanging,
            propertyChanged: OnPlayCommandChanged);

    public ICommand PlayCommand
    {
        get => (ICommand)GetValue(PlayCommandProperty);
        set => SetValue(PlayCommandProperty, value);
    }

    public static readonly BindableProperty PlayCommandParameterProperty
        = BindableProperty.Create(
            nameof(PlayCommandParameter),
            typeof(object),
            typeof(PlayableItemBase),
            null,
            propertyChanged: OnPlayCommandParameterChanged);

    public object PlayCommandParameter
    {
        get => GetValue(PlayCommandParameterProperty);
        set => SetValue(PlayCommandParameterProperty, value);
    }

    public static readonly BindableProperty OpenFlyoutCommandProperty
        = BindableProperty.Create(
            nameof(OpenFlyoutCommand),
            typeof(ICommand),
            typeof(PlayableItemBase),
            null,
            propertyChanging: OnOpenFlyoutCommandChanging,
            propertyChanged: OnOpenFlyoutCommandChanged);

    public ICommand OpenFlyoutCommand
    {
        get => (ICommand)GetValue(OpenFlyoutCommandProperty);
        set => SetValue(OpenFlyoutCommandProperty, value);
    }

    public static readonly BindableProperty OpenFlyoutCommandParameterProperty
        = BindableProperty.Create(
            nameof(OpenFlyoutCommandParameter),
            typeof(object),
            typeof(PlayableItemBase),
            null,
            propertyChanged: OnOpenFlyoutCommandParameterChanged);

    public object OpenFlyoutCommandParameter
    {
        get => GetValue(OpenFlyoutCommandParameterProperty);
        set => SetValue(OpenFlyoutCommandParameterProperty, value);
    }

    protected void OnFlyoutOpenClicked(object sender, EventArgs e)
    {
        OpenFlyoutCommand?.Execute(OpenFlyoutCommandParameter);
    }

    protected void OnItemTapped(object sender, TappedEventArgs e)
    {
        PlayCommand?.Execute(PlayCommandParameter);
    }

    private static void OnPlayCommandChanging(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue is ICommand oldCmd && bindable is PlayableItemBase item)
        {
            oldCmd.CanExecuteChanged -= item.OnPlayCommandCanExecuteChanged;
        }
    }

    private static void OnPlayCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not PlayableItemBase item)
            return;

        if (newValue is ICommand newCmd)
        {
            newCmd.CanExecuteChanged += item.OnPlayCommandCanExecuteChanged;
            // Trigger an initial evaluation (keeps behavior compatible with prior implementation)
            PlayCommandCanExecuteChanged(item);
        }
    }

    private void OnPlayCommandCanExecuteChanged(object? sender, EventArgs e)
    {
        PlayCommandCanExecuteChanged(this);
    }

    private static void PlayCommandCanExecuteChanged(BindableObject bindable)
    {
        if (bindable is PlayableItemBase control && control.PlayCommand is ICommand cmd)
        {
            // evaluate CanExecute; result intentionally ignored to preserve previous behavior
            // (some command implementations perform side-effects on CanExecute calls).
            _ = cmd.CanExecute(control.PlayCommandParameter);
        }
    }

    private static void OnOpenFlyoutCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not PlayableItemBase item)
            return;

        if (newValue is ICommand newCmd)
        {
            newCmd.CanExecuteChanged += item.OnOpenFlyoutCommandCanExecuteChanged;
            OpenFlyoutCommandCanExecuteChanged(item);
        }
    }

    private static void OnOpenFlyoutCommandChanging(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue is ICommand oldCmd && bindable is PlayableItemBase item)
        {
            oldCmd.CanExecuteChanged -= item.OnOpenFlyoutCommandCanExecuteChanged;
        }
    }

    private static void OpenFlyoutCommandCanExecuteChanged(BindableObject bindable)
    {
        if (bindable is PlayableItemBase control && control.OpenFlyoutCommand is ICommand cmd)
        {
            _ = cmd.CanExecute(control.OpenFlyoutCommandParameter);
        }
    }

    private void OnOpenFlyoutCommandCanExecuteChanged(object? sender, EventArgs e)
    {
        OpenFlyoutCommandCanExecuteChanged(this);
    }

    private static void OnPlayCommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
        => PlayCommandCanExecuteChanged(bindable);

    private static void OnOpenFlyoutCommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
        => OpenFlyoutCommandCanExecuteChanged(bindable);
}