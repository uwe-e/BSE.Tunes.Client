using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BSE.Tunes.WinUI.Client.Contracts.Services;

public interface INavigationService
{
    event NavigatedEventHandler Navigated;

    bool CanGoBack { get; }

    Frame? Frame { get; set; }

    /// <summary>
    /// Navigates to the specified page within the shell frame (default).
    /// </summary>
    Task<bool> NavigateToAsync(string pageKey, object? parameter = null, bool clearNavigation = false);

    /// <summary>
    /// Navigates to the specified page within the specified frame.
    /// </summary>
    Task<bool> NavigateToAsync(string pageKey, string? frameKey, object? parameter = null, bool clearNavigation = false);

    bool GoBack();

    void RegisterFrame(string frameKey, Frame frame);

    void UnregisterFrame(string frameKey);

    Frame? GetFrame(string frameKey);
}
