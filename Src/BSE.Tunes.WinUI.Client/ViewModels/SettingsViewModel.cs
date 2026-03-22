using System.Collections.ObjectModel;
using System.Reflection;
using BSE.Tunes.Shared.Services.Abstractions;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Helpers;
using BSE.Tunes.WinUI.Client.Models;
using BSE.Tunes.WinUI.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Windows.ApplicationModel;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly INavigationService _navigationService;
    private readonly IResourceService _resourceService;
    private readonly ISettingsService _settingsService;
    [ObservableProperty]
    private string _versionDescription;

    public ObservableCollection<SettingsItem> SettingsItems { get; } = [];

    public SettingsViewModel(
        INavigationService navigationService,
        IResourceService resourceService,
        ISettingsService settingsService)
    {
        _navigationService = navigationService;
        _resourceService = resourceService;
        _settingsService = settingsService;
        _versionDescription = GetVersionDescription();

        InitializeSettingsItems();
    }

    private void InitializeSettingsItems()
    {
        SettingsItems.Add(new SettingsItem
        {
            Title = _resourceService.GetString("SettingsPage_SectionWebserver_Title"),
            Description = _settingsService.ServiceEndPoint ?? string.Empty,
            Glyph = "\uE774", // Globe
            PageKey = nameof(RemoveEndpointSettingsPage)
        });

        SettingsItems.Add(new SettingsItem
        {
            Title = _resourceService.GetString("SettingsPage_SectionAccount_Title"),
            Description = _settingsService.User?.UserName ?? string.Empty,
            Glyph = "\uE77B", // Contact
            PageKey = nameof(RemoveLoginSettingsPage)
        });

        SettingsItems.Add(new SettingsItem
        {
            Title = "Settings_Cache",
            Description = "Storage and offline data",
            Glyph = "\uE895", // HDD
            PageKey = "CacheSettingsPage"
        });

        SettingsItems.Add(new SettingsItem
        {
            Title = "Settings_Personalization",
            Description = "Theme, appearance",
            Glyph = "\uE771", // Personalize
            PageKey = "PersonalizationSettingsPage"
        });

        SettingsItems.Add(new SettingsItem
        {
            Title = "Settings_About",
            Description = "Version, privacy, terms",
            Glyph = "\uE946", // Info
            PageKey = "AboutSettingsPage"
        });
    }

    [RelayCommand]
    private void NavigateToSetting(SettingsItem? item)
    {
        if (item?.PageKey != null)
        {
            _navigationService.NavigateToAsync(item.PageKey, null, false, false);
        }
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
