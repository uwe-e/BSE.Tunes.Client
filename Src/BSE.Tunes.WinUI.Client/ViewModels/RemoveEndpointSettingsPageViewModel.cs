using BSE.Tunes.Shared.Services.Abstractions;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public partial class RemoveEndpointSettingsPageViewModel : BaseSettingsViewModel
{
    private readonly ISettingsServiceExtended _settingsService;

    [ObservableProperty]
    private string _serviceEndPoint = string.Empty;

    public RemoveEndpointSettingsPageViewModel(
        INavigationService navigationService,
        IResourceService resourceService,
        ISettingsServiceExtended settingsService,
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
            "Removing the endpoint will also clear your account data. Are you sure?",
            ResourceService.GetString("RemoveEndpointSettingsPage_Dialog_Delete"),
            ResourceService.GetString("RemoveEndpointSettingsPage_Dialog_Cancel"));

        if (result)
        {
            System.Diagnostics.Debug.WriteLine("RemoveEndpointSettingsPageViewModel: Removing endpoint and cascading account data");

            // Clear the endpoint (this will CASCADE and also clear user account)
            // Navigation will happen automatically via SettingsMonitorService
            await _settingsService.ClearServiceEndpointAsync();
        }
    }
}