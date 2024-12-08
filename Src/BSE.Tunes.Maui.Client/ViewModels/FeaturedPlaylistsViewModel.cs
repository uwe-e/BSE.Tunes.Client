using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class FeaturedPlaylistsViewModel : ViewModelBase
    {
        private ObservableCollection<GridPanel>? _items;
        private DelegateCommand<GridPanel> _selectItemCommand;
        private readonly ISettingsService _settingsService;
        private readonly IImageService _imageService;
        private readonly IResourceService _resourceService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDataService _dataService;

        public ObservableCollection<GridPanel> Items => _items ??= [];

        public ICommand SelectItemCommand => _selectItemCommand ??= new DelegateCommand<GridPanel>(SelectItem);

        public FeaturedPlaylistsViewModel(
            INavigationService navigationService,
            ISettingsService settingsService,
            IImageService imageService,
            IResourceService resourceService,
            IEventAggregator eventAggregator,
            IDataService dataService) : base(navigationService)
        {
            _settingsService = settingsService;
            _imageService = imageService;
            _resourceService = resourceService;
            _eventAggregator = eventAggregator;
            _dataService = dataService;

            LoadData();

            _eventAggregator.GetEvent<PlaylistActionContextChanged>().Subscribe(args =>
            {
                if (args is PlaylistActionContext managePlaylistContext)
                {
                    switch (managePlaylistContext.ActionMode)
                    {
                        case PlaylistActionMode.PlaylistUpdated:
                        case PlaylistActionMode.PlaylistDeleted:
                            IsBusy = true;
                            LoadData();
                            break;
                    }
                }
            });
        }

        private async void LoadData()
        {
            var playlists = await _dataService.GetPlaylistsByUserName(_settingsService.User.UserName, 0, 5);
            if (playlists != null)
            {
                foreach (var playlist in playlists)
                {
                    Items.Add(new GridPanel
                    {
                        Id = playlist.Id,
                        Title = playlist.Name,
                        SubTitle = $"{playlist.NumberEntries} {_resourceService.GetString("PlaylistItem_PartNumberOfEntries")}",
                        ImageSource = await _imageService.GetStitchedBitmapSource(playlist.Id),
                        Data = playlist
                    });
                }
            }
            IsBusy = false;
        }
        
        private void SelectItem(GridPanel panel)
        {
            if (panel?.Data is Playlist playlist)
            {
                _eventAggregator.GetEvent<PlaylistSelectedEvent>().Publish(playlist);
            }
        }
    }
}
