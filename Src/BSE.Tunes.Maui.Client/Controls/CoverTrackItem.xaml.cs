namespace BSE.Tunes.Maui.Client.Controls;

public partial class CoverTrackItem : PlayableItemBase
{
    public static readonly BindableProperty CoverProperty
        = BindableProperty.Create(nameof(Cover),
            typeof(ImageSource),
            typeof(CoverTrackItem),
            default(ImageSource));

    public ImageSource Cover
    {
        get => (ImageSource)GetValue(CoverProperty);
        set => SetValue(CoverProperty, value);
    }

    public static readonly BindableProperty DurationProperty
        = BindableProperty.Create(nameof(Duration), typeof(string), typeof(CoverTrackItem), string.Empty);

    public string Duration
    {
        get => (string)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public CoverTrackItem()
    {
        InitializeComponent();
    }
    
}