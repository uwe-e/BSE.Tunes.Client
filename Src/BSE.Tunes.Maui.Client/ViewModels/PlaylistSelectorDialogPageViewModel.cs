

using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class PlaylistSelectorDialogPageViewModel : ViewModelBase
    {
        private ObservableCollection<FlyoutItemViewModel> _playlistFlyoutItems;
        private PlaylistActionContext _playlistActionContext;
        private ICommand _cancelCommand;
        private readonly IDataService _dataService;
        private readonly ISettingsService _settingsService;
        private readonly IImageService _imageService;
        private readonly IEventAggregator _eventAggregator;

        public virtual ObservableCollection<FlyoutItemViewModel> PlaylistFlyoutItems =>
            _playlistFlyoutItems ??= new ObservableCollection<FlyoutItemViewModel>();

        public ICommand CancelCommand =>
           _cancelCommand ??= new DelegateCommand(CloseDialog);

        

        public PlaylistSelectorDialogPageViewModel(
            INavigationService navigationService,
            IDataService dataService,
            ISettingsService settingsService,
            IImageService imageService,
            IEventAggregator eventAggregator) : base(navigationService)
        {
            _dataService = dataService;
            _settingsService = settingsService;
            _imageService = imageService;
            _eventAggregator = eventAggregator;
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            _playlistActionContext = parameters.GetValue<PlaylistActionContext>("source");
            
            Task.Run(async () =>
            {
                await CreatePlaylistFlyoutItems();
                IsBusy = false;
            });

            base.OnNavigatedTo(parameters);
        }

        private async Task CreatePlaylistFlyoutItems()
        {
            var playlists = await _dataService.GetPlaylistsByUserName(_settingsService.User.UserName, 0, 50);
            if (playlists != null)
            {
                foreach (var playlist in playlists)
                {
                    if (playlist != null)
                    {
                        var flyoutItem = new FlyoutItemViewModel
                        {
                            Text = playlist.Name,
                            ImageSource = await _imageService.GetStitchedBitmapSource(playlist.Id, 50, true),
                            Data = playlist
                        };
                        flyoutItem.ItemClicked += OnFlyoutItemClicked;
                        PlaylistFlyoutItems.Add(flyoutItem);
                    }
                }
            }
        }

        private void OnFlyoutItemClicked(object sender, EventArgs e)
        {
            if (sender is FlyoutItemViewModel flyoutItem)
            {
                _playlistActionContext.PlaylistTo = flyoutItem.Data as Playlist;
                _playlistActionContext.ActionMode = PlaylistActionMode.AddToPlaylist;
                _eventAggregator.GetEvent<PlaylistActionContextChanged>().Publish(_playlistActionContext);
            }
        }
        
        private async void CloseDialog()
        {
            var navigationParams = new NavigationParameters
            {
                { KnownNavigationParameters.UseModalNavigation, true}
            };
            await NavigationService.GoBackAsync(navigationParams);
        }
    }
}
