using Prism;
using Prism.Commands;
using Prism.Navigation;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class BaseSettingsPageViewModel : ViewModelBase, IActiveAware
    {
        private bool _isActive;
        private bool _isActivated;
        private ICommand? _deleteCommand;

        public ICommand DeleteCommand => _deleteCommand ??= new DelegateCommand(Delete);

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value, RaiseIsActiveChanged); }
        }

        public event EventHandler IsActiveChanged;

        public BaseSettingsPageViewModel(INavigationService navigationService) : base(navigationService)
        {
        }

        public virtual void DeleteSettings()
        {
        }

        public virtual void LoadSettings()
        {
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
