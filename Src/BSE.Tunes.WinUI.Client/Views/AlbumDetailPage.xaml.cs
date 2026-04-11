using BSE.Tunes.WinUI.Client.ViewModels;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BSE.Tunes.WinUI.Client.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumDetailPage : Page
    {
        public AlbumDetailPageViewModel ViewModel { get; }
        
        public AlbumDetailPage()
        {
            ViewModel = App.GetService<AlbumDetailPageViewModel>();
            InitializeComponent();
        }
    }
}
