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
    public partial class SearchPageViewModel : ViewModelBase
    {
        private const int AlbumResultsLimit = 4;
        private const int TrackResultsLimit = 5;

        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly INavigationService _navigationService;
        private readonly IMediaManager _mediaManager;
        private readonly IResourceService _resourceService;
        private CancellationTokenSource? _searchCancellationTokenSource;

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
        public partial IncrementalObservableCollection<CarouselItem> Albums { get; set; } = default!;

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
        }

        public override void OnNavigatedTo(object parameter)
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

        private async Task LoadSearchResultsAsync(string queryText, CancellationToken cancellationToken)
        {
            IsBusy = true;
            HasNoResults = false;

            try
            {
                var pageNumber = 1;

                var albumTask = _dataService.GetAlbumSearchResults(queryText, pageNumber, AlbumResultsLimit, cancellationToken);
                var trackTask = _dataService.GetTrackSearchResults(queryText, 1, TrackResultsLimit, cancellationToken);

                await Task.WhenAll(albumTask, trackTask);

                var albumsResult = await albumTask;
                var tracksResult = await trackTask;

                Albums?.Clear();

                uint totalCount = (uint)(albumsResult?.TotalCount ?? 0);

                Albums = new IncrementalObservableCollection<CarouselItem>(
                totalCount,
                (uint count) =>
                {
                    async Task<Microsoft.UI.Xaml.Data.LoadMoreItemsResult> loadFunc()
                    {
                        var result = await _dataService.GetAlbumSearchResults(
                            queryText, pageNumber, 1, cancellationToken);

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
                                    Albums?.Add(new CarouselItem
                                    {
                                        Title = album.Title ?? string.Empty,
                                        SubTitle = album.Artist?.Name ?? string.Empty,
                                        ImagePath = _imageService.GetBitmapSource(album.AlbumId, true),
                                        Data = album
                                    });
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

                if (albumsResult?.Items is { Count: > 0 } items)
                {
                    int albumCount = items.Count;

                    for (int i = 0; i < albumCount; i++)
                    {
                        var album = items[i];
                        if (album != null)
                        {
                            Albums.Add(new CarouselItem
                            {
                                Title = album.Title ?? string.Empty,
                                SubTitle = album.Artist?.Name ?? string.Empty,
                                ImagePath = _imageService.GetBitmapSource(album.AlbumId, true),
                                Data = album
                            });
                        }
                    }
                    pageNumber++;
                }

                //if (albumsResult?.Items is { Count: > 0 } albumItems)
                //{
                //    int albumCount = albumItems.Count;
                //    for (int i = 0; i < albumCount; i++)
                //    {
                //        var item = albumItems[i];
                //        if (item != null)
                //        {
                //            Albums.Add(new CarouselItem
                //            {
                //                Title = item.Title ?? string.Empty,
                //                SubTitle = item.Artist?.Name ?? string.Empty,
                //                ImagePath = _imageService.GetBitmapSource(item.AlbumId, true),
                //                Data = item
                //            });
                //        }
                //    }
                //}

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
                HasMoreAlbums = albumsResult?.HasNextPage ?? false;
                HasTracks = Tracks.Count > 0;
                HasMoreTracks = tracksResult?.HasNextPage ?? false;

                HasNoResults = !HasAlbums && !HasTracks;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                Albums.Clear();
                Tracks.Clear();
                HasAlbums = false;
                HasTracks = false;
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
