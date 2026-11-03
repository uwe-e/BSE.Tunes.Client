using BSE.Tunes.WinUI.Client.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BSE.Tunes.WinUI.Client.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        private const double AlbumTileWidth = 150;
        private const double AlbumTileRightMargin = 15;

        private bool _isLoadingVisibleAlbums;

        public SearchPageViewModel ViewModel { get; }
        
        public SearchPage()
        {
            ViewModel = App.GetService<SearchPageViewModel>();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            Unloaded += SearchPage_Unloaded;

            InitializeComponent();
        }

        private async void AlbumsListView_Loaded(object sender, RoutedEventArgs e)
        {
            await EnsureVisibleAlbumsLoadedAsync();
        }

        private async void AlbumsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            await EnsureVisibleAlbumsLoadedAsync();
        }

        private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchPageViewModel.Albums)
                || e.PropertyName == nameof(SearchPageViewModel.HasAlbums))
            {
                await EnsureVisibleAlbumsLoadedAsync();
            }
        }

        private void SearchPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            Unloaded -= SearchPage_Unloaded;
        }

        private async Task EnsureVisibleAlbumsLoadedAsync()
        {
            if (_isLoadingVisibleAlbums
                || !ViewModel.HasAlbums
                || AlbumsListView.ActualWidth <= 0
                || ViewModel.Albums is not ISupportIncrementalLoading incrementalAlbums)
            {
                return;
            }

            _isLoadingVisibleAlbums = true;

            try
            {
                int targetVisibleCount = CalculateVisibleAlbumCount(AlbumsListView.ActualWidth);

                while (ViewModel.Albums.Count < targetVisibleCount && incrementalAlbums.HasMoreItems)
                {
                    uint requestCount = (uint)(targetVisibleCount - ViewModel.Albums.Count);
                    var loadResult = await incrementalAlbums.LoadMoreItemsAsync(requestCount);
                    if (loadResult.Count == 0)
                    {
                        break;
                    }
                }
            }
            finally
            {
                _isLoadingVisibleAlbums = false;
            }
        }

        private static int CalculateVisibleAlbumCount(double availableWidth)
        {
            double tileWidthWithSpacing = AlbumTileWidth + AlbumTileRightMargin;
            int count = (int)Math.Floor((availableWidth + AlbumTileRightMargin) / tileWidthWithSpacing);
            return Math.Max(1, count);
        }
    }
}
