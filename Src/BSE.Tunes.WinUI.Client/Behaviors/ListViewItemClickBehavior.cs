using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System.Windows.Input;

namespace BSE.Tunes.WinUI.Client.Behaviors;

public class ListViewItemClickBehavior : Behavior<ListView>
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(ListViewItemClickBehavior),
            new PropertyMetadata(null));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            AssociatedObject.ItemClick += OnItemClick;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject != null)
        {
            AssociatedObject.ItemClick -= OnItemClick;
        }
    }

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (Command?.CanExecute(e.ClickedItem) == true)
        {
            Command.Execute(e.ClickedItem);
        }
    }
}