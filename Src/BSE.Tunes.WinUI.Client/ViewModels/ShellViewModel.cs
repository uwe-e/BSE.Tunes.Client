using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Views;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private bool isPaneOpen;

    [ObservableProperty]
    private object? selected;

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }

    [RelayCommand]
    private void TogglePane(bool isPaneOpen)
    {
        IsPaneOpen = !isPaneOpen;
    }

    [RelayCommand]
    private void GoBack()
    {
        if (NavigationService.CanGoBack)
        {
            NavigationService.GoBack();
        }
        
    }
}
