
using BSE.Tunes.WinUI.Client.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;


namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class SearchAlbumsPageViewModel : ViewModelBase
    {
        private readonly IResourceService _resourceService;

        private CancellationTokenSource? _searchCancellationTokenSource;

        [ObservableProperty]
        public partial string? TitleText { get; set; }

        public SearchAlbumsPageViewModel(IResourceService resourceService)
        {
            this._resourceService = resourceService;
        }

        public async override void OnNavigatedTo(object parameter)
        {
            if (parameter is string queryText)
            {
                TitleText = string.Format(
                    _resourceService.GetString("SearchAlbumsPage_Title_Text"), queryText);

                _searchCancellationTokenSource?.Cancel();
                _searchCancellationTokenSource = new CancellationTokenSource();

                //_ = LoadSearchResultsAsync(queryText, _searchCancellationTokenSource.Token);

                //PlaylistMenuItems?.Clear();
                //PlaylistMenuItems = await LoadPlaylistMenusAsync();
                //var menuitems = await LoadPlaylistMenusAsync();
                //for (int i = 0; i < menuitems.Count; i++)
                //{
                //    PlaylistMenuItems?.Add(menuitems[i]);
                //}
            }

            base.OnNavigatedTo(parameter);
        }
    }
}
