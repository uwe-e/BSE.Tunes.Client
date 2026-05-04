using BSE.Tunes.WinUI.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class FeaturedPlaylistViewModel : RefreshableViewModel
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly Contracts.Services.IResourceService _resourceService;
        [ObservableProperty]
        private ObservableCollection<CarouselItem> _items = [];

        [ObservableProperty]
        private bool _isBusy;

        public FeaturedPlaylistViewModel(
            IDataService dataService,
            IImageService imageService,
            Contracts.Services.IResourceService resourceService,
            IMessenger messenger) : base(messenger)
        {
            _dataService = dataService;
            _imageService = imageService;
            _resourceService = resourceService;
            Initialize();
        }

        protected override async Task LoadDataAsync()
        {
            Items.Clear();
            IsBusy = true;

            try
            {
                var pagedResult = await _dataService.GetPagedPlaylistsByOwnerAsync(1, 10);
                if (pagedResult.Items != null && pagedResult.Items.Count > 0)
                {
                    var carouselItems = new List<CarouselItem>(pagedResult.Items.Count);
                    
                    foreach (var playlist in pagedResult.Items)
                    {
                        if (playlist != null)
                        {
                            var imageSource = await _imageService.GetComposedBitmapSourceAsync(
                                playlist.Id,
                                playlist.CoverAlbumIds);

                            carouselItems.Add(new CarouselItem
                            {
                                Title = playlist.Name ?? string.Empty,
                                SubTitle = $"{playlist.NumberEntries} {_resourceService.GetString("FeaturedPlaylist_PlaylistItem_PartNumberOfEntries")}",
                                ImageSource = imageSource,
                                Data = playlist
                            });
                        }
                    }
                    
                    foreach (var item in carouselItems)
                    {
                        Items.Add(item);
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
