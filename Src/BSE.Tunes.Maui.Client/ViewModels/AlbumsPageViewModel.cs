using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using BSEtunes.Contracts.Enums;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class AlbumsPageViewModel : ViewModelBase, IActiveAware
    {
        private bool _isActive;
        private bool _isActivated;
        private bool _hasItems;
        private int _pageSize;
        private int _pageNumber;
        private int _totalNumberOfItems;
        private ObservableCollection<GridPanel> _items;
        private ICommand _remainingItemsThresholdReachedCommand;
        private ICommand _selectItemCommand;
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly IEventAggregator _eventAggregator;

        public ICommand RemainingItemsThresholdReachedCommand => _remainingItemsThresholdReachedCommand ??= new DelegateCommand(async () =>
        {
            if (IsBusy || !HasMoreItems())
            {
                return;
            }

            PageNumber++;
            await FetchAndPopulateAlbumsAsync();
        });

        public ICommand SelectItemCommand => _selectItemCommand ??= new DelegateCommand<GridPanel>(SelectItem);


        private bool HasMoreItems()
        {
            return HasItems && (PageNumber * PageSize) < TotalNumberOfItems;
        }

        public ObservableCollection<GridPanel> Items => _items ??= [];

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value, RaiseIsActiveChanged);
        }

        public bool HasItems
        {
            get => _hasItems;
            set => SetProperty(ref _hasItems, value);
        }

        public int PageSize
        {
            get => _pageSize;
            set => SetProperty(ref _pageSize, value);
        }

        public int PageNumber
        {
            get => _pageNumber;
            set => SetProperty(ref _pageNumber, value);
        }

        public int TotalNumberOfItems
        {
            get => _totalNumberOfItems;
            set => SetProperty(ref _totalNumberOfItems, value);
        }

        public event EventHandler IsActiveChanged;

        public AlbumsPageViewModel(
            INavigationService navigationService,
            IDataService dataService,
            IImageService imageService,
            IEventAggregator eventAggregator) : base(navigationService)
        {
            _dataService = dataService;
            _imageService = imageService;
            _eventAggregator = eventAggregator;

            PageNumber = 1;
            PageSize = 30;

            _eventAggregator.GetEvent<AlbumInfoSelectionEvent>().ShowAlbum(async (uniqueTrack) =>
            {
                if (PageUtilities.IsCurrentPageTypeOf(typeof(AlbumsPage), uniqueTrack.UniqueId))
                {
                    var navigationParams = new NavigationParameters
                    {
                        { "album", uniqueTrack.Album }
                    };

                    await NavigationService.NavigateAsync(nameof(AlbumDetailPage), navigationParams);
                }
            });
        }

        private void RaiseIsActiveChanged()
        {
            if (IsActive && !_isActivated)
            {
                _isActivated = true;

                _ = LoadAlbumsAsync(null);
            }
            IsActiveChanged?.Invoke(this, EventArgs.Empty);
        }

        private async Task LoadAlbumsAsync(int? genreId)
        {
            IsBusy = false;
            Items.Clear();
            PageNumber = 1;

            await FetchAndPopulateAlbumsAsync();
        }

        private async Task FetchAndPopulateAlbumsAsync()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            try
            {
                PagedResult<Album> pagedAlbums = await _dataService.GetPagedAlbums(
                    null,
                    null,
                    null,
                    null,
                    null,
                    PageNumber,
                    PageSize,
                    AlbumSortOption.Artist);

                if (PageNumber == 1)
                {
                    TotalNumberOfItems = pagedAlbums.TotalCount;
                    HasItems = TotalNumberOfItems > 0;
                }

                if (pagedAlbums?.Items != null)
                {
                    foreach (var album in pagedAlbums.Items)
                    {
                        Items.Add(new GridPanel
                        {
                            Title = album.Title,
                            SubTitle = album.Artist.Name,
                            ImageSource = _imageService.GetBitmapSource(album.AlbumId, true),
                            Data = album
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (PageNumber > 1)
                {
                    PageNumber--; // Rollback on failure during lazy loading
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private void SelectItem(GridPanel panel)
        {
            if (panel?.Data is Album album)
            {
                _ = SelectAlbumAsync(album);
            }
        }

        private async Task SelectAlbumAsync(Album album)
        {
            var navigationParams = new NavigationParameters
            {
                { "album", album }
            };
            await NavigationService.NavigateAsync($"{nameof(AlbumDetailPage)}", navigationParams);
        }
    }
}
