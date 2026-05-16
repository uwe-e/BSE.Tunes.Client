using BSE.Tunes.Shared.Services.Extensions;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Messages;
using BSE.Tunes.WinUI.Client.Models;
using BSE.Tunes.WinUI.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class PlaylistDetailPageViewModel : PlaylistBaseViewModel<PlaylistEntryItem>, IRecipient<PlaylistChangedMessage>
    {
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private Playlist? _playlist;
        
        //[ObservableProperty]
        //private ObservableCollection<PlaylistEntryItem> _items = [];
        
        //[ObservableProperty]
        //private ObservableCollection<FlyoutItem> _playlistMenuItems = [];

        //[ObservableProperty]
        //private ObservableCollection<PlaylistEntryItem> _selectedItems = [];

        [ObservableProperty]
        private PlaylistEntryItem? _selectedItem;

        //[ObservableProperty]
        //private ListViewSelectionMode _selectionMode = ListViewSelectionMode.Single;

        //[ObservableProperty]
        //private ImageSource? _imageSource;

        //[ObservableProperty]
        //private bool _isBusy;

        //[ObservableProperty]
        //private bool _isCommandBarVisible;

        //[ObservableProperty]
        //private bool _isItemClickEnabled = true;

        //public bool HasSelectedItems => SelectedItems.Count > 0;

        public PlaylistDetailPageViewModel(
            IDataService dataService,
            IImageService imageService,
            IMediaManager mediaManager,
            IDialogService dialogService,
            INavigationService navigationService,
            IResourceService resourceService,
            IMessenger messenger) : base(dataService, imageService, mediaManager, dialogService, resourceService)
        {
            _navigationService = navigationService;

            //SelectedItems.CollectionChanged += OnSelectedItemsChanged;

            

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

            Messenger.Register<PlaylistChangedMessage>(this);

            if (parameter is Playlist playlist)
            {
                _ = LoadPlaylistAsync(playlist.Id);
            }
        }

        public override void OnNavigatedFrom()
        {
            base.OnNavigatedFrom();

            Messenger.Unregister<PlaylistChangedMessage>(this);
        }

        //private async Task LoadPlaylistsAsync()
        //{
        //    var playlists = await DataService.GetAllPlaylists();
        //    PlaylistMenuItems.Clear();

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
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<Playlist?> LoadPlaylistByIdAsync(int playlistId)
        {
            var playlist = await DataService.GetPlaylistById(playlistId);
            if (playlist != null)
            {
                var imagePath = await ImageService.GetComposedBitmapSourceAsync(playlistId, playlist.CoverAlbumIds);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    ImageSource = new BitmapImage
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

            var pagedEntries = await DataService.GetPagedPlaylistEntriesByIdAsync(
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
        //private void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        //{
        //    OnPropertyChanged(nameof(HasSelectedItems));
        //    IsCommandBarVisible = HasSelectedItems;
        //    SelectionMode = HasSelectedItems ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Extended;
        //    IsItemClickEnabled = !HasSelectedItems;
        //}

        [RelayCommand]
        private async Task RemoveSelectionAsync()
        {
            IsBusy = true;

            try
            {
                var count = SelectedItems.Count;
                if (count > 0)
                {
                    var entryIds = SelectedItems
                        .OfType<PlaylistEntryItem>()
                        .Select(item => item.Id)
                        .ToList();

                    await Task.WhenAll(
                        DataService.DeletePlaylistEntriesAsync(Playlist.Id, entryIds),
                        ImageService.RemoveComposedBitmaps(Playlist.Id));

                    Messenger.Send(new PlaylistChangedMessage(Playlist.Id));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        //[RelayCommand]
        //private void SelectItems(PlaylistEntryItem? item)
        //{
        //    if (item != null && !SelectedItems.Contains(item))
        //    {
        //        SelectedItems.Add(item);
        //    }
        //}

        [RelayCommand]
        private async Task RemovePlaylistAsync()
        {
            var(result, dialog) = await DialogService.ShowDialogAsync<DeletePlaylistDialog>();
            if (result == ContentDialogResult.Primary)
            {
                await DataService.DeletePlaylist(Playlist!.Id);

                Messenger.Send(new PlaylistDeletedMessage(Playlist.Id));
            }

        }

        //[RelayCommand]
        //private void ClearSelection()
        //{
        //    SelectedItems.Clear();
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

        public override void PlayTrack(object? listItemData)
        {
            if (listItemData is PlaylistEntryItem entryItem)
            {
                var trackIds = new ObservableCollection<int> { entryItem.TrackId };
                _ = MediaManager.PlayTracksAsync(trackIds, PlayerMode.Song);
            }
        }

        public override void PlayAll()
        {
            var entryIds = new ObservableCollection<int>(Items.OfType<PlaylistEntryItem>().Select(item => item.TrackId));
            _ = MediaManager.PlayTracksAsync(entryIds, PlayerMode.Playlist);
        }

        public override void PlayAllShuffle()
        {
            var entryIds = new ObservableCollection<int>(Items.OfType<PlaylistEntryItem>().Select(item => item.TrackId));
            _ = MediaManager.PlayTracksAsync(entryIds.ToRandomCollection(), PlayerMode.Playlist);
        }

        [RelayCommand]
        private async Task PlaySelectedAsync()
        {
            if (SelectedItems != null)
            {
                var entryItems = SelectedItems.OfType<PlaylistEntryItem>().ToList();
                var entryIds = new ObservableCollection<int>(entryItems.Select(t => t.Id));
                await MediaManager.InsertTracksToPlayQueueAsync(entryIds, PlayerMode.Song);
                SelectedItems.Clear();
            }
        }

        public override void PlayAsNext()
        {
            if (SelectedItems != null)
            {
                var entryItems = SelectedItems.OfType<PlaylistEntryItem>().ToList();
                var entryIds = new ObservableCollection<int>(entryItems.Select(t => t.Id));
                _ = MediaManager.InsertTracksToPlayQueueAsync(entryIds, PlayerMode.Song);
                SelectedItems.Clear();
            }
        }

        //[RelayCommand]
        //private async Task MenuItemClicked(object? parameter)
        //{
        //    if (parameter is FlyoutItem flyoutItem)
        //    {
        //        if (flyoutItem.Data is ActionMode actionMode)
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
            var count = SelectedItems.Count;
            if (count > 0)
            {
                var entryIds = SelectedItems
                    .OfType<PlaylistEntryItem>()
                    .Select(item => item.TrackId)
                    .ToList();

                await Task.WhenAll(
                    DataService.AppendToPlaylist(playlistId, entryIds),
                    ImageService.RemoveComposedBitmaps(playlistId));

                Messenger.Send(new PlaylistChangedMessage(playlistId));
            }
        }

        public async Task UpdatePlaylist(int playlistId)
        {
            if (Playlist != null && Playlist.Id == playlistId)
            {
                await LoadPlaylistAsync(playlistId);
            }
        }

        void IRecipient<PlaylistChangedMessage>.Receive(PlaylistChangedMessage message)
        {
            _ = UpdatePlaylist(message.PlaylistId);
        }

        [RelayCommand]
        private void DragItemsCompleted(DragItemsCompletedEventArgs e)
        {
            if (e.Items is { Count: 1 } && e.Items[0] is PlaylistEntryItem entry)
            {
                _ = UpdateReorderedPlaylist(entry);
            }
        }

        private async Task UpdateReorderedPlaylist(PlaylistEntryItem entry)
        {
            IsBusy = true;

            try
            {
                var newIndex = Items.IndexOf(entry);
                if (newIndex >= 0)
                {
                    var count = Items.Count;
                    var entriesIds = new List<int>(count);

                    for (int i = 0; i < count; i++)
                    {
                        if (Items[i].Data is PlaylistEntry playlistEntry)
                        {
                            entriesIds.Add(playlistEntry.Id);
                        }
                    }

                    await DataService.UpdatePlaylistEntriesSortOrderAsync(Playlist.Id, entriesIds);

                    if (newIndex <= 4)
                    {
                        await LoadPlaylistAsync(Playlist.Id);
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
