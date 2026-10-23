using BSE.Tunes.Shared.Services.Extensions;
using BSE.Tunes.WinUI.Client.Collections;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Models;
using BSE.Tunes.WinUI.Client.Views;
using BSEtunes.Contracts.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class AlbumDetailPageViewModel : PlaylistBaseViewModel<TrackItem>
    {
        [ObservableProperty]
        private Album? _album;

        [ObservableProperty]
        private TrackItem? _selectedTrack;

        [ObservableProperty]
        private bool _hasFurtherAlbums;

        [ObservableProperty]
        private ObservableCollection<CarouselItem> _furtherAlbums = [];
        private readonly INavigationService _navigationService;

        public AlbumDetailPageViewModel(
            IDataService dataService,
            IImageService imageService,
            IMediaManager mediaManager,
            IDialogService dialogService,
            IResourceService resourceService,
            INavigationService navigationService) : base(dataService, imageService, mediaManager, dialogService, resourceService)
        {
            _navigationService = navigationService;
        }

        public override void OnNavigatedTo(object parameter)
        {
            base.OnNavigatedTo(parameter);

            if (parameter is Album album)
            {
                _ = LoadAlbumAsync(album.Id);
            }
        }

        public async Task LoadAlbumAsync(int albumId)
        {
            IsBusy = true;

            try
            {
                Album = await DataService.GetAlbumById(albumId);

                if (Album != null)
                {
                    var imagePath = ImageService.GetBitmapSource(Album.AlbumId, false);
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        LoadImageSource(imagePath);
                    }
                    LoadTracks(Album);
                    _ = LoadFurtherAlbumsAsync();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LoadTracks(Album album)
        {
            var trackItems = album.Tracks?
            .Where(t => t != null)
            .Select(t =>
            {
                t.Album = album;
                return TrackItem.FromTrack(t);
            }) ?? Enumerable.Empty<TrackItem>();

            LoadItemsIntoCollection(trackItems);
        }
        private async Task LoadFurtherAlbumsAsync()
        {
            FurtherAlbums.Clear();

            const int pageSize = 3;
            int pageNumber = 1;

            var result = await DataService.GetPagedAlbums(
                null,
                Album.Artist.Id,
                null,
                null,
                null,
                pageNumber,
                pageSize,
                AlbumSortOption.Title);

            uint totalCount = (uint)(result?.TotalCount ?? 0);
            HasFurtherAlbums = totalCount > 1;

            FurtherAlbums = new IncrementalObservableCollection<CarouselItem>(
                totalCount,
                (uint count) =>
                {
                    async Task<Microsoft.UI.Xaml.Data.LoadMoreItemsResult> loadFunc()
                    {
                        var result = await DataService.GetPagedAlbums(
                            null, Album.Artist.Id, null, null, null,
                            pageNumber, pageSize, AlbumSortOption.Title);

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
                                    FurtherAlbums.Add(CreateCarouselItem(album));
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
                        FurtherAlbums.Add(CreateCarouselItem(album));
                    }
                }
                pageNumber++;
            }
        }

        public override void PlayTrack(object? listItemData)
        {
            if (listItemData is TrackItem trackItem)
            {
                var trackIds = new ObservableCollection<int> { trackItem.Id };
                _ = MediaManager.PlayTracksAsync(trackIds, PlayerMode.Song);
            }
        }

        public override void PlayAll()
        {
            var trackIds = new ObservableCollection<int>(Items.OfType<TrackItem>().Select(t => t.Id));
            _ = MediaManager.PlayTracksAsync(trackIds, PlayerMode.CD);
        }

        public override void PlayAllShuffle()
        {
            var trackIds = new ObservableCollection<int>(Items.OfType<TrackItem>().Select(t => t.Id));
            _ = MediaManager.PlayTracksAsync(trackIds.ToRandomCollection(), PlayerMode.CD);
        }

        protected override int GetTrackId(TrackItem item) => item.Id;

        [RelayCommand]
        private async Task SelectItem(CarouselItem? item)
        {
            if (item?.Data != null)
            {
                await _navigationService.NavigateToAsync(nameof(AlbumDetailPage), item.Data);
            }
        }
        
        private CarouselItem CreateCarouselItem(Album album)
        {
            return new CarouselItem
            {
                Title = album.Title ?? string.Empty,
                SubTitle = album.Artist?.Name ?? string.Empty,
                ImagePath = ImageService.GetBitmapSource(album.AlbumId, false),
                Data = album
            };
        }
    }
}
