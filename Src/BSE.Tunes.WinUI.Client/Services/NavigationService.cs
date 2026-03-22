using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Contracts.ViewModels;
using BSE.Tunes.WinUI.Client.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics.CodeAnalysis;

namespace BSE.Tunes.WinUI.Client.Services;

// For more information on navigation between pages see
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
public class NavigationService : INavigationService
{
    public const string FrameKeyMain = "MainFrame";
    public const string FrameKeyShell = "ShellFrame";

    private readonly IPageService _pageService;
    private readonly Dictionary<string, Frame> _frames = [];
    private readonly Dictionary<string, object?> _lastParametersUsed = [];
    private Frame? _frame;

    public event NavigatedEventHandler? Navigated;

    public Frame? Frame
    {
        get => _frame;
        set
        {
            if (_frame != value)
            {
                UnregisterFrameEvents(_frame);
                _frame = value;
                RegisterFrameEvents(_frame);
            }
        }
    }

    [MemberNotNullWhen(true, nameof(Frame), nameof(_frame))]
    public bool CanGoBack => Frame != null && Frame.CanGoBack;

    public NavigationService(IPageService pageService)
    {
        _pageService = pageService;
    }

    public void RegisterFrame(string frameKey, Frame frame)
    {
        if (_frames.TryGetValue(frameKey, out var existingFrame))
        {
            if (existingFrame == frame)
            {
                return; // Already registered
            }
            
            UnregisterFrameEvents(existingFrame);
        }
        
        _frames[frameKey] = frame;
        RegisterFrameEvents(frame);
        
        System.Diagnostics.Debug.WriteLine($"NavigationService: Registered frame '{frameKey}'");
    }

    public void UnregisterFrame(string frameKey)
    {
        if (_frames.Remove(frameKey, out var frame))
        {
            UnregisterFrameEvents(frame);
            _lastParametersUsed.Remove(frameKey);
            
            System.Diagnostics.Debug.WriteLine($"NavigationService: Unregistered frame '{frameKey}'");
        }
    }

    public Frame? GetFrame(string frameKey)
    {
        return _frames.GetValueOrDefault(frameKey);
    }

    private void RegisterFrameEvents(Frame? frame)
    {
        if (frame != null)
        {
            frame.Navigated -= OnNavigated;
            frame.NavigationFailed -= OnNavigationFailed;
            
            frame.Navigated += OnNavigated;
            frame.NavigationFailed += OnNavigationFailed;
        }
    }

    private void UnregisterFrameEvents(Frame? frame)
    {
        if (frame != null)
        {
            frame.Navigated -= OnNavigated;
            frame.NavigationFailed -= OnNavigationFailed;
        }
    }

    public bool GoBack()
    {
        if (CanGoBack)
        {
            var vmBeforeNavigation = Frame.GetPageViewModel();
            Frame.GoBack();
            
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }

            return true;
        }

        return false;
    }

    public async Task<bool> NavigateToAsync(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        // Default to shell frame for most navigation
        return await NavigateToAsync(pageKey, FrameKeyShell, parameter, clearNavigation);
    }

    public async Task<bool> NavigateToAsync(
        string pageKey, 
        string? frameKey = null, 
        object? parameter = null, 
        bool clearNavigation = false)
    {
        try
        {
            var pageType = _pageService.GetPageType(pageKey);
            var targetFrameKey = frameKey ?? FrameKeyShell;

            // Get the target frame
            if (!_frames.TryGetValue(targetFrameKey, out var targetFrame))
            {
                var registeredFrames = string.Join(", ", _frames.Keys);
                throw new InvalidOperationException(
                    $"Frame '{targetFrameKey}' is not registered. " +
                    $"Available frames: [{registeredFrames}]. " +
                    $"Ensure the frame is registered before navigation.");
            }

            // Set as active frame
            Frame = targetFrame;

            // Check if we need to navigate
            _lastParametersUsed.TryGetValue(targetFrameKey, out var lastParameter);
            if (Frame.Content?.GetType() == pageType && 
                (parameter == null || parameter.Equals(lastParameter)))
            {
                return false; // Already on this page
            }

            // Store navigation state
            Frame.Tag = clearNavigation;
            var vmBeforeNavigation = Frame.GetPageViewModel();

            System.Diagnostics.Debug.WriteLine($"NavigationService: Navigating to {pageType.Name} in frame '{targetFrameKey}'");

            // Navigate
            var navigated = Frame.Navigate(pageType, parameter);

            if (navigated)
            {
                _lastParametersUsed[targetFrameKey] = parameter;

                if (vmBeforeNavigation is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }
            }

            return navigated;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"NavigationService: Navigation error - {ex.Message}");
            throw;
        }
    }

    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        var errorDetails = $"Navigation failed to '{e.SourcePageType.Name}'.";
        
        if (e.Exception != null)
        {
            errorDetails += $"\nException: {e.Exception.GetType().Name}";
            errorDetails += $"\nMessage: {e.Exception.Message}";
        }
        
        System.Diagnostics.Debug.WriteLine($"NavigationService: {errorDetails}");
        throw new InvalidOperationException(errorDetails, e.Exception);
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            System.Diagnostics.Debug.WriteLine($"NavigationService: Navigated to {e.SourcePageType.Name}");
            
            // Clear back stack if requested
            var clearNavigation = frame.Tag is bool clear && clear;
            if (clearNavigation)
            {
                frame.BackStack.Clear();
                frame.Tag = null; // Reset flag
            }

            // Notify ViewModel
            if (frame.GetPageViewModel() is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.Parameter);
            }

            Navigated?.Invoke(sender, e);
        }
    }
}
