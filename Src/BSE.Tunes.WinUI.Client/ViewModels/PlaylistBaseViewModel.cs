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
    public abstract partial class PlaylistBaseViewModel<TItem> : ViewModelBase
    {
        public IDataService DataService { get; }
        public IImageService ImageService { get; }
        public IMediaManager MediaManager { get; }
        public IDialogService DialogService { get; }
        public IResourceService ResourceService { get; }

        [ObservableProperty]
        private ImageSource? _imageSource;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private ListViewSelectionMode _selectionMode = ListViewSelectionMode.Extended;

        [ObservableProperty]
        private bool _isCommandBarVisible;

        [ObservableProperty]
        private bool _isItemClickEnabled = true;

        [ObservableProperty]
        private ObservableCollection<FlyoutItem> _playlistMenuItemsForAll = [];

        [ObservableProperty]
        private ObservableCollection<FlyoutItem> _playlistMenuItemsForSelected = [];

        [ObservableProperty]
        private ObservableCollection<TItem> _items = [];

        [ObservableProperty]
        private ObservableCollection<object> _selectedItems = [];

        [ObservableProperty]
        private bool _allItemsSelected;

        public bool HasSelectedItems => SelectedItems.Count > 0;

        public PlaylistBaseViewModel(
            IDataService dataService,
            IImageService imageService,
            IMediaManager mediaManager,
            IDialogService dialogService,
            IResourceService resourceService)
        {
            DataService = dataService;
            ImageService = imageService;
            MediaManager = mediaManager;
            DialogService = dialogService;
            ResourceService = resourceService;
        }

        public override void OnNavigatedTo(object parameter)
        {
            base.OnNavigatedTo(parameter);
            SelectedItems.CollectionChanged += OnSelectedItemsChanged;

            _ = InitializePlaylistMenusAsync();
        }

        public override void OnNavigatedFrom()
        {
            base.OnNavigatedFrom();
            SelectedItems.CollectionChanged -= OnSelectedItemsChanged;
        }
        
        protected virtual void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasSelectedItems));
            IsCommandBarVisible = HasSelectedItems;
            SelectionMode = HasSelectedItems ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Extended;
            IsItemClickEnabled = !HasSelectedItems;
            AllItemsSelected = HasSelectedItems && SelectedItems.Count == Items.Count;
        }

        protected virtual async Task InitializePlaylistMenusAsync()
        {
            var playlists = await DataService.GetAllPlaylists();
            var menuItemsForAll = await LoadPlaylistMenuAsync(playlists, InsertMode.AddAll);
            PlaylistMenuItemsForAll.Clear();
            foreach (var item in menuItemsForAll)
            {
                PlaylistMenuItemsForAll.Add(item);
            }

            var menuItemsForSelected = await LoadPlaylistMenuAsync(playlists, InsertMode.AddSelected);
            PlaylistMenuItemsForSelected.Clear();
            foreach (var item in menuItemsForSelected)
            {
                PlaylistMenuItemsForSelected.Add(item);
            }
        }

        protected virtual Task<IList<FlyoutItem>> LoadPlaylistMenuAsync(IEnumerable<PlaylistSummary> playlists, InsertMode insertMode)
        {
            var flyoutItems = new List<FlyoutItem>();

            try
            {
                flyoutItems.Add(new FlyoutItem
                {
                    Text = ResourceService.GetString("PlaylistBaseViewModel_MenuPlaylists_NewPlaylistText"),
                    Glyph = "\uE710", // Add icon
                    Data = ActionMode.AddNewPlaylist,
                    InsertMode = insertMode
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during playlist loading
                System.Diagnostics.Debug.WriteLine($"Error loading playlists: {ex.Message}");
            }

            // Add separator
            flyoutItems.Add(new FlyoutItem
            {
                IsSeparator = true
            });

            // Add playlists
            foreach (var playlist in playlists)
            {
                flyoutItems.Add(new FlyoutItem
                {
                    Text = playlist.Name ?? string.Empty,
                    Data = playlist,
                    InsertMode = insertMode
                });
            }

            return Task.FromResult<IList<FlyoutItem>>(flyoutItems);
        }

        protected abstract int GetTrackId(TItem item);

        private ObservableCollection<int> GetTrackIdsFromSelectedItems()
        {
            var trackIds = new List<int>();
            foreach (var selectedItem in SelectedItems)
            {
                if (selectedItem is TItem item)
                {
                    trackIds.Add(GetTrackId(item));
                }
            }
            return new ObservableCollection<int>(trackIds);
        }

        private ObservableCollection<int> GetAllTrackIds()
        {
            var trackIds = new List<int>();
            foreach (var item in Items)
            {
                if (item != null)
                {
                    trackIds.Add(GetTrackId(item));
                }
            }
            return new ObservableCollection<int>(trackIds);
        }

        protected void LoadImageSource(string imagePath, BitmapCreateOptions options = BitmapCreateOptions.None)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                ImageSource = new BitmapImage
                {
                    CreateOptions = options,
                    UriSource = new Uri(imagePath)
                };
            }
        }

        protected void LoadItemsIntoCollection(IEnumerable<TItem> items)
        {
            Items.Clear();
            foreach (var item in items)
            {
                if (item != null)
                {
                    Items.Add(item);
                }
            }
        }

        [RelayCommand]
        public virtual void PlayTrack(object? listItemData) { }

        [RelayCommand]
        public virtual void PlayAll() { }

        [RelayCommand]
        public virtual void PlayAllShuffle() { }

        [RelayCommand]
        public virtual void PlaySelected() 
        {
            if (SelectedItems != null && SelectedItems.Count > 0)
            {
                var trackIds = GetTrackIdsFromSelectedItems();
                _ = MediaManager.PlayTracksAsync(trackIds, PlayerMode.Song);
                SelectedItems.Clear();
            }
        }

        [RelayCommand]
        public virtual void PlayAsNext()
        {
            if (SelectedItems != null && SelectedItems.Count > 0)
            {
                var trackIds = GetTrackIdsFromSelectedItems();
                _ = MediaManager.InsertTracksToPlayQueueAsync(trackIds, PlayerMode.Song);
                SelectedItems.Clear();
            }
        }

        [RelayCommand]
        public virtual void SelectAll()
        {
            foreach (var item in Items)
            {
                if (item != null && !SelectedItems.Contains(item))
                {
                    SelectedItems.Add(item);
                }
            }
        }

        [RelayCommand]
        public virtual void UnSelectAll()
        {
            SelectedItems.Clear();
        }

        [RelayCommand]
        public virtual void ClearSelection()
        {
            SelectedItems.Clear();
        }

        [RelayCommand]
        public virtual void SelectItems(object? item)
        {
            if (item != null)
            {
                SelectedItems.Add(item);
            }
        }

        [RelayCommand]
        public virtual async Task MenuItemClickedAsync(object? parameter)
        {
            if (parameter is FlyoutItem flyoutItem)
            {
                if (flyoutItem.Data is ActionMode actionMode)
                {
                    switch (actionMode)
                    {
                        case ActionMode.AddNewPlaylist:
                            var (result, dialog) = await DialogService.ShowDialogAsync<CreatePlaylistDialog>();
                            if (result == ContentDialogResult.Primary)
                            {
                                var createdPlaylist = dialog.ViewModel.CreatedPlaylist;
                                if (createdPlaylist != null)
                                {
                                    await AppendTracksToPlaylistAsync(createdPlaylist.Id, flyoutItem.InsertMode);
                                    WeakReferenceMessenger.Default.Send(new PlaylistCreatedMessage(createdPlaylist.Id));
                                }
                            }
                            break;
                    }
                }
                else if (flyoutItem.Data is PlaylistSummary playlist)
                {
                    await AppendTracksToPlaylistAsync(playlist.Id, flyoutItem.InsertMode);
                }
            }
        }

        public virtual async Task AppendTracksToPlaylistAsync(int playlistId, InsertMode insertMode)
        {
            var trackIds = insertMode == InsertMode.AddAll 
                ? GetAllTrackIds() 
                : GetTrackIdsFromSelectedItems();

            if (trackIds.Count == 0)
                return;

            await Task.WhenAll(
                DataService.AppendToPlaylist(playlistId, trackIds),
                ImageService.RemoveComposedBitmaps(playlistId));

            if (insertMode == InsertMode.AddSelected)
            {
                SelectedItems.Clear();
            }

            /*
             * Use the WeakReferenceMessenger from CommunityToolkit.Mvvm.Messaging
             * to send the PlaylistChangedMessage. This ensures that the message
             * is sent without creating strong references that could lead to memory leaks.
             */
            WeakReferenceMessenger.Default.Send(new PlaylistChangedMessage(playlistId));
        }

        public virtual Task AppendSelectedTracksToPlaylistAsync(int playlistId) 
            => AppendTracksToPlaylistAsync(playlistId, InsertMode.AddSelected);

        public virtual Task AppendAllTracksToPlaylistAsync(int playlistId) 
            => AppendTracksToPlaylistAsync(playlistId, InsertMode.AddAll);
    }
}
