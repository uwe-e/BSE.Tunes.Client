using BSE.Tunes.WinUI.Client.Activation;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.ViewModels;
using BSE.Tunes.WinUI.Client.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Services;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly IThemeSelectorService _themeSelectorService;
    private UIElement? _page = null;

    public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, IEnumerable<IActivationHandler> activationHandlers, IThemeSelectorService themeSelectorService)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _themeSelectorService = themeSelectorService;
    }

    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();

        // Set the MainWindow Content to SplashPage initially
        if (App.MainWindow?.Content == null)
        {
            _page = App.GetService<SplashPage>();
            if (App.MainWindow != null)
            {
                App.MainWindow.Content = _page ?? new Frame();
            }
        }

        // If you have a reference to the page
        if (_page is Page page)
        {
            // read the ViewModel property using reflection
            // and get the view model instance
            var viewModelProperty = page.GetType().GetProperty("ViewModel");
            var viewModel = viewModelProperty?.GetValue(page);

            if (viewModel is IActivationAware activationAware)
            {
                // Call the OnActivatedAsync method on the ViewModel
                await activationAware.OnActivatedAsync(activationArgs);
            }
        }

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        // Activate the MainWindow (with null check for .NET 8/9 compatibility)
        App.MainWindow?.Activate();

        // Execute tasks after activation.
        await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(activationArgs);
        }

        if (_defaultHandler.CanHandle(activationArgs))
        {
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }

    private async Task InitializeAsync()
    {
        await _themeSelectorService.InitializeAsync().ConfigureAwait(false);
        await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
        await _themeSelectorService.SetRequestedThemeAsync();
        await Task.CompletedTask;
    }
}
