using BSE.Tunes.Shared.Services;
using BSE.Tunes.Shared.Services.Models.Contract;
using BSE.Tunes.WinUI.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class AlbumsCarouselViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        //private readonly IImageService _imageService;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private ObservableCollection<CarouselItem> _items = [];

        [ObservableProperty]
        private bool _isBusy;

        public AlbumsCarouselViewModel(
            IDataService dataService,
            //IImageService imageService,
            IMessenger messenger)
        {
            _dataService = dataService;
            //_imageService = imageService;
            _messenger = messenger;

            LoadData();

            // Subscribe to refresh events if needed
            //_messenger.Register<HomePageRefreshMessage>(this, (r, m) =>
            //{
            //    IsBusy = true;
            //    LoadData();
            //});
        }

        private void LoadData()
        {
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            Items.Clear();
            IsBusy = true;

            try
            {
                IList<Album> albums = await _dataService.GetFeaturedAlbums(10);
                if (albums != null)
                {
                    foreach (var album in albums)
                    {
                        if (album != null)
                        {
                            Items.Add(new CarouselItem
                            {
                                Title = album.Title ?? string.Empty,
                                SubTitle = album.Artist?.Name ?? string.Empty,
                                ImageSource = _dataService.GetAlbumCoverUriById(album.AlbumId, false).ToString(),//_imageService.GetBitmapSource(album.AlbumId, false),
                                Data = album
                            });
                        }
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void SelectItem(CarouselItem? item)
        {
            if (item?.Data != null)
            {
                //_messenger.Send(new AlbumSelectedMessage(item.Album));
            }
        }
    }
}
