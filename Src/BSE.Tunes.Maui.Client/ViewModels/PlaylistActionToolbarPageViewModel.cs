using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class PlaylistActionToolbarPageViewModel : ViewModelBase
    {
        private ICommand? _closeFlyoutCommand;
        private ICommand? _addToPlaylistCommand;
        private ICommand _removeFromPlaylistCommand;
        private ICommand _removePlaylistCommand;
        private ICommand _displayAlbumInfoCommand;
        private bool _canRemovePlaylist;
        private bool _canRemoveFromPlaylist;
        private bool _canDisplayAlbumInfo;
        private PlaylistActionContext _playlistActionContext;
        private string _imageSource;
        private string _subTitle;
        private string _title;
        private readonly IImageService _imageService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IFlyoutNavigationService _flyoutNavigationService;

        public ICommand CloseFlyoutCommand => _closeFlyoutCommand
            ??= new DelegateCommand(async () =>
            {
                await CloseFlyoutAsync();
            });

        public ICommand AddToPlaylistCommand => _addToPlaylistCommand
           ??= new DelegateCommand(AddToPlaylist);

        public ICommand RemoveFromPlaylistCommand => _removeFromPlaylistCommand
           ??= new DelegateCommand(RemoveFromPlaylist);

        public ICommand RemovePlaylistCommand => _removePlaylistCommand
             ??= new DelegateCommand(RemovePlaylist);

        public ICommand DisplayAlbumInfoCommand => _displayAlbumInfoCommand
            ??= new DelegateCommand(async() => ShowAlbumAsync());

        public string ImageSource
        {
            get => _imageSource;
            set => SetProperty<string>(ref _imageSource, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string SubTitle
        {
            get => _subTitle;
            set => SetProperty<string>(ref _subTitle, value);
        }

        public bool CanRemovePlaylist
        {
            get => _canRemovePlaylist;
            set => SetProperty<bool>(ref _canRemovePlaylist, value);
        }
        public bool CanRemoveFromPlaylist
        {
            get => _canRemoveFromPlaylist;
            set => SetProperty<bool>(ref _canRemoveFromPlaylist, value);
        }

        public bool CanDisplayAlbumInfo
        {
            get => _canDisplayAlbumInfo;
            set => SetProperty<bool>(ref _canDisplayAlbumInfo, value);
        }

        public PlaylistActionToolbarPageViewModel(
            INavigationService navigationService,
            IImageService imageService,
            IEventAggregator eventAggregator,
            IFlyoutNavigationService flyoutNavigationService) : base(navigationService)
        {
            _imageService = imageService;
            _eventAggregator = eventAggregator;
            _flyoutNavigationService = flyoutNavigationService;
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            _playlistActionContext = parameters.GetValue<PlaylistActionContext>("source");
            if (_playlistActionContext?.Data is Track track)
            {
                //comes from the nowplaying page and is used only there
                CanDisplayAlbumInfo = (bool)_playlistActionContext?.DisplayAlbumInfo;
                Title = track.Name;
                SubTitle = track.Album.Artist.Name;
                ImageSource = _imageService.GetBitmapSource(track.Album.AlbumId, true);
            }
            if (_playlistActionContext?.Data is Album album)
            {
                Title = album.Title;
                SubTitle = album.Artist?.Name;
                ImageSource = _imageService.GetBitmapSource(album.AlbumId, true);
            }
            if (_playlistActionContext?.Data is Playlist playlist)
            {
                CanRemovePlaylist = true;
                Title = playlist.Name;
                ImageSource = await _imageService.GetStitchedBitmapSourceAsync(playlist.Id, 50, true);
            }
            if (_playlistActionContext?.Data is PlaylistEntry playlistEntry)
            {
                CanRemoveFromPlaylist = true;
                CanDisplayAlbumInfo = true;
                Title = playlistEntry.Track?.Name;
                SubTitle = playlistEntry.Artist;
                ImageSource = _imageService.GetBitmapSource(playlistEntry.AlbumId, true);
            }
            base.OnNavigatedTo(parameters);
        }

        private async Task CloseFlyoutAsync()
        {
            await _flyoutNavigationService.CloseFlyoutAsync();
        }

        private void AddToPlaylist()
        {
            if (_playlistActionContext != null)
            {
                _playlistActionContext.ActionMode = PlaylistActionMode.SelectPlaylist;
                _eventAggregator.GetEvent<PlaylistActionContextChanged>().Publish(_playlistActionContext);
            }
        }

        private void RemoveFromPlaylist()
        {
            if (_playlistActionContext != null)
            {
                _playlistActionContext.ActionMode = PlaylistActionMode.RemoveFromPlaylist;
                _eventAggregator.GetEvent<PlaylistActionContextChanged>().Publish(_playlistActionContext);
            }
        }

        private async Task ShowAlbumAsync()
        {
            await CloseFlyoutAsync();

            if (_playlistActionContext != null)
            {
                /*
                 * This event has a unique identifier that can be used to prevent multiple execution.
                 */
                var uniqueTrack = new UniqueAlbum
                {
                    UniqueId = Guid.NewGuid()
                };

                if (_playlistActionContext.Data is Track track)
                {
                    uniqueTrack.Album = track.Album;
                }
                if (_playlistActionContext.Data is PlaylistEntry playlistEntry)
                {
                    uniqueTrack.Album = playlistEntry.Track.Album;
                }

                _eventAggregator.GetEvent<AlbumInfoSelectionEvent>().Publish(uniqueTrack);
            }
        }

        private void RemovePlaylist()
        {
            if (_playlistActionContext != null)
            {
                _playlistActionContext.ActionMode = PlaylistActionMode.RemovePlaylist;
                _eventAggregator.GetEvent<PlaylistActionContextChanged>().Publish(_playlistActionContext);
            }
        }
    }
}
