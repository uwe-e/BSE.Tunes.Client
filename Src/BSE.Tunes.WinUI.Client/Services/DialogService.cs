using BSE.Tunes.WinUI.Client.Contracts.Services;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Services;

public class DialogService : IDialogService
{
    public async Task<bool> ShowConfirmationDialogAsync(
        string title,
        string message,
        string primaryButtonText,
        string closeButtonText)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = closeButtonText,
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = App.MainWindow?.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    public async Task ShowAlertAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = App.MainWindow?.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }
}