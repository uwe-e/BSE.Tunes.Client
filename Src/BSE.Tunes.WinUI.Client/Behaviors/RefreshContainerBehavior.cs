using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System.Windows.Input;

namespace BSE.Tunes.WinUI.Client.Behaviors;

public class RefreshContainerBehavior : Behavior<RefreshContainer>
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(RefreshContainerBehavior),
            new PropertyMetadata(null));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.RefreshRequested += OnRefreshRequested;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.RefreshRequested -= OnRefreshRequested;
    }

    private async void OnRefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
    {
        using var deferral = args.GetDeferral();

        if (Command?.CanExecute(null) == true)
        {
            Command.Execute(null);

            // Allow async commands to complete
            await Task.Delay(100);
        }
    }
}