using BSE.Tunes.WinUI.Client.ViewModels;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BSE.Tunes.WinUI.Client.Controls;

public sealed partial class PlayerBarView : UserControl
{
    PlayerBarViewModel ViewModel { get; }
    
    public PlayerBarView()
    {
        ViewModel = App.GetService<PlayerBarViewModel>();
        InitializeComponent();
    }
}
