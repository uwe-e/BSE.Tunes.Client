using BSE.Tunes.WinUI.Client.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Views
{
    public sealed partial class EndpointConfigurationPage : Page
    {
        public EndpointConfigurationViewModel ViewModel
        {
            get;
        }

        public EndpointConfigurationPage()
        {
            ViewModel = App.GetService<EndpointConfigurationViewModel>();
            InitializeComponent();
        }
    }
}
