using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using BSE.Tunes.WinUI.Client.Messages;

namespace BSE.Tunes.WinUI.Client.ViewModels;

public abstract partial class RefreshableViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool _isBusy;

    protected RefreshableViewModel(IMessenger messenger) : base(messenger)
    {
        // Activate messenger support
        IsActive = true;
    }

    /// <summary>
    /// Call this method from derived class constructor after initializing dependencies
    /// </summary>
    protected void Initialize()
    {
        _ = LoadDataAsync();
    }

    /// <summary>
    /// Override this method in derived classes to implement data loading logic
    /// </summary>
    protected abstract Task LoadDataAsync();

    protected override void OnActivated()
    {
        base.OnActivated();
        
        // Register for refresh messages
        Messenger.Register<RefreshableViewModel, RefreshRequestedMessage>(this, async (r, m) =>
        {
            await r.LoadDataAsync();
        });
    }
}