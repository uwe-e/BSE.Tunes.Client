using BSE.Tunes.WinUI.Client.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
