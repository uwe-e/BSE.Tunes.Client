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
    public partial class PlaylistDetailPageViewModel : ViewModelBase, IRecipient<PlaylistChangedMessage>
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly IMediaManager _mediaManager;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private Playlist? _playlist;
        
        [ObservableProperty]
        private ObservableCollection<PlaylistEntryItem> _items = [];
        
        [ObservableProperty]
        private ObservableCollection<FlyoutItem> _playlistMenuItems = [];

        [ObservableProperty]
        private ObservableCollection<PlaylistEntryItem> _selectedItems = [];

        [ObservableProperty]
        private PlaylistEntryItem? _selectedItem;

        [ObservableProperty]
        private ListViewSelectionMode _selectionMode = ListViewSelectionMode.Single;

        [ObservableProperty]
        private ImageSource? _coverImageSource;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isCommandBarVisible;

        [ObservableProperty]
        private bool _isItemClickEnabled = true;

        public bool HasSelectedItems => SelectedItems.Count > 0;

        public PlaylistDetailPageViewModel(
            IDataService dataService,
            IImageService imageService,
            IMediaManager mediaManager,
            IDialogService dialogService,
            INavigationService navigationService,
            IMessenger messenger)
        {
            _dataService = dataService;
            _imageService = imageService;
            _mediaManager = mediaManager;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _messenger = messenger;

            SelectedItems.CollectionChanged += OnSelectedItemsChanged;

            Messenger.Register<PlaylistChangedMessage>(this);

            Messenger.Register<PlaylistDeletedMessage>(this, (r, m) =>
            {
                if (Playlist != null && m.PlaylistId == Playlist.Id)
                {
                    _navigationService.GoBack();
                }
            });
        }

        

        public override void OnNavigatedTo(object parameter)
        {
            base.OnNavigatedTo(parameter);

            if (parameter is Playlist playlist)
            {
                _ = LoadPlaylistAsync(playlist.Id);
            }

            _ = LoadPlaylistsAsync();

        }

        public override void OnNavigatedFrom()
        {
            base.OnNavigatedFrom();

            Messenger.Unregister<PlaylistChangedMessage>(this);
        }

        private async Task LoadPlaylistsAsync()
        {
            var playlists = await _dataService.GetAllPlaylists();
            PlaylistMenuItems.Clear();

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

        private async Task LoadPlaylistAsync(int playlistId)
        {
            IsBusy = true;

            try
            {
                Playlist = await LoadPlaylistByIdAsync(playlistId);
                if (Playlist != null)
                {
                    await LoadEntriesAsync(Playlist);
                }
                //Playlist = await _dataService.GetPlaylistById(playlistId);
                //if (Playlist != null)
                //{
                //    var imagePath = await _imageService.GetComposedBitmapSourceAsync(playlistId, Playlist.CoverAlbumIds);
                //    if (!string.IsNullOrEmpty(imagePath))
                //    {
                //        var bitmapImage = new BitmapImage
                //        {
                //            CreateOptions = BitmapCreateOptions.IgnoreImageCache
                //        };
                //        bitmapImage.UriSource = new Uri(imagePath);
                //        CoverImageSource = bitmapImage;
                //    }
                //    await LoadEntriesAsync(Playlist);
                //}
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<Playlist?> LoadPlaylistByIdAsync(int playlistId)
        {
            var playlist = await _dataService.GetPlaylistById(playlistId);
            if (playlist != null)
            {
                var imagePath = await _imageService.GetComposedBitmapSourceAsync(playlistId, playlist.CoverAlbumIds);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    CoverImageSource = new BitmapImage
                    {
                        CreateOptions = BitmapCreateOptions.IgnoreImageCache,
                        UriSource = new Uri(imagePath)
                    };
                }
            }
            return playlist;
        }
        private async Task LoadEntriesAsync(Playlist playlist)
        {
            Items.Clear();

            var pagedEntries = await _dataService.GetPagedPlaylistEntriesByIdAsync(
                                        playlist.Id,
                                        1,
                                        1000);

            if (pagedEntries != null)
            {
                foreach (var entry in pagedEntries.Items)
                {
                    Items.Add(PlaylistEntryItem.FromPlaylistEntry(entry));
                }
            }
        }
        private void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasSelectedItems));
            IsCommandBarVisible = HasSelectedItems;
            SelectionMode = HasSelectedItems ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Extended;
            IsItemClickEnabled = !HasSelectedItems;
        }

        [RelayCommand]
        private async Task RemoveSelectionAsync()
        {
            IsBusy = true;

            try
            {
                var count = SelectedItems.Count;
                if (count > 0)
                {
                    var entryIds = new List<int>(count);
                    for (int i = 0; i < count; i++)
                    {
                        entryIds.Add(SelectedItems[i].Id);
                    }

                    await _dataService.DeletePlaylistEntriesAsync(Playlist.Id, entryIds);

                    // Remove items in a single batch operation
                    var itemsToRemove = SelectedItems.ToArray();
                    foreach (var item in itemsToRemove)
                    {
                        Items.Remove(item);
                    }

                    await _imageService.RemoveComposedBitmaps(Playlist.Id);

                    SelectedItems.Clear();

                    _messenger.Send(new PlaylistChangedMessage(Playlist.Id));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void SelectItems(PlaylistEntryItem? item)
        {
            if (item != null && !SelectedItems.Contains(item))
            {
                SelectedItems.Add(item);
            }
        }

        [RelayCommand]
        private async Task RemovePlaylistAsync()
        {
            var(result, dialog) = await _dialogService.ShowDialogAsync<DeletePlaylistDialog>();
            if (result == ContentDialogResult.Primary)
            {
                await _dataService.DeletePlaylist(Playlist!.Id);

                _messenger.Send(new PlaylistDeletedMessage(Playlist.Id));
            }

        }

        [RelayCommand]
        private void ClearSelection()
        {
            SelectedItems.Clear();
        }

        [RelayCommand]
        private void UnSelectAll()
        {
            SelectedItems.Clear();
        }

        [RelayCommand]
        private async Task PlayAllAsync()
        {
            //if (Album?.Tracks != null && Album.Tracks.Length > 0)
            //{
            //    var trackIds = new ObservableCollection<int>(Album.Tracks.Select(t => t.Id));
            //    await _mediaManager.PlayTracksAsync(trackIds, PlayerMode.CD);
            //}
        }

        [RelayCommand]
        private async Task PlayAllShuffleAsync()
        {
            //if (Album?.Tracks != null && Album.Tracks.Length > 0)
            //{
            //    var trackIds = new ObservableCollection<int>(Album.Tracks.Select(t => t.Id));
            //    await _mediaManager.PlayTracksAsync(trackIds.ToRandomCollection(), PlayerMode.CD);
            //}
        }

        [RelayCommand]
        private async Task PlaySelectedAsync()
        {
            //if (SelectedItems != null)
            //{
            //    var trackIds = new ObservableCollection<int>(SelectedItems.Select(t => t.Id));
            //    await _mediaManager.PlayTracksAsync(trackIds, PlayerMode.Song);
            //    SelectedItems.Clear();
            //}
        }

        [RelayCommand]
        private async Task PlayAsNext()
        {
            //if (SelectedItems != null)
            //{
            //    var trackIds = new ObservableCollection<int>(SelectedItems.Select(t => t.Id));
            //    await _mediaManager.InsertTracksToPlayQueueAsync(trackIds, PlayerMode.Song);
            //    SelectedItems.Clear();
            //}
        }

        [RelayCommand]
        private async Task MenuItemClicked(object? parameter)
        {
            //if (parameter is FlyoutItem flyoutItem)
            //{
            //    if (flyoutItem.Data is ActionMode actionMode)
            //    {
            //        switch (actionMode)
            //        {
            //            case ActionMode.AddNewPlaylist:
            //                IsAllToPlaylistFlyoutOpen = false;

            //                var (result, dialog) = await _dialogService.ShowDialogAsync<CreatePlaylistDialog>();
            //                if (result == ContentDialogResult.Primary)
            //                {
            //                    var createdPlaylist = dialog.ViewModel.CreatedPlaylist;
            //                    if (createdPlaylist != null)
            //                    {
            //                        await AppendSelectedTracksToPlaylistAsync(createdPlaylist.Id);
            //                    }
            //                }
            //                break;
            //        }
            //    }
            //    else if (flyoutItem.Data is PlaylistSummary playlist)
            //    {
            //        await AppendSelectedTracksToPlaylistAsync(playlist.Id);

            //    }
            //}
        }

        private async Task AppendSelectedTracksToPlaylistAsync(int playlistId)
        {
            //var trackIds = new ObservableCollection<int>(SelectedItems.Select(t => t.Id));
            //await _dataService.AppendToPlaylist(playlistId, trackIds);
            //SelectedItems.Clear();
        }

        public async Task Receive(int playlistId)
        {
            if (Playlist != null && Playlist.Id == playlistId)
            {
                await LoadPlaylistAsync(playlistId);
                //CoverImageSource = null;
                //var playlist = await _dataService.GetPlaylistById(playlistId);
                //if (playlist != null)
                //{
                //    var imagePath = await _imageService.GetComposedBitmapSourceAsync(playlistId, playlist.CoverAlbumIds);
                //    if (!string.IsNullOrEmpty(imagePath))
                //    {
                //        var bitmapImage = new BitmapImage
                //        {
                //            CreateOptions = BitmapCreateOptions.IgnoreImageCache
                //        };
                //        bitmapImage.UriSource = new Uri(imagePath);
                //        CoverImageSource = bitmapImage;
                //    }
                //    //await LoadEntriesAsync(Playlist);
                //}
            }
        }

        void IRecipient<PlaylistChangedMessage>.Receive(PlaylistChangedMessage message)
        {
            _ = Receive(message.PlaylistId);
        }
    }
}
