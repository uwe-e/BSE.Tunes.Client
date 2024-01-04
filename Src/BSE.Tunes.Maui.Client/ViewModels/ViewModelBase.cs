namespace BSE.Tunes.Maui.Client.ViewModels
{
    public abstract class ViewModelBase : BindableBase, INavigationAware, IPageLifecycleAware
    {
        public INavigationService NavigationService => _navigationService;

        private readonly INavigationService _navigationService;

        protected ViewModelBase(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void OnAppearing()
        {
        }

        public void OnDisappearing()
        {
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }
    }
}
