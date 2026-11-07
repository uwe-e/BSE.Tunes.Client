using BSE.Tunes.WinUI.Client.Collections;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Messages;
using BSE.Tunes.WinUI.Client.Models;
using BSE.Tunes.WinUI.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class PlaylistsPageViewModel : RefreshableViewModel, IRecipient<PlaylistChangedMessage>, IRecipient<PlaylistCreatedMessage>
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly INavigationService _navigationService;
        private readonly IResourceService _resourceService;
        private string _cachedResourceString = string.Empty;
        
        [ObservableProperty]
        internal partial FuncIncrementalLoadingCollection<CarouselItem> Items { get; set; } = default!;
        
        //[ObservableProperty]
        //private ObservableCollection<CarouselItem> _selectedItems = [];

        public PlaylistsPageViewModel(
            IDataService dataService,
            IImageService imageService,
            INavigationService navigationService,
            IResourceService resourceService,
            IMessenger messenger) : base(messenger)
        {
            _dataService = dataService;
            _imageService = imageService;
            _navigationService = navigationService;
            _resourceService = resourceService;
            
            Initialize();

            Messenger.Register<PlaylistsPageViewModel, PlaylistDeletedMessage>(this, async (r, m) =>
            {
                // Remove the deleted playlist from the collection
                var itemToRemove = r.Items.FirstOrDefault(i => i.Data is Playlist p && p.Id == m.PlaylistId);
                if (itemToRemove != null)
                {
                    r.Items.Remove(itemToRemove);
                }
            });
        }

        protected override async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                
                const int pageSize = 10;                
                
                Items?.Clear();

                // Cache resource string once
                _cachedResourceString = _resourceService.GetString("PlaylistsPage_PlaylistItem_PartNumberOfEntries");

                Items = new FuncIncrementalLoadingCollection<CarouselItem>(
                async (pageIndex, pageSize, cancellationToken) =>
                {
                    var result = await _dataService.GetPagedPlaylistsByOwnerAsync(
                        pageIndex + 1,
                        pageSize);

                    if (result?.Items is not { Count: > 0 } items)
                    {
                        return [];
                    }

                    var carouselItems = new List<CarouselItem>(items.Count);

                    for (int i = 0; i < items.Count; i++)
                    {
                        var playlist = items[i];
                        if (playlist != null)
                        {
                            carouselItems.Add(await CreateCarouselItemAsync(playlist));
                        }
                    }

                    return carouselItems;
                },
                pageSize);

                await Items.LoadMoreItemsAsync((uint)pageSize);

            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<CarouselItem> CreateCarouselItemAsync(Playlist playlist)
        {
            return new CarouselItem
            {
                Title = playlist.Name ?? string.Empty,
                SubTitle = $"{playlist.NumberEntries} {_cachedResourceString}",
                ImagePath = await _imageService.GetComposedBitmapSourceAsync(
                    playlist.Id,
                    playlist.CoverAlbumIds,
                    asThumbnail: true),
                IgnoreImageCache = true,
                Data = playlist
            };
        }

        void IRecipient<PlaylistChangedMessage>.Receive(PlaylistChangedMessage message)
        {
            _ = UpdatePlaylistItem(message.PlaylistId);
        }

        void IRecipient<PlaylistCreatedMessage>.Receive(PlaylistCreatedMessage message)
        {
            _ = LoadDataAsync();
        }

        private async Task UpdatePlaylistItem(int playlistId)
        {
            CarouselItem? item = Items.FirstOrDefault(i => i.Data is Playlist p && p.Id == playlistId);
            if(item != null)
            {
                // Update the existing item
                var playlist = await _dataService.GetPlaylistById(playlistId);
                if (playlist != null)
                {
                    int index = Items.IndexOf(item);
                    if (index >= 0)
                    {
                        Items[index] = await CreateCarouselItemAsync(playlist);
                    }
                }
            }
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
