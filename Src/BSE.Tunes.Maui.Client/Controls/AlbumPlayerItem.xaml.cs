using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.Controls;

public partial class AlbumPlayerItem : PlayableItemBase
{
    public static readonly BindableProperty CoverProperty
        = BindableProperty.Create(nameof(Cover),
            typeof(ImageSource),
            typeof(AlbumPlayerItem),
            default(ImageSource));

    public ImageSource Cover
    {
        get => (ImageSource)GetValue(CoverProperty);
        set => SetValue(CoverProperty, value);
    }

    public static readonly BindableProperty YearProperty
        = BindableProperty.Create(nameof(Year), typeof(string), typeof(AlbumPlayerItem), string.Empty);

    public string Year
    {
        get => (string)GetValue(YearProperty);
        set => SetValue(YearProperty, value);
    }

    public static readonly BindableProperty GenreProperty
        = BindableProperty.Create(nameof(Genre), typeof(string), typeof(AlbumPlayerItem), string.Empty);

    public string Genre
    {
        get => (string)GetValue(GenreProperty);
        set => SetValue(GenreProperty, value);
    }

    public static readonly BindableProperty PlayRandomizedCommandProperty
        = BindableProperty.Create(
            nameof(PlayRandomizedCommand),
            typeof(ICommand),
            typeof(AlbumPlayerItem),
            null,
            propertyChanging: OnPlayRandomizedCommandChanging,
            propertyChanged: OnPlayRandomizedCommandChanged);

    public ICommand PlayRandomizedCommand
    {
        get => (ICommand)GetValue(PlayRandomizedCommandProperty);
        set => SetValue(PlayRandomizedCommandProperty, value);
    }

    public static readonly BindableProperty PlayRandomizedCommandParameterProperty
        = BindableProperty.Create(
            nameof(PlayRandomizedCommandParameter),
            typeof(object),
            typeof(AlbumPlayerItem),
            null,
            propertyChanged: OnPlayRandomizedCommandParameterChanged);

    public object PlayRandomizedCommandParameter
    {
        get => GetValue(PlayRandomizedCommandParameterProperty);
        set => SetValue(PlayRandomizedCommandParameterProperty, value);
    }

    public AlbumPlayerItem()
    {
        InitializeComponent();
    }

    private void OnPlayAllClicked(object sender, EventArgs e)
    {
        PlayCommand?.Execute(PlayCommandParameter);
    }

    private void OnPlayAllRandomizedClicked(object sender, EventArgs e)
    {
        PlayRandomizedCommand?.Execute(PlayRandomizedCommandParameter);
    }

    private static void OnPlayRandomizedCommandChanging(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue is ICommand oldCmd && bindable is AlbumPlayerItem item)
        {
            oldCmd.CanExecuteChanged -= item.OnPlayRandomizedCommandCanExecuteChanged;
        }
    }

    private static void OnPlayRandomizedCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not AlbumPlayerItem item)
            return;

        if (newValue is ICommand newCmd)
        {
            newCmd.CanExecuteChanged += item.OnPlayRandomizedCommandCanExecuteChanged;
            // initial evaluation to keep behavior compatible with previous implementation
            PlayRandomizedCommandCanExecuteChanged(item);
        }
    }

    private void OnPlayRandomizedCommandCanExecuteChanged(object? sender, EventArgs e)
    {
        PlayRandomizedCommandCanExecuteChanged(this);
    }

    private static void PlayRandomizedCommandCanExecuteChanged(BindableObject bindable)
    {
        if (bindable is AlbumPlayerItem control && control.PlayRandomizedCommand is ICommand cmd)
        {
            // call CanExecute for side-effects or to trigger command internals if needed;
            // result is intentionally ignored to preserve previous behavior.
            _ = cmd.CanExecute(control.PlayRandomizedCommandParameter);
        }
    }

    private static void OnPlayRandomizedCommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
        => PlayRandomizedCommandCanExecuteChanged(bindable);
}