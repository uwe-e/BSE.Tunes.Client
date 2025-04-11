using BSE.Tunes.Maui.Client.Controls;
using BSE.Tunes.Maui.Client.ViewModels;

namespace BSE.Tunes.Maui.Client.Views;

public partial class PlaylistActionToolbarPage : BottomFlyoutPage
{
	private PlaylistActionToolbarPageViewModel viewModel => viewModel as PlaylistActionToolbarPageViewModel;
	public PlaylistActionToolbarPage(PlaylistActionToolbarPageViewModel vm)
	{
        BindingContext = vm;
        InitializeComponent();
	}
}