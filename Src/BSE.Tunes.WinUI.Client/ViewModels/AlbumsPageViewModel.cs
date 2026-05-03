using BSE.Tunes.Shared.Services.Services;
using BSE.Tunes.WinUI.Client.Collections;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Models;
using BSE.Tunes.WinUI.Client.Views;
using BSEtunes.Contracts.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class AlbumsPageViewModel : RefreshableViewModel
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly INavigationService _navigationService;
        
        [ObservableProperty]
        private IncrementalObservableCollection<CarouselItem> _items = default!;

        [ObservableProperty]
        private ObservableCollection<CarouselItem> _selectedItems = [];

        public AlbumsPageViewModel(
            IDataService dataService,
            IMessenger messenger,
            IImageService imageService,
            INavigationService navigationService) : base(messenger)
        {
            _dataService = dataService;
            _imageService = imageService;
            _navigationService = navigationService;

            Initialize();
        }

        protected override async Task LoadDataAsync()
        {
            Items?.Clear();
            IsBusy = true;

            try
            {
                int pageSize = 30;
                int pageNumber = 1;
                
                PagedResult<Album> pagedAlbums = await _dataService.GetPagedAlbums(
                    null,
                    null,
                    null,
                    null,
                    null,
                    pageNumber,
                    pageSize,
                    AlbumSortOption.Artist);

                uint totalCount = (uint)(pagedAlbums?.TotalCount ?? 0);

                Items = new IncrementalObservableCollection<CarouselItem>(
                    totalCount,
                    (uint count) =>
                    {
                        async Task<Microsoft.UI.Xaml.Data.LoadMoreItemsResult> loadFunc()
                        {
                            var pagedAlbums = await _dataService.GetPagedAlbums(
                                null, null, null, null, null,
                                pageNumber, pageSize, AlbumSortOption.Artist);

                            if (pagedAlbums?.Items != null)
                            {
                                foreach (var album in pagedAlbums.Items)
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
                            
                            pageNumber++;
                            
                            return new Microsoft.UI.Xaml.Data.LoadMoreItemsResult
                            {
                                Count = (uint)(pagedAlbums?.Items?.Count ?? 0)
                            };
                        }
                        return loadFunc().AsAsyncOperation();
                    });

                // Add the first page results
                if (pagedAlbums?.Items != null)
                {
                    foreach (var album in pagedAlbums.Items)
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
                    pageNumber++;
                }


            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SelectItemAsync(CarouselItem? item)
        {
            if (item?.Data != null)
            {
                await _navigationService.NavigateToAsync(nameof(AlbumDetailPage), item.Data);
            }
        }
    }
}