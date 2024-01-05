﻿using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    internal class LoginWizzardPageViewModel : ViewModelBase
    {
        private readonly IResourceService _resourceService;
        private readonly IPageDialogService _pageDialogService;
        private readonly IAuthenticationService _authenticationService;
        private DelegateCommand _saveCommand;
        private string? _userName;
        private string? _password;

        public string? UserName
        {
            get => _userName;
            set
            {
                SetProperty(ref _userName, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }
        public string? Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(Save, CanSave);

        public LoginWizzardPageViewModel(
            INavigationService navigationService,
            IResourceService resourceService,
            IPageDialogService pageDialogService,
            IAuthenticationService authenticationService) : base(navigationService)
        {
            _resourceService = resourceService;
            _pageDialogService = pageDialogService;
            _authenticationService = authenticationService;
        }

        private bool CanSave()
        {
            return !String.IsNullOrEmpty(UserName) && !(string.IsNullOrEmpty(Password));
        }

        private async void Save()
        {
            try
            {
                await _authenticationService.LoginAsync(UserName, Password);
                NavigationService.CreateBuilder()
                                .UseAbsoluteNavigation()
                                .AddSegment<MainPage>()
                                .Navigate();
            }
            catch (Exception)
            {
                var title = _resourceService.GetString("AlertDialog_Error_Title_Text");
                var message = _resourceService.GetString("LoginPageViewModel_LoginException");
                var dialogResult = _resourceService.GetString("Dialog_Result_Cancel");

                await _pageDialogService.DisplayAlertAsync(
                    title,
                    message,
                    dialogResult);
            }
        }
    }
}
