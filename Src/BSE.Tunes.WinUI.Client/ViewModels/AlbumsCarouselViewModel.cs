using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Models;
using BSE.Tunes.WinUI.Client.Views;
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
        private readonly INavigationService _navigationService;
        [ObservableProperty]
        private ObservableCollection<CarouselItem> _items = [];

        public AlbumsCarouselViewModel(
            IDataService dataService,
            IImageService imageService,
            INavigationService navigationService,
            IMessenger messenger) : base(messenger)
        {
            _dataService = dataService;
            _imageService = imageService;
            _navigationService = navigationService;
            
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
        private async Task SelectItem(CarouselItem? item)
        {
            if (item?.Data != null)
            {
                await _navigationService.NavigateToAsync(nameof(AlbumDetailPage), item.Data);
            }
        }
    }
}
