using BSE.Tunes.WinUI.Client.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public abstract partial class BaseSettingsViewModel : ViewModelBase
{
    protected readonly INavigationService NavigationService;
    protected readonly IResourceService ResourceService;
    protected readonly IDialogService DialogService;
    [ObservableProperty]
    private bool _isLoading;

    protected BaseSettingsViewModel(
        INavigationService navigationService,
        IResourceService resourceService,
        IDialogService dialogService)
    {
        NavigationService = navigationService;
        ResourceService = resourceService;
        DialogService = dialogService;
    }

    /// <summary>
    /// Override this method to load specific settings
    /// </summary>
    public virtual void LoadSettings()
    {
    }

    /// <summary>
    /// Override this method to implement the delete logic with confirmation
    /// </summary>
    [RelayCommand]
    protected virtual void DeleteSettings()
    {
    }

    /// <summary>
    /// Called when the view is navigated to
    /// </summary>
    public override void OnNavigatedTo(object parameter)
    {
        LoadSettings();
    }

    /// <summary>
    /// Called when the view is navigated from
    /// </summary>
    public override void OnNavigatedFrom()
    {
    }
}