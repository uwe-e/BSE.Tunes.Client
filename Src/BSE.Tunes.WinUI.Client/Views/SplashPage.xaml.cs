using BSE.Tunes.WinUI.Client.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Views
{
    public sealed partial class SplashPage : Page
    {
        public SplashPageViewModel ViewModel
        {
            get;
        }

        public SplashPage()
        {
            ViewModel = App.GetService<SplashPageViewModel>();
            InitializeComponent();
        }
    }
}
