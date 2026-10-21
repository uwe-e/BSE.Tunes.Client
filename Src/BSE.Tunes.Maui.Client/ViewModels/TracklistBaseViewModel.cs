using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using BSE.Tunes.Shared.Services.Media;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public abstract class TracklistBaseViewModel : ViewModelBase, IAlbumInfoSelectionHandler
    {
        private ObservableCollection<GridPanel> _items;
        private string _imageSource;
        private ICommand _openFlyoutCommand;
        private ICommand _playCommand;
        private DelegateCommand _playAllCommand;
        private DelegateCommand _playAllRandomizedCommand;
        private readonly IFlyoutNavigationService _flyoutNavigationService;
        private readonly IDataService _dataService;
        private readonly IMediaManager _mediaManager;
        private readonly IImageService _imageService;
        private readonly IEventAggregator _eventAggregator;
        private readonly SubscriptionToken _basePlaylistActionToken;

        public ICommand OpenFlyoutCommand => _openFlyoutCommand
            ??= new DelegateCommand<object>(async(obj) => await OpenFlyoutAsync(obj));

        public ICommand PlayCommand => _playCommand
            ??= new DelegateCommand<GridPanel>(async(gridPanel) => await PlayTrackAsync(gridPanel), CanExecutePlayTrack);

        public DelegateCommand PlayAllCommand => _playAllCommand
            ??= new DelegateCommand(async() => await PlayAllAsync(), CanPlayAll);

        public DelegateCommand PlayAllRandomizedCommand => _playAllRandomizedCommand
            ??= new DelegateCommand(async() => await PlayAllRandomizedAsync(), CanPlayAllRandomized);

        public ObservableCollection<GridPanel> Items => _items ??= [];

        public string ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                SetProperty(ref _imageSource, value);
            }
        }

        public TracklistBaseViewModel(
            INavigationService navigationService,
            IFlyoutNavigationService flyoutNavigationService,
            IDataService dataService,
            IMediaManager mediaManager,
            IImageService imageService,
            IEventAggregator eventAggregator) : base(navigationService)
        {
            _flyoutNavigationService = flyoutNavigationService;
            _dataService = dataService;
            _mediaManager = mediaManager;
            _imageService = imageService;
            _eventAggregator = eventAggregator;

            _basePlaylistActionToken = _eventAggregator
                .GetEvent<PlaylistActionContextChanged>()
                .Subscribe(
                    OnPlaylistActionChanged,
                    ThreadOption.UIThread);
        }

        /// <summary>
        /// Handles the selection of an album, which can be triggered from various contexts
        /// such as album lists or search results. The method receives an AlbumSelectionContext that provides details
        /// about the selected album and the context of the selection. Implementing this method allows derived 
        /// view models to respond appropriately to album selections, such as navigating to an album detail page
        /// or updating the UI with album information.
        /// </summary>
        /// <param name="context"></param>
        public abstract void HandleShowAlbum(AlbumSelectionContext context);

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            // Only subscribe to album selection events if this page is not being navigated to modally,
            // to avoid handling events in the background when a dialog is open
            this.SubscribeToAlbumSelection(_eventAggregator);
            base.OnNavigatedTo(parameters);
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            // Only unsubscribe from album selection events if this page is not being navigated from modally,
            if (!parameters.IsModalNavigation())
            {
                this.UnsubscribeFromAlbumSelection();
            }
            base.OnNavigatedFrom(parameters);
        }

        private async void OnPlaylistActionChanged(PlaylistActionContext context)
        {
            switch (context.ActionMode)
            {
                case PlaylistActionMode.AddToPlaylist:
                    context.ActionMode = PlaylistActionMode.None;
                    await AddToPlaylist(context);
                    break;
                case PlaylistActionMode.SelectPlaylist:
                    context.ActionMode = PlaylistActionMode.None;
                    await SelectPlaylist(context);
                    break;
                case PlaylistActionMode.CreatePlaylist:
                    context.ActionMode = PlaylistActionMode.None;
                    await CreateNewPlaylist(context);
                    break;
                case PlaylistActionMode.RemoveFromPlaylist:
                    context.ActionMode = PlaylistActionMode.None;
                    await RemoveFromPlaylistAsync(context);
                    break;
                case PlaylistActionMode.RemovePlaylist:
                    context.ActionMode = PlaylistActionMode.None;
                    await RemovePlaylistAsync(context);
                    break;
                case PlaylistActionMode.PlaylistDeleted:
                    //managePlaylistContext.ActionMode = PlaylistActionMode.None;
                    // closes the open PlaylistDetailPage
                    await NavigationService.GoBackAsync();
                    break;
                case PlaylistActionMode.PlaylistUpdated:
                    await OnPlaylistUpdatedAsync(context);
                    break;
            }
        }

        protected virtual Task OnPlaylistUpdatedAsync(PlaylistActionContext context)
        {
            return Task.CompletedTask;
        }

        private async Task CreateNewPlaylist(PlaylistActionContext managePlaylistContext)
        {
            var navigationParams = new NavigationParameters
            {
                { KnownNavigationParameters.UseModalNavigation, true },
                { "source", managePlaylistContext }
            };
            await NavigationService.NavigateAsync(nameof(NewPlaylistDialogPage), navigationParams);
        }

        protected virtual Task RemoveFromPlaylistAsync(PlaylistActionContext managePlaylistContext)
        {
            return Task.CompletedTask;
        }

        protected virtual async Task OpenFlyoutAsync(object obj, PlaylistActionContext playlistActionContext)
        {
            var source = playlistActionContext;
            source ??= new PlaylistActionContext
                {
                    Data = obj,
                };

            if (obj is GridPanel item)
            {
                source.Data = item.Data;
            }

            var navigationParams = new NavigationParameters
            {
                { "source", source },
                { KnownNavigationParameters.UseModalNavigation, true},
                { KnownNavigationParameters.Animated, false}
            };

            await _flyoutNavigationService.ShowFlyoutAsync(nameof(PlaylistActionToolbarPage), navigationParams);
        }
        
        protected virtual async Task OpenFlyoutAsync(object obj)
        {
            await OpenFlyoutAsync(obj, null);
        }

        protected async Task SelectPlaylist(PlaylistActionContext managePlaylistContext)
        {
            var navigationParams = new NavigationParameters
            {
                { "source", managePlaylistContext },
                { KnownNavigationParameters.UseModalNavigation, true}
            };
            await NavigationService.NavigateAsync(nameof(PlaylistSelectorDialogPage), navigationParams);
        }
        
        protected virtual bool CanExecutePlayTrack(GridPanel panel)
        {
            throw new NotImplementedException();
        }

        protected virtual Task PlayTrackAsync(GridPanel panel)
        {
            return Task.CompletedTask;
        }

        protected virtual bool CanPlayAll()
        {
            return Items.Count > 0;
        }

        protected virtual Task PlayAllAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual bool CanPlayAllRandomized()
        {
            return CanPlayAll();
        }

        protected virtual Task PlayAllRandomizedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual async Task PlayTracksAsync(IEnumerable<int> trackIds, PlayerMode playerMode)
        {
            await _mediaManager.PlayTracksAsync(new ObservableCollection<int>(trackIds), playerMode);
        }

        protected virtual ObservableCollection<int> GetTrackIds()
        {
            return [];
        }

        protected virtual async Task AddToPlaylist(PlaylistActionContext managePlaylistContext)
        {
            IEnumerable<Track> tracks = managePlaylistContext.Data switch
            {
                Track track => [track],
                Album album => album.Tracks,
                PlaylistEntry playlistEntry => [playlistEntry.Track],
                Playlist playlist => playlist.Entries?.Select(t => t.Track),
                _ => null
            };

            if (tracks != null)
            {
                await AddTracksToPlaylist(managePlaylistContext, tracks);
            }

        }

        private async Task AddTracksToPlaylist(PlaylistActionContext managePlaylistContext, IEnumerable<Track> tracks)
        {
            var playlistTo = managePlaylistContext.PlaylistTo;
            if (playlistTo != null && tracks != null)
            {
                var trackIds = tracks
                    .Where(track => track != null)
                    .Select(track => track.Id)
                    .ToList();

                if (trackIds.Count > 0)
                {
                    await Task.WhenAll(
                        _dataService.AppendToPlaylist(playlistTo.Id, trackIds),
                        _imageService.RemoveComposedBitmaps(playlistTo.Id));

                    managePlaylistContext.ActionMode = PlaylistActionMode.PlaylistUpdated;
                    _eventAggregator.GetEvent<PlaylistActionContextChanged>().Publish(managePlaylistContext);
                }
            }
        }
        
        private async Task RemovePlaylistAsync(PlaylistActionContext managePlaylistContext)
        {
            if (managePlaylistContext.Data is Playlist playlist)
            {
                await _dataService.DeletePlaylist(playlist.Id);

                managePlaylistContext.ActionMode = PlaylistActionMode.PlaylistDeleted;

                _eventAggregator.GetEvent<PlaylistActionContextChanged>().Publish(managePlaylistContext);
            }
        }
        
    }
}
