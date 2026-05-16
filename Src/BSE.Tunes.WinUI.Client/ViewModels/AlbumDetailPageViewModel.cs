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
    public partial class AlbumDetailPageViewModel : PlaylistBaseViewModel<TrackItem>
    {
        [ObservableProperty]
        private Album? _album;

        //[ObservableProperty]
        //private ImageSource? _imageSource;

        //[ObservableProperty]
        //private ObservableCollection<TrackItem> _items = [];

        //[ObservableProperty]
        //private ObservableCollection<TrackItem> _selectedItems = [];

        [ObservableProperty]
        private TrackItem? _selectedTrack;

        //[ObservableProperty]
        //private ListViewSelectionMode _selectionMode = ListViewSelectionMode.Single;

        //[ObservableProperty]
        //private bool _isBusy;

        //[ObservableProperty]
        //private bool _isCommandBarVisible;

        //[ObservableProperty]
        //private bool _isItemClickEnabled = true;

        //[ObservableProperty]
        //private bool _allItemsSelected;

        //[ObservableProperty]
        //private ObservableCollection<BSE.Tunes.WinUI.Client.Models.FlyoutItem> _playlistMenuItems = [];

        //[ObservableProperty]
        //private bool _isAllToPlaylistFlyoutOpen;

        //public bool HasSelectedItems => SelectedItems.Count > 0;

        public AlbumDetailPageViewModel(
            IDataService dataService,
            IImageService imageService,
            IMediaManager mediaManager,
            IDialogService dialogService,
            IResourceService resourceService) : base(dataService, imageService, mediaManager, dialogService, resourceService)
        {
            // Monitor selected items collection
            //SelectedItems.CollectionChanged += OnSelectedItemsChanged;
        }



        public override void OnNavigatedTo(object parameter)
        {
            base.OnNavigatedTo(parameter);

            if (parameter is Album album)
            {
                _ = LoadAlbumAsync(album.Id);
            }

            //_ = LoadPlaylistsAsync();
        }

        //private async Task LoadPlaylistsAsync()
        //{
        //    var playlists = await DataService.GetAllPlaylists();
        //    PlaylistMenuItems.Clear();

        //    // Add "New Playlist" item

        //    try
        //    {
        //        PlaylistMenuItems.Add(new FlyoutItem
        //        {
        //            Text = "New Playlist",
        //            Glyph = "\uE710", // Add icon
        //            Data = ActionMode.AddNewPlaylist,
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle any exceptions that may occur during playlist loading
        //        System.Diagnostics.Debug.WriteLine($"Error loading playlists: {ex.Message}");
        //    }

        //    // Add separator
        //    PlaylistMenuItems.Add(new FlyoutItem
        //    {
        //        IsSeparator = true
        //    });

        //    // Add playlists
        //    foreach (var playlist in playlists)
        //    {
        //        PlaylistMenuItems.Add(new FlyoutItem
        //        {
        //            Text = playlist.Name ?? string.Empty,
        //            Data = playlist
        //        });
        //    }
        //}

        public async Task LoadAlbumAsync(int albumId)
        {
            IsBusy = true;

            try
            {
                Album = await DataService.GetAlbumById(albumId);

                if (Album != null)
                {
                    var imagePath = ImageService.GetBitmapSource(Album.AlbumId, false);
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
            Items.Clear();
            if (Album?.Tracks != null)
            {
                foreach (var track in Album.Tracks)
                {
                    if (track != null)
                    {
                        track.Album = album;
                        Items.Add(TrackItem.FromTrack(track));
                    }
                }
            }
        }

        //protected override void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        //{
        //    base.OnSelectedItemsChanged(sender, e);
        //    //OnPropertyChanged(nameof(HasSelectedItems));
        //    //IsCommandBarVisible = HasSelectedItems;
        //    //SelectionMode = HasSelectedItems ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Extended;
        //    //AllItemsSelected = HasSelectedItems && SelectedItems.Count == Items.Count;
        //    //IsItemClickEnabled = !HasSelectedItems;
        //}

        //[RelayCommand]
        //private void SelectItems(TrackItem? trackItem)
        //{
        //    if (trackItem != null && !SelectedItems.Contains(trackItem))
        //    {
        //        SelectedItems.Add(trackItem);
        //    }
        //}

        //[RelayCommand]
        //private void ClearSelection()
        //{
        //    SelectedItems.Clear();
        //}

        //[RelayCommand]
        //private void SelectAll()
        //{
        //    foreach (var track in Tracks)
        //    {
        //        if (!SelectedItems.Contains(track))
        //        {
        //            SelectedItems.Add(track);
        //        }
        //    }
        //}
        //public override void SelectAll()
        //{
        //    foreach (var item in Items)
        //    {
        //        if (!SelectedItems.Contains(item))
        //        {
        //            SelectedItems.Add(item);
        //        }
        //    }
        //}

        //[RelayCommand]
        //private void UnSelectAll()
        //{
        //    SelectedItems.Clear();
        //}

        public override void PlayTrack(object? listItemData)
        {
            if (listItemData is TrackItem trackItem)
            {
                var trackIds = new ObservableCollection<int> { trackItem.Id };
                _ = MediaManager.PlayTracksAsync(trackIds, PlayerMode.Song);
            }
        }

        public override void PlayAll()
        {
            var trackIds = new ObservableCollection<int>(Items.OfType<TrackItem>().Select(t => t.Id));
            _ = MediaManager.PlayTracksAsync(trackIds, PlayerMode.CD);
        }

        public override void PlayAllShuffle()
        {
            var trackIds = new ObservableCollection<int>(Items.OfType<TrackItem>().Select(t => t.Id));
            _ = MediaManager.PlayTracksAsync(trackIds.ToRandomCollection(), PlayerMode.CD);
        }

        public override void PlaySelected()
        {
            if (SelectedItems != null)
            {
                var trackItems = SelectedItems.OfType<TrackItem>().ToList();
                var trackIds = new ObservableCollection<int>(trackItems.Select(t => t.Id));
                _ = MediaManager.PlayTracksAsync(trackIds, PlayerMode.Song);
                SelectedItems.Clear();
            }
        }

        public override void PlayAsNext()
        {
            if (SelectedItems != null)
            {
                var trackItems = SelectedItems.OfType<TrackItem>().ToList();
                var trackIds = new ObservableCollection<int>(trackItems.Select(t => t.Id));
                _ = MediaManager.InsertTracksToPlayQueueAsync(trackIds, PlayerMode.Song);
                SelectedItems.Clear();
            }
        }

        //[RelayCommand]
        //private async Task MenuItemClicked(object? parameter)
        //{
        //    if (parameter is FlyoutItem flyoutItem)
        //    {
        //        if(flyoutItem.Data is ActionMode actionMode)
        //        {
        //            switch (actionMode)
        //            {
        //                case ActionMode.AddNewPlaylist:
                            
        //                    var (result, dialog) = await DialogService.ShowDialogAsync<CreatePlaylistDialog>();
        //                    if (result == ContentDialogResult.Primary)
        //                    {
        //                        var createdPlaylist = dialog.ViewModel.CreatedPlaylist;
        //                        if (createdPlaylist != null)
        //                        {
        //                            await AppendSelectedTracksToPlaylistAsync(createdPlaylist.Id);
        //                        }
        //                    }
        //                    break;
        //            }
        //        }
        //        else if (flyoutItem.Data is PlaylistSummary playlist)
        //        {
        //            await AppendSelectedTracksToPlaylistAsync(playlist.Id);
                    
        //        }
        //    }
        //}

        public override async Task AppendSelectedTracksToPlaylistAsync(int playlistId)
        {
            var trackItems = SelectedItems.OfType<TrackItem>().ToList();
            var trackIds = new ObservableCollection<int>(trackItems.Select(t => t.Id));

            await Task.WhenAll(
                        DataService.AppendToPlaylist(playlistId, trackIds),
                        ImageService.RemoveComposedBitmaps(playlistId));

            SelectedItems.Clear();

            Messenger.Send(new PlaylistChangedMessage(playlistId));
        }
    }
}
