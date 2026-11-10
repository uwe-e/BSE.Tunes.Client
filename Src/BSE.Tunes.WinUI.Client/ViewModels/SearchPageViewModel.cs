using BSE.Tunes.WinUI.Client.Collections;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Extensions;
using BSE.Tunes.WinUI.Client.Models;
using BSE.Tunes.WinUI.Client.Views;
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
        public partial TrackItem? SelectedTrack { get; set; }

        [ObservableProperty]
        public partial IncrementalLoadingCollection<DelegateIncrementalSource<CarouselItem>, CarouselItem> Albums { get; set; } = default!;

        [ObservableProperty]
        public partial ObservableCollection<TrackItem> Tracks { get; set; } = [];

        [ObservableProperty]
        public partial IList<FlyoutItem> PlaylistMenuItems { get; set; } = [];

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

        public async override void OnNavigatedTo(object parameter)
        {
            if (parameter is string queryText)
            {
                TitleText = string.Format(
                    _resourceService.GetString("SearchPage_Title_Text"), queryText);

                _searchCancellationTokenSource?.Cancel();
                _searchCancellationTokenSource = new CancellationTokenSource();

                _ = LoadSearchResultsAsync(queryText, _searchCancellationTokenSource.Token);

                //PlaylistMenuItems?.Clear();
                PlaylistMenuItems = await LoadPlaylistMenusAsync();
                //var menuitems = await LoadPlaylistMenusAsync();
                //for (int i = 0; i < menuitems.Count; i++)
                //{
                //    PlaylistMenuItems?.Add(menuitems[i]);
                //}
            }
            
            base.OnNavigatedTo(parameter);
        }

        private async Task<IList<FlyoutItem>> LoadPlaylistMenusAsync()
        {
            var playlists = await _dataService.GetAllPlaylists();

            var flyoutItems = new List<FlyoutItem>();

            try
            {
                flyoutItems.Add(new FlyoutItem
                {
                    Text = _resourceService.GetString("PlaylistBaseViewModel_MenuPlaylists_NewPlaylistText"),
                    Glyph = "\uE710", // Add icon
                    Data = ActionMode.AddNewPlaylist,
                    InsertMode = InsertMode.AddSelected
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during playlist loading
                System.Diagnostics.Debug.WriteLine($"Error loading playlists: {ex.Message}");
            }

            foreach (var playlist in playlists)
            {
                flyoutItems.Add(new FlyoutItem
                {
                    Text = playlist.Name ?? string.Empty,
                    Data = playlist,
                    InsertMode = InsertMode.AddSelected
                });
            }

            return flyoutItems;
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
                Albums = new IncrementalLoadingCollection<DelegateIncrementalSource<CarouselItem>, CarouselItem>(
                    new DelegateIncrementalSource<CarouselItem>(async (pageIndex, pageSize, loadCancellationToken) =>
                    {
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(loadCancellationToken, cancellationToken);
                        var result = await _dataService.GetAlbumSearchResults(queryText, pageIndex + 1, pageSize, linkedCts.Token);

                        HasMoreAlbums = result?.HasNextPage ?? false;

                        if (result?.Items is not { Count: > 0 } items)
                        {
                            return [];
                        }

                        var carouselItems = new List<CarouselItem>(items.Count);
                        for (int i = 0; i < items.Count; i++)
                        {
                            var album = items[i];
                            if (album != null)
                            {
                                carouselItems.Add(new CarouselItem
                                {
                                    Title = album.Title ?? string.Empty,
                                    SubTitle = album.Artist?.Name ?? string.Empty,
                                    ImagePath = _imageService.GetBitmapSource(album.AlbumId, true),
                                    Data = album
                                });
                            }
                        }

                        return carouselItems;
                    }),
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
        
        [RelayCommand]
        private async Task AddToPlaylistAsync(object parameter)
        {
            if (parameter is MenuFlyoutCommandParameter menuCommandParam)
            {

            }
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
        private void PlayAsNext(TrackItem? item)
        {
            if (item != null)
            {
                _ = _mediaManager.InsertTracksToPlayQueueAsync( [item.Id], PlayerMode.Song);
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
    }
}
