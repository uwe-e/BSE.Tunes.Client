using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class NowPlayingPageViewModel : PlayerBaseViewModel
    {
        private string _coverImage;
        private ICommand _closeDialogCommand;
        private DelegateCommand<object> _openFlyoutCommand;
        private readonly IEventAggregator _eventAggregator;
        private readonly IFlyoutNavigationService _flyoutNavigationService;
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly IMediaManager _mediaManager;
        private SubscriptionToken _albumInfoSelectionToken;
        
        public ICommand CloseDialogCommand => _closeDialogCommand ??= new DelegateCommand(async () =>
        {
            await CloseDialog();
        });

        public DelegateCommand<object> OpenFlyoutCommand => _openFlyoutCommand
            ??= new DelegateCommand<object>(async (obj) =>
            {
                await OpenFlyout(obj);
            });

        public string CoverImage
        {
            get => _coverImage;
            set => SetProperty(ref _coverImage, value);
        }

        public NowPlayingPageViewModel(
            INavigationService navigationService,
            IEventAggregator eventAggregator,
            IFlyoutNavigationService flyoutNavigationService,
            IDataService dataService,
            IImageService imageService,
            IMediaManager mediaManager) : base(navigationService, eventAggregator, mediaManager)
        {
            _eventAggregator = eventAggregator;
            _flyoutNavigationService = flyoutNavigationService;
            _dataService = dataService;
            _imageService = imageService;
            _mediaManager = mediaManager;

            _eventAggregator.GetEvent<PlaylistActionContextChanged>().Subscribe(async args =>
            {
                if (args is PlaylistActionContext managePlaylistContext)
                {
                    switch (managePlaylistContext.ActionMode)
                    {
                        case PlaylistActionMode.AddToPlaylist:
                            managePlaylistContext.ActionMode = PlaylistActionMode.None;
                            await AddToPlaylist(managePlaylistContext);
                            break;
                        case PlaylistActionMode.SelectPlaylist:
                            managePlaylistContext.ActionMode = PlaylistActionMode.None;
                            await SelectPlaylist(managePlaylistContext);
                            break;
                        case PlaylistActionMode.CreatePlaylist:
                            managePlaylistContext.ActionMode = PlaylistActionMode.None;
                            await CreateNewPlaylist(managePlaylistContext);
                            break;
                    }
                }
            }, ThreadOption.UIThread);

            _albumInfoSelectionToken = _eventAggregator.GetEvent<AlbumInfoSelectionEvent>().ShowAlbum(async (uniqueTrack) =>
            {
                // because of preventing the multiple execution of the event, we unsubscribe this event
                _eventAggregator.GetEvent<AlbumInfoSelectionEvent>().Unsubscribe(_albumInfoSelectionToken);

                await CloseDialog();
                
                _eventAggregator.GetEvent<AlbumInfoSelectionEvent>().Publish(uniqueTrack);
            });

        }
        
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetValue<Track>("source") is Track currentTrack)
            {
                CurrentTrack = currentTrack;
                CoverImage = _imageService.GetBitmapSource(CurrentTrack.Album.AlbumId);
            }
            //Progress = _mediaManager.pr.Progress;
            //CurrentTrack = _mediaManager.CurrentTrack;
            //CoverImage = _imageService.GetBitmapSource(CurrentTrack.Album.AlbumId);
            PlayerState = _mediaManager.PlayerState;

            base.OnNavigatedTo(parameters);
        }

        protected override void OnTrackChanged(Track track)
        {
            if (track != null)
            {
                CoverImage = _imageService.GetBitmapSource(track.Album.AlbumId);
            }
        }

        private async Task CloseDialog()
        {
            var navigationParams = new NavigationParameters
            {
                { KnownNavigationParameters.UseModalNavigation, true },
                { KnownNavigationParameters.Animated, true }
            };
            await NavigationService.GoBackAsync(navigationParams);
        }

        private async Task OpenFlyout(object obj)
        {
            var source = new PlaylistActionContext
            {
                DisplayAlbumInfo = true,
                Data = obj,
            };

            var navigationParams = new NavigationParameters{
                { "source", source }
            };

            await _flyoutNavigationService.ShowFlyoutAsync(nameof(PlaylistActionToolbarPage), navigationParams);
        }

        private async Task CreateNewPlaylist(PlaylistActionContext managePlaylistContext)
        {
            await CloseDialog();

            var navigationParams = new NavigationParameters
            {
                { "source", managePlaylistContext },
                { KnownNavigationParameters.UseModalNavigation, true }
            };
            await NavigationService.NavigateAsync(nameof(NewPlaylistDialogPage), navigationParams);
        }

        private async Task AddToPlaylist(PlaylistActionContext managePlaylistContext)
        {
            await CloseDialog();

            if (managePlaylistContext.Data is Track track)
            {
                var playlistTo = managePlaylistContext.PlaylistTo;
                if (playlistTo != null && track != null)
                {
                    playlistTo.Entries.Add(new PlaylistEntry
                    {
                        PlaylistId = playlistTo.Id,
                        TrackId = track.Id,
                        Guid = Guid.NewGuid()
                    });

                    await _dataService.AppendToPlaylist(playlistTo);
                    await _imageService.RemoveStitchedBitmaps(playlistTo.Id);

                    managePlaylistContext.ActionMode = PlaylistActionMode.PlaylistUpdated;
                    _eventAggregator.GetEvent<PlaylistActionContextChanged>().Publish(managePlaylistContext);
                }
            }
        }

        private async Task SelectPlaylist(PlaylistActionContext managePlaylistContext)
        {
            await CloseDialog();

            var navigationParams = new NavigationParameters
            {
                { "source", managePlaylistContext },
                { KnownNavigationParameters.UseModalNavigation, true }
            };
            await NavigationService.NavigateAsync(nameof(PlaylistSelectorDialogPage), navigationParams);
        }
    }
}
