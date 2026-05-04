using BSE.Tunes.WinUI.Client.Collections;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class PlaylistsPageViewModel : RefreshableViewModel
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly INavigationService _navigationService;
        private readonly IResourceService _resourceService;
        private string _cachedResourceString = string.Empty;
        
        [ObservableProperty]
        private IncrementalObservableCollection<CarouselItem> _items = default!;

        [ObservableProperty]
        private ObservableCollection<CarouselItem> _selectedItems = [];

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
        }

        protected override async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                Items?.Clear();

                const int pageSize = 10;
                int pageNumber = 1;

                // Cache resource string once
                _cachedResourceString = _resourceService.GetString("PlaylistsPage_PlaylistItem_PartNumberOfEntries");

                PagedResult<Playlist> result = await _dataService.GetPagedPlaylistsByOwnerAsync(pageNumber, pageSize);

                uint totalCount = (uint)(result?.TotalCount ?? 0);

                Items = new IncrementalObservableCollection<CarouselItem>(
                    totalCount,
                    (uint count) =>
                    {
                        async Task<Microsoft.UI.Xaml.Data.LoadMoreItemsResult> loadFunc()
                        {
                            var result = await _dataService.GetPagedPlaylistsByOwnerAsync(pageNumber, pageSize);

                            uint itemCount = 0;
                            if (result?.Items != null)
                            {
                                var items = result.Items;
                                itemCount = (uint)items.Count;

                                foreach (var playlist in items)
                                {
                                    if (playlist != null)
                                    {
                                        Items.Add(await CreateCarouselItemAsync(playlist));
                                    }
                                }
                            }
                            pageNumber++;

                            return new Microsoft.UI.Xaml.Data.LoadMoreItemsResult
                            {
                                Count = itemCount
                            };
                        }
                        return loadFunc().AsAsyncOperation();
                    });

                // Add the first page results
                if (result?.Items != null)
                {
                    foreach (var playlist in result.Items)
                    {
                        if (playlist != null)
                        {
                            Items.Add(await CreateCarouselItemAsync(playlist));
                        }
                    }
                    pageNumber++;
                }
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
                ImageSource = await _imageService.GetComposedBitmapSourceAsync(playlist.Id, playlist.CoverAlbumIds, asThumbnail: true),
                Data = playlist
            };
        }

        [RelayCommand]
        private async Task SelectItemAsync(CarouselItem? item)
        {
            if (item?.Data != null)
            {
                //await _navigationService.NavigateToAsync(nameof(AlbumDetailPage), item.Data);
            }
        }
    }
}
