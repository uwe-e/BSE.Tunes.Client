using BSE.Tunes.WinUI.Client.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Views;

public sealed partial class RemoveLoginSettingsPage : Page
{
    public RemoveLoginSettingsPageViewModel ViewModel { get; }

    public RemoveLoginSettingsPage()
    {
        ViewModel = App.GetService<RemoveLoginSettingsPageViewModel>();
        InitializeComponent();
    }
}