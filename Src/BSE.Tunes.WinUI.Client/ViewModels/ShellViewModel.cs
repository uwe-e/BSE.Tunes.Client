using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Views;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private bool isPaneOpen;

    [ObservableProperty]
    private object? selected;

    [ObservableProperty]
    private string queryText;

    //[ObservableProperty]
    private IList<string> searchSuggestions;
    public IList<string> SearchSuggestions
    {
        get => searchSuggestions;
        set => SetProperty(ref searchSuggestions, value);
    }
    private readonly IDataService _dataService;

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
        
        //

        if (reason == AutoSuggestionBoxTextChangeReason.UserInput
            && (!string.IsNullOrEmpty(QueryText) && QueryText.Length >= 3))
        {
            ////SearchSuggestions.Clear();
            var suggestions = await _dataService.GetSearchSuggestions(QueryText, 10);
            //foreach (var suggestion in suggestions)
            //{
            //    SearchSuggestions.Add(suggestion);
            //}
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

    [RelayCommand]
    private async Task SuggestionChosen(AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args?.SelectedItem is string selectedText)
        {
            //QueryText = selectedText;
        }
    }
}
