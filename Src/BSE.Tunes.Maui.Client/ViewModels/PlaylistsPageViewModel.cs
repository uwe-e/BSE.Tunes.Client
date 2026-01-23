using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class PlaylistsPageViewModel : ViewModelBase, IActiveAware
    {
        private bool _isActive;
        private bool _isActivated;
        private bool _hasItems;
        private ICommand _remainingItemsThresholdReachedCommand;
        private int _pageSize;
        private ObservableCollection<GridPanel> _items;
        private int _pageNumber;
        private ICommand _selectItemCommand;
        private int _totalNumberOfItems;
        private readonly IDataService _dataService;
        private readonly ISettingsService _settingsService;
        private readonly IImageService _imageService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IResourceService _resourceService;

        public ICommand RemainingItemsThresholdReachedCommand => _remainingItemsThresholdReachedCommand ??= new DelegateCommand(async () =>
        {
            if (IsBusy || !HasMoreItems())
            {
                return;
            }

            PageNumber++;
            await FetchAndPopulateAlbumsAsync();
        });

        public ICommand SelectItemCommand => _selectItemCommand ??= new DelegateCommand<GridPanel>(async(panel) => await SelectItemAsync(panel));

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

        public PlaylistsPageViewModel(
            INavigationService navigationService,
            IDataService dataService,
            ISettingsService settingsService,
            IImageService imageService,
            IEventAggregator eventAggregator,
            IResourceService resourceService) : base(navigationService)
        {
            _dataService = dataService;
            _settingsService = settingsService;
            _imageService = imageService;
            _eventAggregator = eventAggregator;
            _resourceService = resourceService;

            PageNumber = 1;
            PageSize = 20;

            _eventAggregator.GetEvent<PlaylistActionContextChanged>().Subscribe(args =>
            {
                if (args is PlaylistActionContext managePlaylistContext)
                {
                    Items.Clear();
                    _isActivated = false;
                    RaiseIsActiveChanged();
                }
            });

            _eventAggregator.GetEvent<CacheChangedEvent>().Subscribe((args) =>
            {
                Items.Clear();
                _isActivated = false;
                RaiseIsActiveChanged();
            });

            _eventAggregator.GetEvent<AlbumInfoSelectionEvent>().ShowAlbum(async (uniqueTrack) =>
            {
                if (PageUtilities.IsCurrentPageTypeOf(typeof(PlaylistsPage), uniqueTrack.UniqueId))
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
                IsBusy = false;
                Items.Clear();
                PageNumber = 1;

                _ = FetchAndPopulateAlbumsAsync();
            }
            IsActiveChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool HasMoreItems()
        {
            return HasItems && (PageNumber * PageSize) < TotalNumberOfItems; ;
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
                PagedResult<Playlist> pagedResult = await _dataService.GetPagedPlaylistsByOwnerAsync(_pageNumber, PageSize);

                if (PageNumber == 1)
                {
                    TotalNumberOfItems = pagedResult.TotalCount;
                    HasItems = TotalNumberOfItems > 0;
                }

                if (pagedResult?.Items != null)
                {
                    foreach (var playlist in pagedResult.Items)
                    {
                        if (playlist != null)
                        {
                            Items.Add(new GridPanel
                            {
                                Title = playlist.Name,
                                SubTitle = $"{playlist.NumberEntries} {_resourceService.GetString("PlaylistItem_PartNumberOfEntries")}",
                                ImageSource = await _imageService.GetStitchedBitmapSourceAsync(playlist.Id, playlist.CoverAlbumIds, asThumbnail: true),
                                Data = playlist
                            });
                        }
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
        
        private async Task SelectItemAsync(GridPanel panel)
        {
            if (panel?.Data is Playlist playlist)
            {
                var navigationParams = new NavigationParameters
                    {
                        { "playlist", playlist }
                    };

                await NavigationService.NavigateAsync($"{nameof(PlaylistDetailPage)}", navigationParams);
            }
        }
    }
}
