﻿using BSE.Tunes.Maui.Client.Events;
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
        private readonly IImageService _imageService;
        private ObservableCollection<GridPanel> _items;
        private ICommand _selectItemCommand;

        public ObservableCollection<GridPanel> Items => _items ??= [];

        public ICommand SelectItemCommand => _selectItemCommand ??= new DelegateCommand<GridPanel>(SelectItem);

        public AlbumsCarouselViewModel(
            INavigationService navigationService,
            IEventAggregator eventAggregator,
            IResourceService resourceService,
            IDataService dataService,
            IImageService imageService) : base(navigationService)
        {
            _eventAggregator = eventAggregator;
            _resourceService = resourceService;
            _dataService = dataService;
            _imageService = imageService;
            
            LoadData();
            
            _eventAggregator.GetEvent<HomePageRefreshEvent>().Subscribe(() =>
            {
                IsBusy = true;
                LoadData();
            });
        }

        private void LoadData()
        {
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
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
                            SubTitle = album.Artist?.Name,
                            ImageSource = _imageService.GetBitmapSource(album.AlbumId, false),
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
