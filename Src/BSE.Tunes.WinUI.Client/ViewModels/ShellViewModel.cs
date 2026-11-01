using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Views;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    private readonly IDataService _dataService;
    
    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private bool isPaneOpen;

    [ObservableProperty]
    private object? selected;

    [ObservableProperty]
    private string? queryText;

    public IList<string>? SearchSuggestions
    {
        get;
        set => SetProperty(ref field, value);
    }

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    public ShellViewModel(
        INavigationService navigationService,
        INavigationViewService navigationViewService,
        IDataService dataService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
        _dataService = dataService;
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

    [RelayCommand]
    private async Task QueryTextChanged(AutoSuggestionBoxTextChangeReason? reason)
    {
        if (reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            IList<string>? suggestions = null;
            if (!string.IsNullOrEmpty(QueryText) && QueryText.Length >= 3)
            {
                suggestions = await _dataService.GetSearchSuggestions(QueryText, 10);
            }
            SearchSuggestions = suggestions;
        }
    }

    [RelayCommand]
    private async Task QuerySubmitted(AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var queryText = args?.QueryText;
        if (!string.IsNullOrEmpty(queryText))
        {
            await NavigationService.NavigateToAsync(nameof(SearchPage), queryText, false);
        }
    }
}
