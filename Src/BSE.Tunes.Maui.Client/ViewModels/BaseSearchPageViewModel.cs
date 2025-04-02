
using BSE.Tunes.Maui.Client.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class BaseSearchPageViewModel : ViewModelBase
    {
        private int _pageSize;
        private int _pageNumber;
        private bool _hasItems;
        private ICommand _loadMoreItemsCommand;
        private ICommand _selectItemCommand;
        private string _query;
        private ObservableCollection<GridPanel> _items;

        public ICommand LoadMoreItemsCommand => _loadMoreItemsCommand ??= new DelegateCommand(LoadMoreItems);

        public ICommand SelectItemCommand => _selectItemCommand ??= new DelegateCommand<GridPanel>(SelectItem);


        public ObservableCollection<GridPanel> Items => _items ??= [];

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
            set => _hasItems = value;
        }

        protected string Query => _query;

        public BaseSearchPageViewModel(
            INavigationService navigationService
            ) : base(navigationService)
        {
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            var query = parameters.GetValue<string>("query");
            if (!string.IsNullOrEmpty(query))
            {
                if (query.CompareTo(_query) != 0)
                {
                    IsBusy = false;
                    _query = query;
                    _hasItems = true;
                    PageNumber = 0;
                    Items.Clear();
                    LoadMoreItems();
                }
            }
        }

        private void LoadMoreItems()
        {
            _ = LoadMoreItemsAsync();
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
                finally
                {
                    IsBusy = false;
                }
            }
        }

        
    }
}
