using BSE.Tunes.WinUI.Client.ViewModels;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BSE.Tunes.WinUI.Client.Controls
{
    public sealed partial class FeaturedAlbumsView : UserControl
    {
        public FeaturedAlbumsViewModel ViewModel { get; }
        
        public FeaturedAlbumsView()
        {
            ViewModel = App.GetService<FeaturedAlbumsViewModel>();
            InitializeComponent();
        }
    }
}
