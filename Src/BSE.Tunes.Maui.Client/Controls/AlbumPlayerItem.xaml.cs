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
                nameof(AlbumPlayerItem),
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
            nameof(AlbumPlayerItem),
            typeof(object),
            typeof(AlbumPlayerItem),
            null,
            propertyChanged: (bindable, oldvalue, newvalue) => PlayRandomizedCommandCanExecuteChanged(bindable, EventArgs.Empty));



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
        AlbumPlayerItem item = (AlbumPlayerItem)bindable;
        if (oldValue != null)
        {
            (oldValue as ICommand).CanExecuteChanged -= item.OnPlayRandomizedCommandCanExecuteChanged;
        }
    }
    private static void OnPlayRandomizedCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        AlbumPlayerItem item = (AlbumPlayerItem)bindable;
        if (newValue is ICommand newCommand)
        {
            newCommand.CanExecuteChanged += item.OnPlayRandomizedCommandCanExecuteChanged;
        }
        if (item.PlayCommand != null)
        {
            PlayRandomizedCommandCanExecuteChanged(item, EventArgs.Empty);
        }
    }

    private void OnPlayRandomizedCommandCanExecuteChanged(object sender, EventArgs e)
    {
        AlbumPlayerItem.PlayRandomizedCommandCanExecuteChanged(this, EventArgs.Empty);
    }
    private static void PlayRandomizedCommandCanExecuteChanged(BindableObject bindable, EventArgs empty)
    {
        if (bindable is AlbumPlayerItem control)
        {
            control.PlayRandomizedCommand?.CanExecute(control.PlayRandomizedCommandParameter);
        }
    }
}