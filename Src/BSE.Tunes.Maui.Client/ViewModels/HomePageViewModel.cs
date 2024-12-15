using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Navigation;
using Prism.Navigation.Regions;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class HomePageViewModel : ViewModelBase, IInitialize
    {
        private readonly IRegionManager _regionManager;
        private readonly IResourceService _resourceService;
        private readonly IEventAggregator _eventAggregator;
        private ICommand? _refreshCommand;
        private bool _isRefreshing;

        public ICommand RefreshCommand => _refreshCommand ??= new DelegateCommand(RefreshView);

        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                SetProperty<bool>(ref _isRefreshing, value);
            }
        }

        public HomePageViewModel(
            INavigationService navigationService,
            IRegionManager regionManager,
            IResourceService resourceService,
            IEventAggregator eventAggregator) : base(navigationService)

        {
            _regionManager = regionManager;
            _resourceService = resourceService;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<AlbumSelectedEvent>().Subscribe(SelectAlbum, ThreadOption.UIThread);

        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            
        }

        private void RefreshView()
        {
            //_eventAggregator.GetEvent<HomePageRefreshEvent>().Publish();
            IsRefreshing = false;
        }

        public void Initialize(INavigationParameters parameters)
        {
            _regionManager.RequestNavigate("AlbumsCarousel", nameof(AlbumsCarouselView));
            _regionManager.RequestNavigate("FeaturedAlbums", nameof(FeaturedAlbumsView));
            _regionManager.RequestNavigate("FeaturedPlaylists", nameof(FeaturedPlaylistsView));
            _regionManager.RequestNavigate("RandomPlayerButton", nameof(RandomPlayerButtonView));
        }

        private void SelectAlbum(Album album)
        {
            _ = SelectAlbumAsync(album);
        }

        private async Task SelectAlbumAsync(Album album)
        {
            var navigationParams = new NavigationParameters
            {
                { "album", album }
            };
            await NavigationService.NavigateAsync($"{nameof(AlbumDetailPage)}", navigationParams);
        }
    }
}
