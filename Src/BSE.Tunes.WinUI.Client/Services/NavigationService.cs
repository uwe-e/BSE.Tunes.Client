using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Contracts.ViewModels;
using BSE.Tunes.WinUI.Client.Helpers;
using BSE.Tunes.WinUI.Client.Views;

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
    private readonly Dictionary<string, Frame> _frames = new();
    private readonly Dictionary<string, object?> _lastParametersUsed = new();
    private Frame? _frame;
    private Page? _shell;
    private bool _navigateFullscreen;

    public event NavigatedEventHandler? Navigated;

    public Frame? Frame
    {
        get
        {
            if (_frame == null)
            {
                // Try to get the shell frame first
                if (_frames.TryGetValue(FrameKeyShell, out var shellFrame))
                {
                    _frame = shellFrame;
                }
                else if (_frames.TryGetValue(FrameKeyMain, out var mainFrame))
                {
                    _frame = mainFrame;
                }
            }

            return _frame;
        }

        set
        {
            UnregisterFrameEvents(_frame);
            _frame = value;
            RegisterFrameEvents(_frame);
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
        if (_frames.ContainsKey(frameKey))
        {
            UnregisterFrameEvents(_frames[frameKey]);
            _frames[frameKey] = frame;
        }
        else
        {
            _frames.Add(frameKey, frame);
        }
        
        RegisterFrameEvents(frame);
    }

    public void UnregisterFrame(string frameKey)
    {
        if (_frames.TryGetValue(frameKey, out var frame))
        {
            UnregisterFrameEvents(frame);
            _frames.Remove(frameKey);
            _lastParametersUsed.Remove(frameKey);
        }
    }

    public Frame? GetFrame(string frameKey)
    {
        return _frames.TryGetValue(frameKey, out var frame) ? frame : null;
    }

    private void RegisterFrameEvents(Frame? frame)
    {
        if (frame != null)
        {
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

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        // Default to shell frame navigation (non-fullscreen)
        return NavigateTo(pageKey, parameter, clearNavigation, navigateFullscreen: false);
    }

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false, bool navigateFullscreen = false)
    {
        try
        {
            var pageType = _pageService.GetPageType(pageKey);

            // Handle fullscreen navigation (login, splash, configuration pages)
            if (navigateFullscreen && _navigateFullscreen != navigateFullscreen)
            {
                ResetNavigation();
                
                // Store the shell for later restoration
                _shell = _shell ?? App.MainWindow.Content as Page;

                // Create or get the main frame
                if (!_frames.ContainsKey(FrameKeyMain))
                {
                    var mainFrame = new Frame();
                    _frames.Add(FrameKeyMain, mainFrame);
                    RegisterFrameEvents(mainFrame);
                }

                Frame = _frames[FrameKeyMain];
                App.MainWindow.Content = Frame;
            }
            // Handle shell navigation (normal app pages)
            else if (!navigateFullscreen)
            {
                // Coming back from fullscreen to shell OR first navigation to shell
                if (_navigateFullscreen != navigateFullscreen || _shell == null)
                {
                    ResetNavigation();
                    
                    // Initialize shell if not already done
                    if (_shell == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Creating ShellPage instance...");
                        _shell = App.GetService<ShellPage>();
                        System.Diagnostics.Debug.WriteLine($"ShellPage created: {_shell != null}");
                    }
                    
                    App.MainWindow.Content = _shell;
                    System.Diagnostics.Debug.WriteLine("ShellPage set as MainWindow.Content");
                }

                // Get the shell frame (should be registered by ShellPage constructor)
                Frame = _frames.GetValueOrDefault(FrameKeyShell);
                System.Diagnostics.Debug.WriteLine($"Shell frame retrieved: {Frame != null}");
                System.Diagnostics.Debug.WriteLine($"Registered frames: {string.Join(", ", _frames.Keys)}");
                
                if (Frame == null)
                {
                    throw new InvalidOperationException(
                        $"Shell frame '{FrameKeyShell}' is not registered. " +
                        $"Available frames: {string.Join(", ", _frames.Keys)}. " +
                        "Ensure the ShellPage constructor calls NavigationService.RegisterFrame().");
                }
            }

            _navigateFullscreen = navigateFullscreen;

            if (Frame == null)
            {
                return false;
            }

            _lastParametersUsed.TryGetValue(_navigateFullscreen ? FrameKeyMain : FrameKeyShell, out var lastParameter);

            if (Frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(lastParameter)))
            {
                Frame.Tag = clearNavigation;
                var vmBeforeNavigation = Frame.GetPageViewModel();
                
                System.Diagnostics.Debug.WriteLine($"Navigating to {pageType.Name}...");
                System.Diagnostics.Debug.WriteLine($"Frame: {Frame.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Parameter: {parameter}");
                
                var navigated = Frame.Navigate(pageType, parameter);
                
                System.Diagnostics.Debug.WriteLine($"Navigation result: {navigated}");
                
                if (navigated)
                {
                    var frameKey = _navigateFullscreen ? FrameKeyMain : FrameKeyShell;
                    _lastParametersUsed[frameKey] = parameter;
                    
                    if (vmBeforeNavigation is INavigationAware navigationAware)
                    {
                        navigationAware.OnNavigatedFrom();
                    }
                }

                return navigated;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Inner message: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    private void ResetNavigation()
    {
        foreach (var pair in _frames)
        {
            pair.Value?.BackStack?.Clear();
        }
    }

    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        var errorDetails = $"Navigation failed to '{e.SourcePageType.Name}'.\n";
        
        if (e.Exception != null)
        {
            errorDetails += $"Exception Type: {e.Exception.GetType().FullName}\n";
            errorDetails += $"Message: {e.Exception.Message}\n";
            errorDetails += $"StackTrace: {e.Exception.StackTrace}\n";
            
            if (e.Exception.InnerException != null)
            {
                errorDetails += $"\nInner Exception: {e.Exception.InnerException.GetType().FullName}\n";
                errorDetails += $"Inner Message: {e.Exception.InnerException.Message}\n";
                errorDetails += $"Inner StackTrace: {e.Exception.InnerException.StackTrace}\n";
            }
        }
        
        System.Diagnostics.Debug.WriteLine(errorDetails);
        
        throw new InvalidOperationException(errorDetails, e.Exception);
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            System.Diagnostics.Debug.WriteLine($"Navigated to: {e.SourcePageType.Name}");
            
            var clearNavigation = frame.Tag is bool clear && clear;
            if (clearNavigation)
            {
                frame.BackStack.Clear();
            }

            if (frame.GetPageViewModel() is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.Parameter);
            }

            Navigated?.Invoke(sender, e);
        }
    }
}
