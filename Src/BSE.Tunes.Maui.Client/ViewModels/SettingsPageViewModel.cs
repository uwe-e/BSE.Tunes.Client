using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Navigation;
using System.Windows.Input;
using IResourceService = BSE.Tunes.Maui.Client.Services.IResourceService;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase, IActiveAware
    {
        private readonly ISettingsService _settingsService;
        private readonly IResourceService _resourceService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IStorageService _storageService;
        private readonly IAppInfoService _appInfoService;
        private string _serviceEndPoint;
        private string _userName;
        private string _usedDiskSpace;
        private string _versionString;
        private bool _isActive;
        private bool _isActivated;
        private bool _isCacheChanged;
        private ICommand _toServiceEndpointDetailCommand;
        private ICommand _toAccountDetailCommand;
        private ICommand _toCacheSettingsDetailCommand;

        public ICommand ToServiceEndpointDetailCommand
            => _toServiceEndpointDetailCommand ??= new DelegateCommand(async() => await NavigateToServiceEndpointDetailAsync());

        public ICommand ToAccountDetailCommand
           => _toAccountDetailCommand ??= new DelegateCommand(async() => await NavigateToAccountDetailAsync());

        public ICommand ToCacheSettingsDetailCommand
            => _toCacheSettingsDetailCommand ??= new DelegateCommand(async() => await NavigateToCacheSettingsDetailAsync());

        public string ServiceEndPoint
        {
            get
            {
                return _serviceEndPoint;
            }
            set
            {
                SetProperty(ref _serviceEndPoint, value);
            }
        }

        public string UsedDiskSpace
        {
            get
            {
                return _usedDiskSpace;
            }
            set
            {
                SetProperty(ref _usedDiskSpace, value);
            }
        }

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                SetProperty(ref _userName, value);
            }
        }

        public string VersionString
        {
            get
            {
                return _versionString;
            }
            set
            {
                SetProperty(ref _versionString, value);
            }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value, RaiseIsActiveChanged); }
        }

        public event EventHandler IsActiveChanged;

        public SettingsPageViewModel(
            INavigationService navigationService,
            ISettingsService settingsService,
            IResourceService resourceService,
            IEventAggregator eventAggregator,
            IStorageService storageService,
            IAppInfoService appInfoService) : base(navigationService)
        {
            _settingsService = settingsService;
            _resourceService = resourceService;
            _eventAggregator = eventAggregator;
            _storageService = storageService;
            _appInfoService = appInfoService;

            _versionString = $"{_resourceService.GetString("SettingsPage_SectionInformation_VersionString")} {_appInfoService.VersionString}";

            _eventAggregator.GetEvent<CacheChangedEvent>().Subscribe((args) =>
            {
                switch (args)
                {
                    case CacheChangeMode.Added:
                    case CacheChangeMode.Removed:

                        LoadCacheSettings();
                        break;
                }
            });
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

        private void LoadSettings()
        {
            ServiceEndPoint = _settingsService.ServiceEndPoint;
            UserName = _settingsService.User?.UserName;

            LoadCacheSettings();
        }

        private async void LoadCacheSettings()
        {
            if (!_isCacheChanged)
            {
                _isCacheChanged = true;

                var usedSpace = await _storageService.GetUsedDiskSpaceAsync();
                UsedDiskSpace = $"{Math.Round(Convert.ToDecimal(usedSpace / 1024f / 1024f), 2)} MB";

                _isCacheChanged = false;
            }
        }
        
        private async Task NavigateToServiceEndpointDetailAsync()
        {
            await NavigationService.NavigateAsync(nameof(ServiceEndpointSettingsPage));
        }
        
        private async Task NavigateToAccountDetailAsync()
        {
            await NavigationService.NavigateAsync(nameof(LoginSettingsPage));
        }
        
        private async Task NavigateToCacheSettingsDetailAsync()
        {
            await NavigationService.NavigateAsync(nameof(CacheSettingsPage));
        }
    }
}
