using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class AlbumsCarouselViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IResourceService _resourceService;
        private readonly IDataService _dataService;
        private ObservableCollection<GridPanel>? _items;
        private ICommand? _selectItemCommand;

        public ObservableCollection<GridPanel> Items => _items ??= [];

        public ICommand SelectItemCommand => _selectItemCommand ??= new DelegateCommand<GridPanel>(SelectItem);

        public AlbumsCarouselViewModel(
            INavigationService navigationService,
            IEventAggregator eventAggregator,
            IResourceService resourceService,
            IDataService dataService) : base(navigationService)
        {
            _eventAggregator = eventAggregator;
            _resourceService = resourceService;
            _dataService = dataService;

            _eventAggregator.GetEvent<HomePageRefreshEvent>().Subscribe(() =>
            {
                IsBusy = true;
                LoadData();
            });

            LoadData();
        }

        private async void LoadData()
        {
            Items.Clear();
            var albums = await _dataService.GetFeaturedAlbums(6);
            if (albums != null)
            {
                foreach (var album in albums)
                {
                    if (album != null)
                    {
                        Items.Add(new GridPanel
                        {
                            Title = album.Title,
                            SubTitle = album.Artist.Name,
                            ImageSource = _dataService.GetImage(album.AlbumId)?.AbsoluteUri,
                            Data = album
                        });
                    }
                }
                IsBusy = false;
            }
        }

        private void SelectItem(GridPanel panel)
        {
            if (panel?.Data is Album album)
            {
                _eventAggregator.GetEvent<AlbumSelectedEvent>().Publish(album);
            }
        }
              
    }
}
