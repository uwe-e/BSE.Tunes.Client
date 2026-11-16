using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Views;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    private const string IsPaneOpenSettingsKey = "ShellNavigationPaneIsOpen";

    private readonly IDataService _dataService;
    private readonly ILocalSettingsService _localSettingsService;
    private bool _isPaneStateInitialized;

    [ObservableProperty]
    public partial bool IsBackEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsPaneOpen { get; set; }

    [ObservableProperty]
    public partial object? Selected { get; set; }

    [ObservableProperty]
    public partial string? QueryText { get; set; }

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
        IDataService dataService,
        ILocalSettingsService localSettingsService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
        _dataService = dataService;
        _localSettingsService = localSettingsService;

        PropertyChanged += OnPropertyChanged;
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

    public async Task InitializeAsync()
    {
        if (_isPaneStateInitialized)
        {
            return;
        }

        var isPaneOpen = await _localSettingsService.ReadSettingAsync<bool?>(IsPaneOpenSettingsKey);
        if (isPaneOpen.HasValue)
        {
            IsPaneOpen = isPaneOpen.Value;
        }

        _isPaneStateInitialized = true;
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_isPaneStateInitialized || e.PropertyName != nameof(IsPaneOpen))
        {
            return;
        }

        await _localSettingsService.SaveSettingAsync(IsPaneOpenSettingsKey, IsPaneOpen);
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
                SearchSuggestions = suggestions;
            }
        }
        else if (reason == AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
        {
            SearchSuggestions = null;
        }
    }

    [RelayCommand]
    private async Task QuerySubmitted(AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var queryText = args?.QueryText;
        if (!string.IsNullOrEmpty(queryText))
        {
            await NavigationService.NavigateToAsync(nameof(SearchPage), queryText, false);

            QueryText = string.Empty;
        }
    }
}
