using BSE.Tunes.Shared.Services;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Services;
using BSE.Tunes.WinUI.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public partial class LoginPageViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoggingIn;

    public LoginPageViewModel(
        IAuthenticationService authenticationService,
        INavigationService navigationService)
    {
        _authenticationService = authenticationService;
        _navigationService = navigationService;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsLoggingIn = true;
        ErrorMessage = string.Empty;

        try
        {
            var success = await _authenticationService.SignInAsync(UserName, Password);
            
            if (success)
            {
                _navigationService.NavigateTo(nameof(SplashPage), NavigationService.FrameKeyMain, clearNavigation: true);
            }
            else
            {
                ErrorMessage = "Login failed. Please check your credentials.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    private bool CanLogin() => 
        !string.IsNullOrWhiteSpace(UserName) && 
        !string.IsNullOrWhiteSpace(Password) && 
        !IsLoggingIn;

    partial void OnUserNameChanged(string value)
    {
        LoginCommand.NotifyCanExecuteChanged();
        ErrorMessage = string.Empty;
    }

    partial void OnPasswordChanged(string value)
    {
        LoginCommand.NotifyCanExecuteChanged();
        ErrorMessage = string.Empty;
    }
}