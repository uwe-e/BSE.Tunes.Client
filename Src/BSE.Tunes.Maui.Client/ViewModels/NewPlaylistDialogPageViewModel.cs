using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class NewPlaylistDialogPageViewModel(
        INavigationService navigationService,
        IDataService dataService,
        ISettingsService settingsService,
        IEventAggregator eventAggregator) : ViewModelBase(navigationService)
    {
        private ICommand _cancelCommand;
        private DelegateCommand _saveCommand;
        private string _playlistName;
        private PlaylistActionContext _playlistActionContext;
        private readonly IDataService _dataService = dataService;
        private readonly ISettingsService _settingsService = settingsService;
        private readonly IEventAggregator _eventAggregator = eventAggregator;

        public ICommand CancelCommand => _cancelCommand ??= new DelegateCommand(async() => await CloseDialogAsync());

        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(async() => await SavePlaylistAsync(),
            CanSavePlaylist);

        public string PlaylistName
        {
            get => _playlistName;
            set
            {
                SetProperty<string>(ref _playlistName, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            _playlistActionContext = parameters.GetValue<PlaylistActionContext>("source");

            IsBusy = false;
            
            base.OnNavigatedTo(parameters);
        }

        private async Task CloseDialogAsync()
        {
            await NavigationService.GoBackAsync(new NavigationParameters
            {
                { KnownNavigationParameters.UseModalNavigation, true }
            });
        }
        
        private bool CanSavePlaylist()
        {
            return !String.IsNullOrEmpty(PlaylistName);
        }

        private async Task SavePlaylistAsync()
        {
            try
            {
                var playlist = await _dataService.InsertPlaylist(new Playlist
                {
                    Name = PlaylistName,
                    UserName = _settingsService.User.UserName,
                    Guid = Guid.NewGuid()
                });

                await CloseDialogAsync();
                
                _playlistActionContext.ActionMode = PlaylistActionMode.AddToPlaylist;
                _playlistActionContext.PlaylistTo = playlist as Playlist;

                _eventAggregator.GetEvent<PlaylistActionContextChanged>().Publish(_playlistActionContext);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
