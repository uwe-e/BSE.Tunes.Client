using BSE.Tunes.WinUI.Client.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Views;

public sealed partial class RemoveEndpointSettingsPage : Page
{
    public RemoveEndpointSettingsPageViewModel ViewModel { get; }

    public RemoveEndpointSettingsPage()
    {
        ViewModel = App.GetService<RemoveEndpointSettingsPageViewModel>();
        InitializeComponent();
    }

    //protected override void OnNavigatedTo(NavigationEventArgs e)
    //{
    //    base.OnNavigatedTo(e);
    //    ViewModel.OnNavigatedTo(e.Parameter);
    //}

    //protected override void OnNavigatedFrom(NavigationEventArgs e)
    //{
    //    base.OnNavigatedFrom(e);
    //    ViewModel.OnNavigatedFrom();
    //}
}