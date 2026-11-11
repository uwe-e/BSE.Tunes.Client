using BSE.Tunes.WinUI.Client.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class SearchTracksPageViewModel : ViewModelBase
    {
        private readonly IResourceService _resourceService;
        private readonly INavigationService _navigationService;
        private CancellationTokenSource? _searchCancellationTokenSource;

        [ObservableProperty]
        public partial string? TitleText { get; set; }

        public SearchTracksPageViewModel(
            IResourceService resourceService,
            INavigationService navigationService)
        {
            _resourceService = resourceService;
            _navigationService = navigationService;
        }

        public override void OnNavigatedFrom()
        {
            base.OnNavigatedFrom();
        }

        public override void OnNavigatedTo(object parameter)
        {
            if (parameter is string queryText)
            {
                TitleText = string.Format(
                    _resourceService.GetString("SearchTracksPage_Title_Text"), queryText);

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
