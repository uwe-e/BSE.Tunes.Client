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
        private ICommand _loadMoreItemsCommand;
        private int _pageSize;
        private ObservableCollection<GridPanel> _items;
        private int _pageNumber;
        private ICommand _selectItemCommand;
        private readonly IDataService _dataService;
        private readonly ISettingsService _settingsService;
        private readonly IImageService _imageService;
        private readonly IResourceService _resourceService;

        public ICommand LoadMoreItemsCommand => _loadMoreItemsCommand ??= new DelegateCommand(async () =>
        {
            await LoadMoreItemsAsync();
        }, HasMoreItems);

        public ICommand SelectItemCommand => _selectItemCommand ??= new DelegateCommand<GridPanel>(async(panel) => await SelectItemAsync(panel));

        public ObservableCollection<GridPanel> Items => _items ??= new ObservableCollection<GridPanel>();

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value, RaiseIsActiveChanged);
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

        public event EventHandler IsActiveChanged;

        public PlaylistsPageViewModel(
            INavigationService navigationService,
            IDataService dataService,
            ISettingsService settingsService,
            IImageService imageService,
            IResourceService resourceService) : base(navigationService)
        {
            _dataService = dataService;
            _settingsService = settingsService;
            _imageService = imageService;
            _resourceService = resourceService;
            
            PageSize = 20;
        }

        private void RaiseIsActiveChanged()
        {
            if (IsActive && !_isActivated)
            {
                IsBusy = false;
                _isActivated = true;
                _hasItems = true;
                
                _ = LoadMoreItemsAsync();
            }
            IsActiveChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool HasMoreItems()
        {
            return _hasItems;
        }

        private async Task LoadMoreItemsAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (_hasItems) {
                IsBusy = true;

                try
                {
                    var playlists = await _dataService.GetPlaylistsByUserName(_settingsService.User.UserName, _pageNumber, PageSize);
                    if (playlists == null || playlists.Count == 0)
                    {
                        _hasItems = false;
                        return;
                    }

                    foreach (var playlst in playlists)
                    {
                        if (playlst != null)
                        {
                            var playlist = await _dataService.GetPlaylistByIdWithNumberOfEntries(playlst.Id, _settingsService.User.UserName);
                            if (playlist != null)
                            {
                                Items.Add(new GridPanel
                                {
                                    Title = playlist.Name,
                                    SubTitle = $"{playlist.NumberEntries} {_resourceService.GetString("PlaylistItem_PartNumberOfEntries")}",
                                    ImageSource = await _imageService.GetStitchedBitmapSource(playlist.Id),
                                    Data = playlist
                                });
                            }
                        }
                    }
                    _pageNumber = Items.Count;
                }
                finally
                {
                    IsBusy = false;
                }

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
