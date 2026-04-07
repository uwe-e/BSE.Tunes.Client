using BSE.Tunes.WinUI.Client.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly IMessenger _messenger;

    [ObservableProperty]
    private bool _isRefreshing;

    public MainViewModel(IMessenger messenger)
    {
        _messenger = messenger;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        _messenger.Send(new RefreshRequestedMessage());

        // Give ViewModels time to complete refresh
        await Task.Delay(500);
    }
}