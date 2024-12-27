using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.Controls;

public partial class HeaderedTextCell : ViewCell
{

    public static readonly BindableProperty CommandProperty
        = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(HeaderedTextCell), default(ICommand),
            propertyChanging: (bindable, oldvalue, newvalue) =>
            {
                var textCell = (HeaderedTextCell)bindable;
                var oldcommand = (ICommand)oldvalue;
                if (oldcommand != null)
                    oldcommand.CanExecuteChanged -= textCell.OnCommandCanExecuteChanged;
            }, propertyChanged: (bindable, oldvalue, newvalue) =>
            {
                var textCell = (HeaderedTextCell)bindable;
                var newcommand = (ICommand)newvalue;
                if (newcommand != null)
                {
                    textCell.IsEnabled = newcommand.CanExecute(textCell.CommandParameter);
                    newcommand.CanExecuteChanged += textCell.OnCommandCanExecuteChanged;
                }
            });

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(TextCell), default(object),
            propertyChanged: (bindable, oldvalue, newvalue) =>
            {
                var textCell = (HeaderedTextCell)bindable;
                if (textCell.Command != null)
                {
                    textCell.IsEnabled = textCell.Command.CanExecute(newvalue);
                }
            });

    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    public object CommandParameter
    {
        get { return GetValue(CommandParameterProperty); }
        set { SetValue(CommandParameterProperty, value); }
    }

    public static readonly BindableProperty TextProperty
        = BindableProperty.Create(nameof(Text), typeof(string), typeof(HeaderedTextCell), default(string));

    public static readonly BindableProperty TextColorProperty
            = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(HeaderedTextCell), null);

    public static readonly BindableProperty TitleProperty
        = BindableProperty.Create(nameof(Title), typeof(string), typeof(HeaderedTextCell), default(string));

    public static readonly BindableProperty TitleColorProperty
            = BindableProperty.Create(nameof(TitleColor), typeof(Color), typeof(HeaderedTextCell), null);

    public static readonly BindableProperty DetailProperty
        = BindableProperty.Create(nameof(Detail), typeof(string), typeof(HeaderedTextCell), default(string));

    public static readonly BindableProperty DetailColorProperty
        = BindableProperty.Create(nameof(DetailColor), typeof(Color), typeof(HeaderedTextCell), null);

    public string Detail
    {
        get { return (string)GetValue(DetailProperty); }
        set { SetValue(DetailProperty, value); }
    }

    public Color DetailColor
    {
        get { return (Color)GetValue(DetailColorProperty); }
        set { SetValue(DetailColorProperty, value); }
    }

    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public Color TitleColor
    {
        get { return (Color)GetValue(TitleColorProperty); }
        set { SetValue(TitleColorProperty, value); }
    }

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public Color TextColor
    {
        get { return (Color)GetValue(TextColorProperty); }
        set { SetValue(TextColorProperty, value); }
    }

    public HeaderedTextCell()
	{
		InitializeComponent();
        BindingContext = this;

        //var label = new Label();
        //label.SetBinding(Label.TextProperty, nameof(Detail));
        //View = new StackLayout { Children = { label } };
    }

    protected override void OnTapped()
    {
        base.OnTapped();
        if (!IsEnabled) {
            return;
        }

    }

    void OnCommandCanExecuteChanged(object sender, EventArgs eventArgs)
    {
        IsEnabled = Command.CanExecute(CommandParameter);
    }

    //protected override void OnBindingContextChanged()
    //{
    //    base.OnBindingContextChanged();
    //    View.BindingContext = BindingContext;
    //}


}