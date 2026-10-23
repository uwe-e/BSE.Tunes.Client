using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public abstract class BaseSettingsPageViewModel(
        INavigationService navigationService,
        IEventAggregator eventAggregator)
        : ViewModelBase(navigationService), IActiveAware, IAlbumInfoSelectionHandler
    {
        private bool _isActive;
        private bool _isActivated;
        private ICommand _deleteCommand;
        private readonly IEventAggregator _eventAggregator = eventAggregator;

        public ICommand DeleteCommand => _deleteCommand ??= new DelegateCommand(Delete);

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value, RaiseIsActiveChanged); }
        }

        public event EventHandler IsActiveChanged;

        public abstract void HandleShowAlbum(AlbumSelectionContext context);

        public virtual void DeleteSettings()
        {
        }

        public virtual void LoadSettings()
        {
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            this.SubscribeToAlbumSelection(_eventAggregator);
            base.OnNavigatedTo(parameters);
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            if (parameters.IsModalNavigation())
            {
                this.UnsubscribeFromAlbumSelection();
            }
            base.OnNavigatedFrom(parameters);
        }
        private void RaiseIsActiveChanged()
        {
            if (IsActive && !_isActivated)
            {
                _isActivated = true;

                LoadSettings();
            }
            IsActiveChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Delete()
        {
            DeleteSettings();
        }

        
    }
}
