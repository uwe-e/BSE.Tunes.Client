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
        public partial Album? Album { get; set; }

        [ObservableProperty]
        public partial TrackItem? SelectedTrack { get; set; }

        [ObservableProperty]
        public partial bool HasFurtherAlbums { get; set; }

        [ObservableProperty]
        
        internal partial FuncIncrementalLoadingCollection<CarouselItem> FurtherAlbums { get; set; } = default!;

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
            FurtherAlbums?.Clear();

            const int pageSize = 3;

            FurtherAlbums = new FuncIncrementalLoadingCollection<CarouselItem>(
                async (pageIndex, pageSize, cancellationToken) =>
                {
                    var result = await DataService.GetPagedAlbums(
                        null,
                        Album?.Artist?.Id,
                        null,
                        null,
                        null,
                        pageIndex + 1,
                        pageSize,
                        AlbumSortOption.Title);
                    
                    if (result?.Items is not { Count: > 0 } items)
                    {
                        return [];
                    }

                    var carouselItems = new List<CarouselItem>(items.Count);
                    for (int i = 0; i < items.Count; i++)
                    {
                        var album = items[i];
                        if (album != null && album.AlbumId != Album.AlbumId)
                        {
                            carouselItems.Add(CreateCarouselItem(album));
                        }
                    }

                    return carouselItems;
                }, pageSize);

            await FurtherAlbums.LoadMoreItemsAsync((uint)pageSize);

            HasFurtherAlbums = FurtherAlbums.Count > 0;
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
