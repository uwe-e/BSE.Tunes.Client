﻿using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class TracklistBaseViewModel : ViewModelBase
    {
        private ObservableCollection<GridPanel>? _items;
        private string _imageSource;
        private ICommand _openFlyoutCommand;
        private ICommand _playCommand;
        private DelegateCommand _playAllCommand;
        private DelegateCommand _playAllRandomizedCommand;
        private IFlyoutNavigationService flyoutNavigationService;
        private IMediaManager mediaManager;
        private readonly IFlyoutNavigationService _flyoutNavigationService;
        private readonly IDataService _dataService;
        private readonly IMediaManager _mediaManager;
        private readonly IImageService _imageService;
        private readonly IEventAggregator _eventAggregator;

        public ICommand OpenFlyoutCommand => _openFlyoutCommand ??= new DelegateCommand<object>(async(obj) => await OpenFlyoutAsync(obj));

        public ICommand PlayCommand => _playCommand ??= new DelegateCommand<GridPanel>(PlayTrack);

        public DelegateCommand PlayAllCommand => _playAllCommand ??= new DelegateCommand(PlayAll, CanPlayAll);

        public DelegateCommand PlayAllRandomizedCommand => _playAllRandomizedCommand ??= new DelegateCommand(PlayAllRandomized, CanPlayAllRandomized);

        public ObservableCollection<GridPanel> Items => _items ??= [];

        public string ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                SetProperty<string>(ref _imageSource, value);
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
                        case PlaylistActionMode.RemoveFromPlaylist:
                            managePlaylistContext.ActionMode = PlaylistActionMode.None;
                            await RemoveFromPlaylistAsync(managePlaylistContext);
                            break;
                    }
                }
            }, ThreadOption.UIThread);
        }

        protected virtual async Task RemoveFromPlaylistAsync(PlaylistActionContext managePlaylistContext)
        {
            await _flyoutNavigationService.CloseFlyoutAsync();
        }

        private async Task OpenFlyoutAsync(object obj)
        {
            var source = new PlaylistActionContext
            {
                Data = obj,
            };

            if (obj is GridPanel item)
            {
                source.Data = item.Data;
            }

            var navigationParams = new NavigationParameters();
            navigationParams.Add("source", source);
            navigationParams.Add(KnownNavigationParameters.UseModalNavigation, true);
            navigationParams.Add(KnownNavigationParameters.Animated, false);

            await _flyoutNavigationService.ShowFlyoutAsync(nameof(PlaylistActionToolbarPage), navigationParams);
        }

        protected async Task SelectPlaylist(PlaylistActionContext managePlaylistContext)
        {
            await _flyoutNavigationService.CloseFlyoutAsync();

            var navigationParams = new NavigationParameters
            {
                { "source", managePlaylistContext },
                { KnownNavigationParameters.UseModalNavigation, true}
            };
            await NavigationService.NavigateAsync(nameof(PlaylistSelectorDialogPage), navigationParams);
        }

        protected virtual void PlayTrack(GridPanel panel)
        {
        }

        protected virtual bool CanPlayAll()
        {
            return Items.Count > 0;
        }

        protected virtual void PlayAll()
        {
        }

        protected virtual bool CanPlayAllRandomized()
        {
            return CanPlayAll();
        }

        protected virtual void PlayAllRandomized()
        {
        }

        protected virtual void PlayTracks(IEnumerable<int> trackIds, PlayerMode playerMode)
        {
            _mediaManager.PlayTracks(
                new ObservableCollection<int>(trackIds), playerMode);
        }

        protected virtual ObservableCollection<int> GetTrackIds()
        {
            return new ObservableCollection<int>();
        }

        protected virtual async Task AddToPlaylist(PlaylistActionContext managePlaylistContext)
        {
            var navigationParams = new NavigationParameters
            {
                { KnownNavigationParameters.UseModalNavigation, true}
            };
            await NavigationService.GoBackAsync(navigationParams);

            IEnumerable<Track> tracks = default;

            if (managePlaylistContext.Data is Track track)
            {
                tracks = Enumerable.Repeat(track, 1);
            }
            if (managePlaylistContext.Data is Album album)
            {
                tracks = album.Tracks;
            }
            if (managePlaylistContext.Data is PlaylistEntry playlistEntry)
            {
                tracks = Enumerable.Repeat(playlistEntry.Track, 1);
            }
            if (managePlaylistContext.Data is Playlist playlist)
            {
                tracks = playlist.Entries?.Select(t => t.Track);
            }

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
                foreach (var track in tracks)
                {
                    if (track != null)
                    {
                        playlistTo.Entries.Add(new PlaylistEntry
                        {
                            PlaylistId = playlistTo.Id,
                            TrackId = track.Id,
                            Guid = Guid.NewGuid()
                        });
                    }
                }
                await _dataService.AppendToPlaylist(playlistTo);
                await _imageService.RemoveStitchedBitmaps(playlistTo.Id);

                managePlaylistContext.ActionMode = PlaylistActionMode.PlaylistUpdated;
                _eventAggregator.GetEvent<PlaylistActionContextChanged>().Publish(managePlaylistContext);
            }
        }
    }
}
