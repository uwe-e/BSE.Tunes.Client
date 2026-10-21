using BSE.Tunes.WinUI.Client.Contracts.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Services;

public class DialogService : IDialogService
{
    private readonly IThemeSelectorService _themeSelectorService;

    public DialogService(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
    }

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
            XamlRoot = App.MainWindow?.Content.XamlRoot,
            RequestedTheme = _themeSelectorService.Theme
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
            XamlRoot = App.MainWindow?.Content.XamlRoot,
            RequestedTheme = _themeSelectorService.Theme
        };

        await dialog.ShowAsync();
    }

    public async Task<(ContentDialogResult Result, TDialog Dialog)> ShowDialogAsync<TDialog>() where TDialog : ContentDialog, new()
    {
        var dialog = new TDialog
        {
            XamlRoot = App.MainWindow?.Content.XamlRoot,
            RequestedTheme = _themeSelectorService.Theme,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
        };

        var result = await dialog.ShowAsync();
        return (result, dialog);
    }
}