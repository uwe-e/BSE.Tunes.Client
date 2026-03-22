using BSE.Tunes.WinUI.Client.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public partial class RemoveLoginSettingsPageViewModel : BaseSettingsViewModel
{
    private readonly ISettingsServiceExtended _settingsService;

    [ObservableProperty]
    private string _userName = string.Empty;

    public RemoveLoginSettingsPageViewModel(
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
        UserName = _settingsService.User?.UserName ?? string.Empty;
    }

    protected override async void DeleteSettings()
    {
        var result = await DialogService.ShowConfirmationDialogAsync(
            ResourceService.GetString("RemoveLoginSettingsPage_Dialog_Title"),
            ResourceService.GetString("RemoveLoginSettingsPage_Dialog_Message"),
            ResourceService.GetString("RemoveLoginSettingsPage_Dialog_Delete"),
            ResourceService.GetString("RemoveLoginSettingsPage_Dialog_Cancel"));

        if (result)
        {
            System.Diagnostics.Debug.WriteLine("RemoveLoginSettingsPageViewModel: Removing user account");

            // Clear only the user account (endpoint remains)
            // Navigation will happen automatically via SettingsMonitorService
            await _settingsService.ClearUserAccountAsync();
        }
    }
}