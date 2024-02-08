namespace BSE.Tunes.Maui.Client.Controls;

public partial class ListViewItem : ContentView
{
    public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(ListViewItem), null);

    public static readonly BindableProperty TitleBackgroundColorProperty =
        BindableProperty.Create(nameof(TitleBackgroundColor), typeof(Color), typeof(ListViewItem), default(Color));

    public static readonly BindableProperty SubTitleProperty =
        BindableProperty.Create(nameof(SubTitle), typeof(string), typeof(ListViewItem), null);

    public static readonly BindableProperty SubTitleBackgroundColorProperty =
        BindableProperty.Create(nameof(SubTitleBackgroundColor), typeof(Color), typeof(ListViewItem), default(Color));

    public static readonly BindableProperty ImageSourceProperty =
        BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(IImageElement), default(ImageSource));

    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public Color TitleBackgroundColor
    {
        get => (Color)GetValue(TitleBackgroundColorProperty);
        set => SetValue(TitleBackgroundColorProperty, value);
    }

    public string SubTitle
    {
        get { return (string)GetValue(SubTitleProperty); }
        set { SetValue(SubTitleProperty, value); }
    }

    public Color SubTitleBackgroundColor
    {
        get => (Color)GetValue(SubTitleBackgroundColorProperty);
        set => SetValue(SubTitleBackgroundColorProperty, value);
    }

    public ImageSource ImageSource
    {
        get { return (ImageSource)GetValue(ImageSourceProperty); }
        set { SetValue(ImageSourceProperty, value); }
    }

    public ListViewItem()
	{
		InitializeComponent();
	}
}