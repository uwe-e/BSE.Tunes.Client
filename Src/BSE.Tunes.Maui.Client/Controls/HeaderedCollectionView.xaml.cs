using System.Collections;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.Controls;

public partial class HeaderedCollectionView : ContentView
{
    private SectionHeaderButton _sectionHeaderButton;
    private ActivityIndicator _activityIndicator;


    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(HeaderedCollectionView), null);

    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public static readonly BindableProperty HeaderButtonStyleProperty =
       BindableProperty.Create("Style", typeof(Style), typeof(HeaderedCollectionView), default(Style),
           propertyChanged: OnHeaderButtonStyleChanged);

    public Style HeaderButtonStyle
    {
        get { return (Style)GetValue(StyleProperty); }
        set { SetValue(StyleProperty, value); }
    }

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource),
            typeof(IEnumerable), typeof(HeaderedCollectionView), null);

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(HeaderedCollectionView));

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(HeaderedCollectionView), default,
            defaultBindingMode: BindingMode.TwoWay);

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly BindableProperty SelectionChangedCommandProperty =
        BindableProperty.Create(nameof(SelectionChangedCommand), typeof(object),
            typeof(HeaderedCollectionView));

    public ICommand SelectionChangedCommand
    {
        get => (ICommand)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    public static readonly BindableProperty SelectionChangedCommandParameterProperty =
        BindableProperty.Create(nameof(SelectionChangedCommandParameter), typeof(object),
            typeof(HeaderedCollectionView));

    public object SelectionChangedCommandParameter
    {
        get => GetValue(SelectionChangedCommandParameterProperty);
        set => SetValue(SelectionChangedCommandParameterProperty, value);
    }

    public static readonly BindableProperty RemainingItemsThresholdReachedCommandProperty =
        BindableProperty.Create(nameof(RemainingItemsThresholdReachedCommand),
            typeof(ICommand), typeof(HeaderedCollectionView), null);

    public ICommand RemainingItemsThresholdReachedCommand
    {
        get => (ICommand)GetValue(RemainingItemsThresholdReachedCommandProperty);
        set => SetValue(RemainingItemsThresholdReachedCommandProperty, value);
    }

    public static readonly BindableProperty RemainingItemsThresholdReachedCommandParameterProperty =
        BindableProperty.Create(nameof(RemainingItemsThresholdReachedCommandParameter),
            typeof(object), typeof(HeaderedCollectionView), default);

    public object RemainingItemsThresholdReachedCommandParameter
    {
        get => GetValue(RemainingItemsThresholdReachedCommandParameterProperty);
        set => SetValue(RemainingItemsThresholdReachedCommandParameterProperty, value);
    }

    public static readonly BindableProperty RemainingItemsThresholdProperty =
        BindableProperty.Create(nameof(RemainingItemsThreshold),
            typeof(int), typeof(HeaderedCollectionView), -1, validateValue: (bindable, value) => (int)value >= -1);

    public int RemainingItemsThreshold
    {
        get => (int)GetValue(RemainingItemsThresholdProperty);
        set => SetValue(RemainingItemsThresholdProperty, value);
    }

    public static readonly BindableProperty IsBusyProperty =
       BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(HeaderedCollectionView), false);

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public static readonly BindableProperty ActivityIndicatorColorProperty
        = BindableProperty.Create(nameof(ActivityIndicatorColor), typeof(Color), typeof(HeaderedCollectionView), null);

    public Color ActivityIndicatorColor
    {
        get { return (Color)GetValue(BackgroundColorProperty); }
        set { SetValue(BackgroundColorProperty, value); }
    }

    public HeaderedCollectionView()
    {
        InitializeComponent();
    }

    public void SetHeaderStyle(Style style)
    {
        if (style != null)
        {
            _sectionHeaderButton.Style = style;
        }
    }

    protected override void OnApplyTemplate()
    {
        _sectionHeaderButton = base.GetTemplateChild("PART_SectionHeaderButton") as SectionHeaderButton;
        _activityIndicator = base.GetTemplateChild("PART_ActivityIndicator") as ActivityIndicator;

        base.OnApplyTemplate();
    }

    private static void OnHeaderButtonStyleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeaderedCollectionView element)
        {
            element.SetHeaderStyle(newValue as Style);
        }
    }
}