using CommunityToolkit.Mvvm.ComponentModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class SearchPageViewModel : ViewModelBase
    {
        private readonly Contracts.Services.IResourceService _resourceService;
        [ObservableProperty]
        private string? titleText;

        public SearchPageViewModel(Contracts.Services.IResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        public override void OnNavigatedTo(object parameter)
        {
            if (parameter is string queryText)
            {

                TitleText = string.Format(
                    _resourceService.GetString("SearchPage_Title_Text"), queryText);
            }
            
            base.OnNavigatedTo(parameter);
        }
    }
}
