using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    internal class LoginWizzardPageViewModel(
        INavigationService navigationService,
        IResourceService resourceService,
        IPageDialogService pageDialogService,
        IAuthenticationService authenticationService) : ViewModelBase(navigationService)
    {
        private readonly IResourceService _resourceService = resourceService;
        private readonly IPageDialogService _pageDialogService = pageDialogService;
        private readonly IAuthenticationService _authenticationService = authenticationService;
        private DelegateCommand _saveCommand;
        private string _userName;
        private string _password;

        public string UserName
        {
            get => _userName;
            set
            {
                SetProperty(ref _userName, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(Save, CanSave);

        private bool CanSave()
        {
            return !String.IsNullOrEmpty(UserName) && !(string.IsNullOrEmpty(Password));
        }

        private void Save()
        {
            _ = SaveAsync();
        }

        private async Task SaveAsync()
        {
            try
            {
                await _authenticationService.SignInAsync(UserName, Password);
                var result = await NavigationService.RestartAndNavigateAsync("/" + nameof(SplashPage));
                if (!result.Success)
                {
                    // log result.Exception or message to debug output
                    System.Diagnostics.Debug.WriteLine(result.Exception);
                }
            }
            catch (Exception)
            {
                string title = _resourceService.GetString("AlertDialog_Error_Title_Text");
                string message = _resourceService.GetString("LoginPageViewModel_LoginException");
                string dialogResult = _resourceService.GetString("Dialog_Result_Cancel");

                await _pageDialogService.DisplayAlertAsync(
                    title,
                    message,
                    dialogResult);
            }
        }
    }
}
