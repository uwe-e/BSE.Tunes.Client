

using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
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
        private ICommand _loadMoreItemsCommand;
        private ICommand _selectItemCommand;
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;

        public ICommand LoadMoreItemsCommand => _loadMoreItemsCommand ??= new DelegateCommand(async () =>
           {
               PageSize = 40;
               await LoadMoreItemsAsync();
           }, HasMoreItems);

        public ICommand SelectItemCommand => _selectItemCommand ??= new DelegateCommand<GridPanel>(SelectItem);


        private bool HasMoreItems()
        {
            return HasItems && TotalNumberOfItems > PageNumber;
        }

        public ObservableCollection<GridPanel> Items => _items ??= new ObservableCollection<GridPanel>();

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
            IImageService imageService) : base(navigationService)
        {
            _dataService = dataService;
            _imageService = imageService;

            PageSize = 40;
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
            TotalNumberOfItems = await _dataService.GetNumberOfAlbumsByGenre(genreId);
            HasItems = TotalNumberOfItems > 0;
            if (HasItems)
            {
                IsBusy = false;
                await LoadMoreItemsAsync();
            }
        }

        private async Task LoadMoreItemsAsync()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            try
            {
                var albums = await _dataService.GetAlbumsByGenre(null, PageNumber, PageSize);
                if (albums != null)
                {
                    foreach (var album in albums)
                    {
                        if (album != null)
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
                    PageNumber = Items.Count;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
