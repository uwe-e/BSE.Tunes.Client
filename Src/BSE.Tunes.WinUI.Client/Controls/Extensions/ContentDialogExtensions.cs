using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

namespace BSE.Tunes.WinUI.Client.Controls.Extensions;

/// <summary>
/// Provides attached properties for ContentDialog to support MVVM command binding
/// </summary>
public static class ContentDialogExtensions
{
    #region CancelableCommand

    public static readonly DependencyProperty CancelableCommandProperty =
        DependencyProperty.RegisterAttached(
            "CancelableCommand",
            typeof(ICommand),
            typeof(ContentDialogExtensions),
            new PropertyMetadata(null, OnCancelableCommandChanged));

    public static ICommand GetCancelableCommand(DependencyObject obj)
        => (ICommand)obj.GetValue(CancelableCommandProperty);

    public static void SetCancelableCommand(DependencyObject obj, ICommand value)
        => obj.SetValue(CancelableCommandProperty, value);

    #endregion

    #region CancelableCommandParameter

    public static readonly DependencyProperty CancelableCommandParameterProperty =
        DependencyProperty.RegisterAttached(
            "CancelableCommandParameter",
            typeof(object),
            typeof(ContentDialogExtensions),
            new PropertyMetadata(null));

    public static object GetCancelableCommandParameter(DependencyObject obj)
        => obj.GetValue(CancelableCommandParameterProperty);

    public static void SetCancelableCommandParameter(DependencyObject obj, object value)
        => obj.SetValue(CancelableCommandParameterProperty, value);

    #endregion

    #region DialogCancel

    public static readonly DependencyProperty DialogCancelProperty =
        DependencyProperty.RegisterAttached(
            "DialogCancel",
            typeof(bool),
            typeof(ContentDialogExtensions),
            new PropertyMetadata(false));

    public static bool GetDialogCancel(DependencyObject obj)
        => (bool)obj.GetValue(DialogCancelProperty);

    public static void SetDialogCancel(DependencyObject obj, bool value)
        => obj.SetValue(DialogCancelProperty, value);

    #endregion

    private static void OnCancelableCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ContentDialog dialog)
        {
            dialog.PrimaryButtonClick -= OnPrimaryButtonClick;

            if (e.NewValue is ICommand)
            {
                dialog.PrimaryButtonClick += OnPrimaryButtonClick;
            }
        }
    }

    private static async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var command = GetCancelableCommand(sender);
        if (command == null)
            return; 

        var parameter = GetCancelableCommandParameter(sender);

        // Get deferral to allow async operations
        var deferral = args.GetDeferral();

        try
        {
            /*
             * Check if it's an async command and await it
             * That's the trick. 
             * Without those checks,
             * the dialog would close immediately after executing the command, even if it's async.
             * By awaiting the async command, we ensure that the dialog stays open until the command completes.
             */
            if (command is IAsyncRelayCommand asyncCommand)
            {
                if (asyncCommand.CanExecute(parameter))
                {
                    await asyncCommand.ExecuteAsync(parameter);
                }
            }
            else if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }

            // Check if ViewModel requested cancellation
            var shouldCancel = GetDialogCancel(sender);
            args.Cancel = shouldCancel;
        }
        finally
        {
            deferral.Complete();
        }
    }

}