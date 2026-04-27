using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Contracts.Services;

public interface IDialogService
{
    /// <summary>
    /// Displays a confirmation dialog with primary and close buttons
    /// </summary>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Dialog message</param>
    /// <param name="primaryButtonText">Text for primary button (e.g., "Delete")</param>
    /// <param name="closeButtonText">Text for close/cancel button</param>
    /// <returns>True if primary button was clicked, false otherwise</returns>
    Task<bool> ShowConfirmationDialogAsync(
        string title,
        string message,
        string primaryButtonText,
        string closeButtonText);

    /// <summary>
    /// Displays an alert dialog with a message and OK button
    /// </summary>
    Task ShowAlertAsync(string title, string message);

    Task<(ContentDialogResult Result, TDialog Dialog)> ShowDialogAsync<TDialog>() where TDialog : ContentDialog, new();
}