using BSE.Tunes.WinUI.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class AlbumsCarouselViewModel : RefreshableViewModel
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;

        [ObservableProperty]
        private ObservableCollection<CarouselItem> _items = [];

        public AlbumsCarouselViewModel(
            IDataService dataService,
            IImageService imageService,
            IMessenger messenger) : base(messenger)
        {
            _dataService = dataService;
            _imageService = imageService;

            Initialize(); // Call after dependencies are set
        }

        protected override async Task LoadDataAsync()
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
                                ImageSource = _imageService.GetBitmapSource(album.AlbumId, false),
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
