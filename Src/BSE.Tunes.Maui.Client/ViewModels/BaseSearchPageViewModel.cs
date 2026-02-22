using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class BaseSearchPageViewModel: TracklistBaseViewModel
    {
        private int _pageSize;
        private int _pageNumber;
        private bool _hasItems;
        private ICommand _loadMoreItemsCommand;
        private ICommand _selectItemCommand;
        private string _query;

        public ICommand LoadMoreItemsCommand => _loadMoreItemsCommand ??= new DelegateCommand(LoadMoreItems);

        public ICommand SelectItemCommand => _selectItemCommand ??= new DelegateCommand<GridPanel>(SelectItem);

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

        protected bool HasItems
        {
            get => _hasItems;
            set => SetProperty(ref _hasItems, value);
        }

        protected string Query => _query;

        public BaseSearchPageViewModel(
            INavigationService navigationService,
            IFlyoutNavigationService flyoutNavigationService,
            IDataService dataService,
            IMediaManager mediaManager,
            IImageService imageService,
            IEventAggregator eventAggregator)
            : base(navigationService, flyoutNavigationService, dataService, mediaManager, imageService, eventAggregator)
        {
            PageSize = 20;
        }

        public override void HandleShowAlbum(AlbumSelectionContext context)
        {
            throw new NotImplementedException();
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            
            var query = parameters.GetValue<string>("query");
            if (!string.IsNullOrEmpty(query) && !string.Equals(query, _query, StringComparison.Ordinal))
            {
                IsBusy = false;
                _query = query;
                _hasItems = true;
                PageNumber = 1;
                Items.Clear();
                LoadMoreItems();
            }
        }

        private void LoadMoreItems()
        {
            Task.Run(LoadMoreItemsAsync);
        }

        protected virtual void SelectItem(GridPanel obj)
        {
        }

        protected virtual Task GetSearchResults()
        {
            throw new NotImplementedException();
        }
        
        private async Task LoadMoreItemsAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (_hasItems == true)
            {

                IsBusy = true;

                try
                {
                    await GetSearchResults();
                }
                catch (Exception ex)
                {
                    // Log error or show user feedback
                    HasItems = false;
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        protected virtual void UpdatePaginationState(bool hasItems, bool hasNextPage)
        {
            if (!hasItems)
            {
                HasItems = false;
                return;
            }

            if (hasNextPage)
            {
                PageNumber++;
                HasItems = true;
            }
            else
            {
                HasItems = false; // No more pages to load
            }
        }
    }
}
