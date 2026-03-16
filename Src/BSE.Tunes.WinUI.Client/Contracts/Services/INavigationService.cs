using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BSE.Tunes.WinUI.Client.Contracts.Services;

public interface INavigationService
{
    event NavigatedEventHandler Navigated;

    bool CanGoBack { get; }

    Frame? Frame { get; set; }

    bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);

    bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false, bool navigateFullscreen = false);

    bool GoBack();

    void RegisterFrame(string frameKey, Frame frame);

    void UnregisterFrame(string frameKey);

    Frame? GetFrame(string frameKey);
}
