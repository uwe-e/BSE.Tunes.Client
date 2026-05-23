using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public partial class CacheSettingsPageViewModel : BaseSettingsViewModel, IRecipient<CacheChangedMessage>
{
    private readonly IStorageService _storageService;
    private bool _isCacheChanged;

    [ObservableProperty]
    private string _imageCacheSize = "0 MB";

    [ObservableProperty]
    private string _audioCacheSize = "0 MB";

    public CacheSettingsPageViewModel(
        INavigationService navigationService,
        IResourceService resourceService,
        IDialogService dialogService,
        IStorageService storageService)
        : base(navigationService, resourceService, dialogService)
    {
        _storageService = storageService;
    }

    public override void LoadSettings()
    {
        _ = LoadCacheSizesAsync();
    }

    public override void OnNavigatedTo(object parameter)
    {
        // Register to receive cache changed messages
        Messenger.Register(this);

        base.OnNavigatedTo(parameter);
    }

    public override void OnNavigatedFrom()
    {
        // Unregister from messages
        Messenger.Unregister<CacheChangedMessage>(this);

        base.OnNavigatedFrom();
    }

    /// <summary>
    /// Receives cache changed messages and reloads settings if cache was modified
    /// </summary>
    public void Receive(CacheChangedMessage message)
    {
        if (message.Mode != CacheChangeMode.None)
        {
            LoadSettings();
        }
    }

    protected override async void DeleteSettings()
    {
        var result = await DialogService.ShowConfirmationDialogAsync(
            ResourceService.GetString("CacheSettingsPage_Dialog_Title"),
            ResourceService.GetString("CacheSettingsPage_Dialog_Message"),
            ResourceService.GetString("CacheSettingsPage_Dialog_Delete"),
            ResourceService.GetString("CacheSettingsPage_Dialog_Cancel"));

        if (result)
        {
            await DeleteCacheAsync();
        }
    }

    private async Task LoadCacheSizesAsync()
    {
        if (_isCacheChanged)
        {
            return;
        }

        _isCacheChanged = true;

        try
        {
            IsLoading = true;

            var imageSize = await _storageService.GetUsedImageCacheSizeAsync();
            ImageCacheSize = $"{Math.Round(Convert.ToDecimal(imageSize / 1024f / 1024f), 2)} MB";

            var audioCacheSize = await _storageService.GetAudioCacheSizeAsync();
            var sizeInMB = audioCacheSize / 1024f / 1024f;
            var sizeInGB = sizeInMB / 1024f;

            AudioCacheSize = sizeInGB >= 1
                ? $"{Math.Round(Convert.ToDecimal(sizeInGB), 2)} GB"
                : $"{Math.Round(Convert.ToDecimal(sizeInMB), 2)} MB";
        }
        finally
        {
            _isCacheChanged = false;
            IsLoading = false;
        }
    }

    private async Task DeleteCacheAsync()
    {
        try
        {
            IsLoading = true;

            await _storageService.DeleteCacheAsync();

            // Send message to notify other components that cache was cleared
            Messenger.Send(new CacheChangedMessage(CacheChangeMode.ImageCacheCleared));

            await LoadCacheSizesAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }
}