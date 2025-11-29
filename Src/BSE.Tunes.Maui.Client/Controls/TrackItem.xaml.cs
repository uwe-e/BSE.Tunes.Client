namespace BSE.Tunes.Maui.Client.Controls;

public partial class TrackItem : PlayableItemBase
{
    public static readonly BindableProperty NumberProperty
        = BindableProperty.Create(nameof(Number), typeof(string), typeof(TrackItem), string.Empty);

    public string Number
    {
        get => (string)GetValue(NumberProperty);
        set => SetValue(NumberProperty, value);
    }

    public static readonly BindableProperty DurationProperty
        = BindableProperty.Create(nameof(Duration), typeof(string), typeof(TrackItem), string.Empty);

    public string Duration
    {
        get => (string)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public TrackItem()
    {
        InitializeComponent();
    }
    
}