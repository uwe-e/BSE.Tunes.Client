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
                nameof(PlayableItemBase),
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
            nameof(PlayableItemBase),
            typeof(object),
            typeof(PlayableItemBase),
            null,
            propertyChanged: (bindable, oldvalue, newvalue) => PlayCommandCanExecuteChanged(bindable, EventArgs.Empty));

    public object PlayCommandParameter
    {
        get => GetValue(PlayCommandParameterProperty);
        set => SetValue(PlayCommandParameterProperty, value);
    }

    public static readonly BindableProperty OpenFlyoutCommandProperty
            = BindableProperty.Create(
                nameof(PlayableItemBase),
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
            nameof(PlayableItemBase),
            typeof(object),
            typeof(PlayableItemBase),
            null,
            propertyChanged: (bindable, oldvalue, newvalue) => OpenFlyoutCommandCanExecuteChanged(bindable, EventArgs.Empty));

    public object OpenFlyoutCommandParameter
    {
        get => GetValue(OpenFlyoutCommandParameterProperty);
        set => SetValue(OpenFlyoutCommandParameterProperty, value);
    }
    
    private void OnFlyoutOpenClicked(object sender, EventArgs e)
    {
        OpenFlyoutCommand?.Execute(OpenFlyoutCommandParameter);
    }

    private void OnItemTapped(object sender, TappedEventArgs e)
    {
        PlayCommand?.Execute(PlayCommandParameter);
    }

    private static void OnPlayCommandChanging(BindableObject bindable, object oldValue, object newValue)
    {
        PlayableItemBase item = (PlayableItemBase)bindable;
        if (oldValue != null)
        {
            (oldValue as ICommand).CanExecuteChanged -= item.OnPlayCommandCanExecuteChanged;
        }
    }

    private static void OnPlayCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        PlayableItemBase item = (PlayableItemBase)bindable;
        if (newValue is ICommand newCommand)
        {
            newCommand.CanExecuteChanged += item.OnPlayCommandCanExecuteChanged;
        }
        if (item.PlayCommand != null)
        {
            PlayCommandCanExecuteChanged(item, EventArgs.Empty);
        }
    }

    private void OnPlayCommandCanExecuteChanged(object sender, EventArgs e)
    {
        PlayableItemBase.PlayCommandCanExecuteChanged(this, EventArgs.Empty);
    }

    private static void PlayCommandCanExecuteChanged(BindableObject bindable, EventArgs empty)
    {
        if (bindable is PlayableItemBase control)
        {
            control.PlayCommand?.CanExecute(control.PlayCommandParameter);
        }
    }

    private static void OnOpenFlyoutCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        PlayableItemBase item = (PlayableItemBase)bindable;
        if (newValue is ICommand newCommand)
        {
            newCommand.CanExecuteChanged += item.OnOpenFlyoutCommandCanExecuteChanged;
        }
        if (item.PlayCommand != null)
        {
            OpenFlyoutCommandCanExecuteChanged(item, EventArgs.Empty);
        }
    }

    private static void OnOpenFlyoutCommandChanging(BindableObject bindable, object oldValue, object newValue)
    {
        PlayableItemBase item = (PlayableItemBase)bindable;
        if (oldValue != null)
        {
            (oldValue as ICommand).CanExecuteChanged -= item.OnOpenFlyoutCommandCanExecuteChanged;
        }
    }

    private static void OpenFlyoutCommandCanExecuteChanged(BindableObject bindable, EventArgs empty)
    {
        if (bindable is PlayableItemBase control)
        {
            control.OpenFlyoutCommand?.CanExecute(control.OpenFlyoutCommandParameter);
        }
    }

    private void OnOpenFlyoutCommandCanExecuteChanged(object sender, EventArgs e)
    {
        PlayableItemBase.OpenFlyoutCommandCanExecuteChanged(this, EventArgs.Empty);
    }
}