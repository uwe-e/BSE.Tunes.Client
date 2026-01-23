using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class FeaturedPlaylistsViewModel : ViewModelBase, IDisposable
    {
        private ObservableCollection<GridPanel> _items;
        private DelegateCommand<GridPanel> _selectItemCommand;
        private readonly IImageService _imageService;
        private readonly IResourceService _resourceService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDataService _dataService;
        private SubscriptionToken _refreshToken;
        private SubscriptionToken _playlistActionToken;
        private CancellationTokenSource _loadCancellation;

        public ObservableCollection<GridPanel> Items => _items ??= [];

        public ICommand SelectItemCommand => _selectItemCommand ??= new DelegateCommand<GridPanel>(SelectItem);

        public FeaturedPlaylistsViewModel(
            INavigationService navigationService,
            IImageService imageService,
            IResourceService resourceService,
            IEventAggregator eventAggregator,
            IDataService dataService) : base(navigationService)
        {
            _imageService = imageService;
            _resourceService = resourceService;
            _eventAggregator = eventAggregator;
            _dataService = dataService;

            _refreshToken = _eventAggregator.GetEvent<HomePageRefreshEvent>()
                .Subscribe(OnRefresh, ThreadOption.UIThread, keepSubscriberReferenceAlive: false);

            _playlistActionToken = _eventAggregator.GetEvent<PlaylistActionContextChanged>()
                .Subscribe(OnPlaylistActionChanged, ThreadOption.UIThread, keepSubscriberReferenceAlive: false);
        }
        public void Dispose()
        {
            _refreshToken?.Dispose();
            _playlistActionToken?.Dispose();
            _loadCancellation?.Cancel();
            _loadCancellation?.Dispose();
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (Items.Count == 0)
            {
                LoadData();
            }
        }

        private void OnRefresh()
        {
            IsBusy = true;
            LoadData();
        }

        private void OnPlaylistActionChanged(PlaylistActionContext args)
        {
            if (args is { ActionMode: PlaylistActionMode.PlaylistUpdated or PlaylistActionMode.PlaylistDeleted })
            {
                IsBusy = true;
                LoadData();
            };
        }

        private void LoadData()
        {
            // Cancel previous load operation
            _loadCancellation?.Cancel();
            _loadCancellation = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    await LoadDataAsync(_loadCancellation.Token);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load playlists: {ex}");
                    IsBusy = false;
                }
            });
        }

        private async Task LoadDataAsync(CancellationToken cancellationToken)
        {
            var pagedResult = await _dataService.GetPagedPlaylistsByOwnerAsync(1, 6);

            // Fix: Use .Any() instead of .Count for IEnumerable<T>
            if (pagedResult.Items != null && pagedResult.Items.Any())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var resourceString = _resourceService.GetString("PlaylistItem_PartNumberOfEntries");

                var tasks = pagedResult.Items.Select(playlist =>
                    CreateGridPanelAsync(playlist, resourceString, cancellationToken));

                var gridPanels = await Task.WhenAll(tasks);

                // Update collection on UI thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _items = new ObservableCollection<GridPanel>(gridPanels);
                    RaisePropertyChanged(nameof(Items));
                    IsBusy = false;
                });
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _items.Clear();
                    IsBusy = false;
                });
            }
        }

        private async Task<GridPanel> CreateGridPanelAsync(Playlist playlist, string resourceString, CancellationToken cancellationToken)
        {
            var imageSource = await _imageService.GetStitchedBitmapSourceAsync(
                playlist.Id,
                playlist.CoverAlbumIds,
                asThumbnail: true);

            cancellationToken.ThrowIfCancellationRequested();

            return new GridPanel
            {
                Id = playlist.Id,
                Title = playlist.Name,
                SubTitle = $"{playlist.NumberEntries} {resourceString}",
                ImageSource = imageSource,
                Data = playlist
            };
        }

        private void SelectItem(GridPanel panel)
        {
            if (panel?.Data is Playlist playlist)
            {
                _eventAggregator.GetEvent<PlaylistSelectedEvent>().Publish(playlist);
            }
        }

        
    }
}
