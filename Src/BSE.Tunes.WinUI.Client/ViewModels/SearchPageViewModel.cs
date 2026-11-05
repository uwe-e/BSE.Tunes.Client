using BSE.Tunes.WinUI.Client.Collections;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Models;
using BSE.Tunes.WinUI.Client.Views;
using BSEtunes.Contracts.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class SearchPageViewModel : ViewModelBase
    {
        private const int AlbumResultsLimit = 1;
        private const int TrackResultsLimit = 5;

        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly INavigationService _navigationService;
        private readonly IMediaManager _mediaManager;
        private readonly IResourceService _resourceService;
        private CancellationTokenSource? _searchCancellationTokenSource;
        private bool _isLoadingVisibleAlbums;

        [ObservableProperty]
        public partial string? TitleText { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial bool HasAlbums { get; set; }

        [ObservableProperty]
        public partial bool HasMoreAlbums { get; set; }

        [ObservableProperty]
        public partial bool HasTracks { get; set; }

        [ObservableProperty]
        public partial bool HasMoreTracks { get; set; } 

        [ObservableProperty]
        public partial bool HasNoResults { get; set; }

        [ObservableProperty]
        public partial IncrementalLoadingCollection<AlbumSearchIncrementalSource, CarouselItem> Albums { get; set; } = default!;

        [ObservableProperty]
        public partial ObservableCollection<TrackItem> Tracks { get; set; } = [];

        public SearchPageViewModel(
            IResourceService resourceService,
            IDataService dataService,
            IImageService imageService,
            INavigationService navigationService,
            IMediaManager mediaManager)
        {
            _resourceService = resourceService;
            _dataService = dataService;
            _imageService = imageService;
            _navigationService = navigationService;
            _mediaManager = mediaManager;

            Albums = new IncrementalLoadingCollection<AlbumSearchIncrementalSource, CarouselItem>(
                new AlbumSearchIncrementalSource(_dataService, _imageService, string.Empty, CancellationToken.None),
                AlbumResultsLimit);
        }

//        private async Task FetchAlbumsPageInternalAsync(int pageIndex, int limit, CancellationToken cancellationToken)
//        {
//            //if (string.IsNullOrWhiteSpace(_currentQuery))
//            //{
//            //    return Array.Empty<CarouselItem>();
//            //}

//            try
//            {
//                pageIndex++; // Increment pageIndex to match the API's 1-based indexing

//                // Hier wird dein originaler Datenservice aufgerufen
//                var result = await _dataService.GetAlbumSearchResults(
//                    _currentQuery,
//                    pageIndex,
//                    limit,
//                    cancellationToken
//                );

//List<CarouselItem> results = new List<CarouselItem>();

//                if (result?.Items is { Count: > 0 } items)
//                {
//                    int albumCount = items.Count;

                    

//                    for (int i = 0; i < albumCount; i++)
//                    {
//                        var album = items[i];
//                        if (album != null)
//                        {
//                            Albums.Add(new CarouselItem
//                            {
//                                Title = album.Title ?? string.Empty,
//                                SubTitle = album.Artist?.Name ?? string.Empty,
//                                ImagePath = _imageService.GetBitmapSource(album.AlbumId, true),
//                                Data = album
//                            });
//                        }
//                    }


//                }
////return results;

                    
//            }
//            catch (Exception ex)
//            {
//                // Fehlerbehandlung (z.B. Logging). 
//                // Die Collection fängt dies ab und setzt HasMoreItems auf false.
//                throw;
//            }
//        }

        public async override void OnNavigatedTo(object parameter)
        {
            if (parameter is string queryText)
            {
                TitleText = string.Format(
                    _resourceService.GetString("SearchPage_Title_Text"), queryText);

                _searchCancellationTokenSource?.Cancel();
                _searchCancellationTokenSource = new CancellationTokenSource();

                _ = LoadSearchResultsAsync(queryText, _searchCancellationTokenSource.Token);
            }
            
            base.OnNavigatedTo(parameter);
        }

        [RelayCommand]
        private async Task SelectAlbumAsync(CarouselItem? item)
        {
            if (item?.Data != null)
            {
                await _navigationService.NavigateToAsync(nameof(AlbumDetailPage), item.Data);
            }
        }

        [RelayCommand]
        private void PlayTrack(TrackItem? item)
        {
            if (item != null)
            {
                _ = _mediaManager.PlayTracksAsync([item.Id], PlayerMode.Song);
            }
        }

        [RelayCommand]
        private async Task EnsureVisibleAlbumCapacityAsync(int visibleCount)
        {
            if (_isLoadingVisibleAlbums
                || visibleCount <= 0
                || Albums is not ISupportIncrementalLoading incrementalAlbums)
            {
                return;
            }

            _isLoadingVisibleAlbums = true;

            try
            {
                while (Albums.Count < visibleCount && incrementalAlbums.HasMoreItems)
                {
                    await incrementalAlbums.LoadMoreItemsAsync(AlbumResultsLimit);
                }
            }
            finally
            {
                _isLoadingVisibleAlbums = false;
            }
        }

        private async Task LoadSearchResultsAsync(string queryText, CancellationToken cancellationToken)
        {
            IsBusy = true;
            HasNoResults = false;

            try
            {
                var trackTask = _dataService.GetTrackSearchResults(queryText, 1, TrackResultsLimit, cancellationToken);

                /*
                 * The list view displays only as many items as will fit in the window. For this reason, the "itemsperPage" parameter always has the value 1.
                 * If the size of the window changes, the listview will request only the items that are required to fill the visible area.
                 * The EnsureVisibleAlbumCapacityAsync method is called to load more items if necessary.
                 * 
                 * For this to work, the Listview must have the IncrementalLoadingTrigger property set to "None" (see SearchPage.xaml).
                 */
                Albums = new IncrementalLoadingCollection<AlbumSearchIncrementalSource, CarouselItem>(
                    new AlbumSearchIncrementalSource(
                        _dataService,
                        _imageService,
                        queryText,
                        cancellationToken,
                        hasMoreItems => HasMoreAlbums = hasMoreItems),
                    AlbumResultsLimit);

                var loadResult = await Albums.LoadMoreItemsAsync(AlbumResultsLimit);
                var tracksResult = await trackTask;

                Tracks.Clear();
                if (tracksResult?.Items is { Count: > 0 } trackItems)
                {
                    int trackCount = trackItems.Count;
                    for (int i = 0; i < trackCount; i++)
                    {
                        var item = trackItems[i];
                        if (item != null)
                        {
                            Tracks.Add(TrackItem.FromTrack(item));
                        }
                    }
                }

                HasAlbums = Albums.Count > 0;
                if (loadResult.Count == 0)
                {
                    HasMoreAlbums = false;
                }
                HasTracks = Tracks.Count > 0;
                HasMoreTracks = tracksResult?.HasNextPage ?? false;

                HasNoResults = !HasAlbums && !HasTracks;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                Albums?.Clear();
                Tracks.Clear();
                HasAlbums = false;
                HasTracks = false;
                HasMoreAlbums = false;
                HasMoreTracks = false;
                HasNoResults = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        //private async Task GetTrackResultsAsync(string searchPhrase)
        //{
        //    await GetSearchResultsAsync(
        //        () => _dataService.GetTrackSearchResults(searchPhrase, 1, 4),
        //        Tracks,
        //        item => new CarouselItem
        //        {
        //            Title = item.Name,
        //            SubTitle = item.Album.Artist.Name,
        //            ImagePath = _imageService.GetBitmapSource(item.Album.AlbumId, true),
        //            Data = item
        //        },
        //        hasMore => HasMoreTracks = hasMore,
        //        hasResults => HasTracks = hasResults);
        //}

        //private async Task GetSearchResultsAsync<T>(
        //    Func<Task<PagedResult<T>>> getResults,
        //    ObservableCollection<CarouselItem> collection,
        //    Func<T, CarouselItem> createPanel,
        //    Action<bool> setHasMore,
        //    Action<bool> setHasResults)
        //{
        //    var pagedResult = await getResults();
        //    if (pagedResult?.Items.Count == 0)
        //    {
        //        setHasResults(false);
        //        collection.Clear();
        //    }
        //    else
        //    {
        //        setHasResults(true);
        //        setHasMore(pagedResult.HasNextPage);

        //        int newCount = Math.Min(pagedResult.Items.Count, 4);

        //        // Remove excess items first
        //        while (collection.Count > newCount)
        //        {
        //            collection.RemoveAt(collection.Count - 1);
        //        }

        //        // Update or insert items in reverse order
        //        for (int i = 0; i < newCount; i++)
        //        {
        //            var item = pagedResult.Items[newCount - 1 - i];
        //            var panel = createPanel(item);

        //            if (i < collection.Count)
        //            {
        //                collection[i] = panel;
        //            }
        //            else
        //            {
        //                collection.Add(panel);
        //            }
        //        }
        //    }
        //}
    }
}
