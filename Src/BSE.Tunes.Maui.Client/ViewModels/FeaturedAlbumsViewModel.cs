using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class FeaturedAlbumsViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IImageService _imageService;
        private readonly IDataService _dataService;
        private ObservableCollection<GridPanel>? _items;
        private ICommand? _selectItemCommand;

        public ObservableCollection<GridPanel> Items => _items ??= [];

        public ICommand SelectItemCommand => _selectItemCommand
            ?? (_selectItemCommand = new Command<GridPanel>(SelectItem));

        public FeaturedAlbumsViewModel(
            INavigationService navigationService,
            IEventAggregator eventAggregator,
            IImageService imageService,
            IDataService dataService) : base(navigationService)
        {
            _eventAggregator = eventAggregator;
            _imageService = imageService;
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
            var albums = await _dataService.GetNewestAlbums(20);
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
                            ImageSource = _imageService.GetBitmapSource(album.AlbumId),
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
