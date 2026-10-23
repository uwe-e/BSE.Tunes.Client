using BSE.Tunes.Shared.Services.Extensions;
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
        private readonly IResourceService _resourceService;
        private readonly IMediaManager _mediaManager;
        
        [ObservableProperty]
        private IncrementalObservableCollection<CarouselItem> _items = default!;

        [ObservableProperty]
        private ObservableCollection<CarouselItem> _selectedItems = [];

        [ObservableProperty]
        private ObservableCollection<FlyoutItem> _genreItems = [];

        [ObservableProperty]
        private Genre _selectedGenre = default!;

        public AlbumsPageViewModel(
            IDataService dataService,
            IMessenger messenger,
            IImageService imageService,
            INavigationService navigationService,
            IResourceService resourceService,
            IMediaManager mediaManager) : base(messenger)
        {
            _dataService = dataService;
            _imageService = imageService;
            _navigationService = navigationService;
            _resourceService = resourceService;
            _mediaManager = mediaManager;
            
            Initialize();
        }

        protected override async Task LoadDataAsync()
        {
            IsBusy = true;

            SelectedGenre = new Genre
            {
                Name = _resourceService.GetString("AlbumsPage_FilterPanel_AllGenres-Value")
            };

            try
            {
                GenreItems.Add(new FlyoutItem
                {
                    Text = SelectedGenre.Name,
                    Data = SelectedGenre
                });

                var genres = await _dataService.GetAvailableGenresAsync();
                if (genres is { Count: > 0 })
                {
                    int genreCount = genres.Count;
                    
                    for (int i = 0; i < genreCount; i++)
                    {
                        var genre = genres[i];
                        GenreItems.Add(new FlyoutItem
                        {
                            Text = genre.Name ?? string.Empty,
                            Data = genre
                        });
                    }
                }

                await LoadAlbumsAsync(null);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadAlbumsAsync(string? genreName)
        {
            Items?.Clear();

            const int pageSize = 30;
            int pageNumber = 1;

            var result = await _dataService.GetPagedAlbums(
                genreName,
                null,
                null,
                null,
                null,
                pageNumber,
                pageSize,
                AlbumSortOption.Artist);

            uint totalCount = (uint)(result?.TotalCount ?? 0);

            Items = new IncrementalObservableCollection<CarouselItem>(
                totalCount,
                (uint count) =>
                {
                    async Task<Microsoft.UI.Xaml.Data.LoadMoreItemsResult> loadFunc()
                    {
                        var result = await _dataService.GetPagedAlbums(
                            genreName, null, null, null, null,
                            pageNumber, pageSize, AlbumSortOption.Artist);

                        uint itemCount = 0;
                        if (result?.Items is { Count: > 0 } items)
                        {
                            int albumCount = items.Count;
                            itemCount = (uint)albumCount;

                            for (int i = 0; i < albumCount; i++)
                            {
                                var album = items[i];
                                if (album != null)
                                {
                                    Items.Add(CreateCarouselItem(album));
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
            if (result?.Items is { Count: > 0 } items)
            {
                int albumCount = items.Count;
                
                for (int i = 0; i < albumCount; i++)
                {
                    var album = items[i];
                    if (album != null)
                    {
                        Items.Add(CreateCarouselItem(album));
                    }
                }
                pageNumber++;
            }
        }

        private CarouselItem CreateCarouselItem(Album album)
        {
            return new CarouselItem
            {
                Title = album.Title ?? string.Empty,
                SubTitle = album.Artist?.Name ?? string.Empty,
                ImagePath = _imageService.GetBitmapSource(album.AlbumId, false),
                Data = album
            };
        }

        [RelayCommand]
        private async Task SelectItemAsync(CarouselItem? item)
        {
            if (item?.Data != null)
            {
                await _navigationService.NavigateToAsync(nameof(AlbumDetailPage), item.Data);
            }
        }

        [RelayCommand]
        private async Task PlayRandomAsync()
        {
            var trackIds = await _dataService.GetTrackIdsByGenre(SelectedGenre.Id == 0 ? null : SelectedGenre.Id);

            if (trackIds is { Count: > 0 })
            {
                var tracks = new ObservableCollection<int>(trackIds).ToRandomCollection();
                _mediaManager.Playlist = tracks.ToNavigableCollection();
                await _mediaManager.PlayTracksAsync(PlayerMode.Random);
            }
        }

        [RelayCommand]
        private async Task GenreItemClicked(FlyoutItem? item)
        {
            if (item?.Data is Genre genre)
            {
                IsBusy = true;
                try
                {
                    SelectedGenre = genre;
                    await LoadAlbumsAsync(genre.Id == 0 ? null : genre.Name);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}