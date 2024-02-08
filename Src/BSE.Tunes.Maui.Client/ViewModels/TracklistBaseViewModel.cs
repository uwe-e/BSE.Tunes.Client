using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class TracklistBaseViewModel : ViewModelBase
    {

        private ObservableCollection<GridPanel>? _items;
        private string? _imageSource;

        private ICommand? _openFlyoutCommand;
        private readonly IFlyoutNavigationService _flyoutNavigationService;

        public ICommand OpenFlyoutCommand => _openFlyoutCommand
           ??= new DelegateCommand<object>(OpenFlyout);


        public ObservableCollection<GridPanel> Items => _items ??= [];

        public string? ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                SetProperty<string>(ref _imageSource, value);
            }
        }

        public TracklistBaseViewModel(
            INavigationService navigationService,
            IFlyoutNavigationService flyoutNavigationService) : base(navigationService)
        {
            _flyoutNavigationService = flyoutNavigationService;
        }
        
        protected async void OpenFlyout(object obj)
        {
            var source = new PlaylistActionContext
            {
                Data = obj,
            };

            if (obj is GridPanel item)
            {
                source.Data = item.Data;
            }

            var navigationParams = new NavigationParameters{
                        { "source", source }
            };

            await _flyoutNavigationService.ShowFlyoutAsync(nameof(PlaylistActionToolbarPage), navigationParams);
        }
    }
}
