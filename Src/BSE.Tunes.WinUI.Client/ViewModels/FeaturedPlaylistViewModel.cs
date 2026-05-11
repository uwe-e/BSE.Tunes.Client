using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Messages;
using BSE.Tunes.WinUI.Client.Models;
using BSE.Tunes.WinUI.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class FeaturedPlaylistViewModel : RefreshableViewModel, IRecipient<PlaylistChangedMessage>
    {
        private readonly IDataService _dataService;
        private readonly INavigationService _navigationService;
        private readonly IImageService _imageService;
        private readonly IResourceService _resourceService;
        
        [ObservableProperty]
        private ObservableCollection<CarouselItem> _items = [];

        [ObservableProperty]
        private bool _isBusy;

        public FeaturedPlaylistViewModel(
            IDataService dataService,
            INavigationService navigationService,
            IImageService imageService,
            IResourceService resourceService,
            IMessenger messenger) : base(messenger)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            _imageService = imageService;
            _resourceService = resourceService;
            
            Initialize();

            Messenger.Register<FeaturedPlaylistViewModel, PlaylistDeletedMessage>(this, async (r, m) =>
            {
                // Remove the deleted playlist from the collection
                var itemToRemove = r.Items.FirstOrDefault(i => i.Data is Playlist p && p.Id == m.PlaylistId);
                if (itemToRemove != null)
                {
                    r.Items.Remove(itemToRemove);
                }
            });


        }

        public async Task Receive(PlaylistChangedMessage message)
        {
            CarouselItem? itemToUpdate = Items.FirstOrDefault(i => i.Data is Playlist p && p.Id == message.PlaylistId);
            if (itemToUpdate != null)
            {
                // Update the existing item
                var updatedPlaylist = await _dataService.GetPlaylistById(message.PlaylistId);
                if (updatedPlaylist != null)
                {
                    itemToUpdate.Title = updatedPlaylist.Name ?? string.Empty;
                    itemToUpdate.SubTitle = $"{updatedPlaylist.NumberEntries} {_resourceService.GetString("FeaturedPlaylist_PlaylistItem_PartNumberOfEntries")}";
                    
                    var imageSource = await _imageService.GetComposedBitmapSourceAsync(
                        updatedPlaylist.Id,
                        updatedPlaylist.CoverAlbumIds);
                    
                    itemToUpdate.ImageSource = imageSource;

                    // Notify the UI about the changes 
                    var index = Items.IndexOf(itemToUpdate);
                    if (index >= 0)
                    {
                        Items[index] = itemToUpdate; // This will trigger the UI to refresh the item
                    }
                }
            }

            //await LoadDataAsync();
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

        void IRecipient<PlaylistChangedMessage>.Receive(PlaylistChangedMessage message)
        {
            _ = Receive(message);
        }

        [RelayCommand]
        private async Task SelectItemAsync(CarouselItem? item)
        {
            if (item?.Data != null)
            {
                await _navigationService.NavigateToAsync(nameof(PlaylistDetailPage), item.Data);
            }
        }
    }
}
