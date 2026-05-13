using BSE.Tunes.Shared.Services.Extensions;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Messages;
using BSE.Tunes.WinUI.Client.Models;
using BSE.Tunes.WinUI.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class AlbumDetailPageViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly IMediaManager _mediaManager;
        private readonly IDialogService _dialogService;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private Album? _album;

        [ObservableProperty]
        private ImageSource? _imageSource;

        [ObservableProperty]
        private ObservableCollection<TrackItem> _tracks = [];

        [ObservableProperty]
        private ObservableCollection<TrackItem> _selectedItems = [];

        [ObservableProperty]
        private TrackItem? _selectedTrack;

        [ObservableProperty]
        private ListViewSelectionMode _selectionMode = ListViewSelectionMode.Single;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isCommandBarVisible;

        [ObservableProperty]
        private bool _isItemClickEnabled = true;

        [ObservableProperty]
        private bool _allItemsSelected;

        [ObservableProperty]
        private ObservableCollection<BSE.Tunes.WinUI.Client.Models.FlyoutItem> _playlistMenuItems = [];

        [ObservableProperty]
        private bool _isAllToPlaylistFlyoutOpen;

        public bool HasSelectedItems => SelectedItems.Count > 0;

        public AlbumDetailPageViewModel(
            IDataService dataService,
            IImageService imageService,
            IMediaManager mediaManager,
            IDialogService dialogService,
            IMessenger messenger)
        {
            _dataService = dataService;
            _imageService = imageService;
            _mediaManager = mediaManager;
            _dialogService = dialogService;
            _messenger = messenger;

            // Monitor selected items collection
            SelectedItems.CollectionChanged += OnSelectedItemsChanged;
        }



        public override void OnNavigatedTo(object parameter)
        {
            base.OnNavigatedTo(parameter);

            if (parameter is Album album)
            {
                _ = LoadAlbumAsync(album.Id);
            }

            _ = LoadPlaylistsAsync();
        }

        private async Task LoadPlaylistsAsync()
        {
            var playlists = await _dataService.GetAllPlaylists();
            PlaylistMenuItems.Clear();

            // Add "New Playlist" item

            try
            {
                PlaylistMenuItems.Add(new FlyoutItem
                {
                    Text = "New Playlist",
                    Glyph = "\uE710", // Add icon
                    Data = ActionMode.AddNewPlaylist,
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during playlist loading
                System.Diagnostics.Debug.WriteLine($"Error loading playlists: {ex.Message}");
            }

            // Add separator
            PlaylistMenuItems.Add(new FlyoutItem
            {
                IsSeparator = true
            });

            // Add playlists
            foreach (var playlist in playlists)
            {
                PlaylistMenuItems.Add(new FlyoutItem
                {
                    Text = playlist.Name ?? string.Empty,
                    Data = playlist
                });
            }
        }

        public async Task LoadAlbumAsync(int albumId)
        {
            IsBusy = true;

            try
            {
                Album = await _dataService.GetAlbumById(albumId);

                if (Album != null)
                {
                    var imagePath = _imageService.GetBitmapSource(Album.AlbumId, false);
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        ImageSource = new BitmapImage(new Uri(imagePath));
                    }
                    LoadTracks(Album);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LoadTracks(Album album)
        {
            Tracks.Clear();
            if (Album?.Tracks != null)
            {
                foreach (var track in Album.Tracks)
                {
                    if (track != null)
                    {
                        track.Album = album;
                        Tracks.Add(TrackItem.FromTrack(track));
                    }
                }
            }
        }

        private void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasSelectedItems));
            IsCommandBarVisible = HasSelectedItems;
            SelectionMode = HasSelectedItems ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Extended;
            AllItemsSelected = HasSelectedItems && SelectedItems.Count == Tracks.Count;
            IsItemClickEnabled = !HasSelectedItems;
        }

        [RelayCommand]
        private void SelectItems(TrackItem? trackItem)
        {
            if (trackItem != null && !SelectedItems.Contains(trackItem))
            {
                SelectedItems.Add(trackItem);
            }
        }

        [RelayCommand]
        private void ClearSelection()
        {
            SelectedItems.Clear();
        }

        [RelayCommand]
        private void SelectAll()
        {
            foreach (var track in Tracks)
            {
                if (!SelectedItems.Contains(track))
                {
                    SelectedItems.Add(track);
                }
            }
        }

        [RelayCommand]
        private void UnSelectAll()
        {
            SelectedItems.Clear();
        }

        [RelayCommand]
        private async Task PlayAllAsync()
        {
            if (Album?.Tracks != null && Album.Tracks.Length > 0)
            {
                var trackIds = new ObservableCollection<int>(Album.Tracks.Select(t => t.Id));
                await _mediaManager.PlayTracksAsync(trackIds, PlayerMode.CD);
            }
        }

        [RelayCommand]
        private async Task PlayAllShuffleAsync()
        {
            if (Album?.Tracks != null && Album.Tracks.Length > 0)
            {
                var trackIds = new ObservableCollection<int>(Album.Tracks.Select(t => t.Id));
                await _mediaManager.PlayTracksAsync(trackIds.ToRandomCollection(), PlayerMode.CD);
            }
        }

        [RelayCommand]
        private async Task PlaySelectedAsync()
        {
            if (SelectedItems != null)
            {
                var trackIds = new ObservableCollection<int>(SelectedItems.Select(t => t.Id));
                await _mediaManager.PlayTracksAsync(trackIds, PlayerMode.Song);
                SelectedItems.Clear();
            }
        }

        [RelayCommand]
        private async Task PlayAsNext()
        {
            if (SelectedItems != null)
            {
                var trackIds = new ObservableCollection<int>(SelectedItems.Select(t => t.Id));
                await _mediaManager.InsertTracksToPlayQueueAsync(trackIds, PlayerMode.Song);
                SelectedItems.Clear();
            }
        }

        [RelayCommand]
        private async Task MenuItemClicked(object? parameter)
        {
            if (parameter is FlyoutItem flyoutItem)
            {
                if(flyoutItem.Data is ActionMode actionMode)
                {
                    switch (actionMode)
                    {
                        case ActionMode.AddNewPlaylist:
                            IsAllToPlaylistFlyoutOpen = false;
                            
                            var (result, dialog) = await _dialogService.ShowDialogAsync<CreatePlaylistDialog>();
                            if (result == ContentDialogResult.Primary)
                            {
                                var createdPlaylist = dialog.ViewModel.CreatedPlaylist;
                                if (createdPlaylist != null)
                                {
                                    await AppendSelectedTracksToPlaylistAsync(createdPlaylist.Id);
                                }
                            }
                            break;
                    }
                }
                else if (flyoutItem.Data is PlaylistSummary playlist)
                {
                    await AppendSelectedTracksToPlaylistAsync(playlist.Id);
                    
                }
            }
        }

        private async Task AppendSelectedTracksToPlaylistAsync(int playlistId)
        {
            var trackIds = new ObservableCollection<int>(SelectedItems.Select(t => t.Id));

            await Task.WhenAll(
                        _dataService.AppendToPlaylist(playlistId, trackIds),
                        _imageService.RemoveComposedBitmaps(playlistId));

            SelectedItems.Clear();

            _messenger.Send(new PlaylistChangedMessage(playlistId));
        }
    }
}
