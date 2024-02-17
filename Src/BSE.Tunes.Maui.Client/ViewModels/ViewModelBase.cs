using Prism.AppModel;
using Prism.Navigation;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public abstract class ViewModelBase : Prism.Mvvm.BindableBase, INavigationAware, IPageLifecycleAware
    {
        private readonly INavigationService _navigationService;
        private bool _isBusy;

        public INavigationService NavigationService => _navigationService;

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                SetProperty<bool>(ref _isBusy, value);
            }
        }

        protected ViewModelBase(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public virtual void OnAppearing()
        {
        }

        public virtual void OnDisappearing()
        {
        }

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {
        }
    }
}
