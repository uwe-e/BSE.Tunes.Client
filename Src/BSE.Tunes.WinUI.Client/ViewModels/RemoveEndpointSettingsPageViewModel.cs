using BSE.Tunes.Shared.Services.Abstractions;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public partial class RemoveEndpointSettingsPageViewModel : BaseSettingsViewModel
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private string _serviceEndPoint = string.Empty;

    public RemoveEndpointSettingsPageViewModel(
        INavigationService navigationService,
        IResourceService resourceService,
        ISettingsService settingsService,
        IDialogService dialogService)
        : base(navigationService, resourceService, dialogService)
    {
        _settingsService = settingsService;
    }

    public override void LoadSettings()
    {
        ServiceEndPoint = _settingsService.ServiceEndPoint ?? string.Empty;
    }

    protected override async void DeleteSettings()
    {
            var result = await DialogService.ShowConfirmationDialogAsync(
                ResourceService.GetString("RemoveEndpointSettingsPage_Dialog_Title"),
                ResourceService.GetString("RemoveEndpointSettingsPage_Dialog_Message"),
                ResourceService.GetString("RemoveEndpointSettingsPage_Dialog_Delete"),
                ResourceService.GetString("RemoveEndpointSettingsPage_Dialog_Cancel"));
        //var dialog = new ContentDialog
        //{
        //    Title = ResourceService.GetString("RemoveEndpointSettingsPage_Dialog_Title"),
        //    Content = ResourceService.GetString("RemoveEndpointSettingsPage_Dialog_Message"),
        //    PrimaryButtonText = ResourceService.GetString("Dialog_Delete"),
        //    CloseButtonText = ResourceService.GetString("Dialog_Cancel"),
        //    DefaultButton = ContentDialogButton.Close,
        //    XamlRoot = App.MainWindow?.Content.XamlRoot
        //};

        //var result = await dialog.ShowAsync();

        if (result)
        {
            DeleteAction();
        }
    }

    private void DeleteAction()
    {
        _settingsService.ServiceEndPoint = null;
        NavigationService.NavigateTo(nameof(SplashPage), Services.NavigationService.FrameKeyMain, clearNavigation: true, navigateFullscreen: true);
    }
}