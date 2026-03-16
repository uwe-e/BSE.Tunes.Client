using BSE.Tunes.WinUI.Client.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginPageViewModel ViewModel
        {
            get;
        }
        public LoginPage()
        {
            ViewModel = App.GetService<LoginPageViewModel>();
            InitializeComponent();
        }
    }
}
