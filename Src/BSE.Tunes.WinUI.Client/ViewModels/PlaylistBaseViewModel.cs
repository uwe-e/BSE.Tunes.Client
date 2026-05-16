using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Models;
using BSE.Tunes.WinUI.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class PlaylistBaseViewModel<TItem> : ViewModelBase
    {
        public IDataService DataService { get; }
        public IImageService ImageService { get; }
        public IMediaManager MediaManager { get; }
        public IDialogService DialogService { get; }
        public IResourceService ResourceService { get; }

        [ObservableProperty]
        public ImageSource? _imageSource;

        [ObservableProperty]
        public bool _isBusy;

        [ObservableProperty]
        public ListViewSelectionMode _selectionMode = ListViewSelectionMode.Extended;

        [ObservableProperty]
        public bool _isCommandBarVisible;

        [ObservableProperty]
        public bool _isItemClickEnabled = true;

        [ObservableProperty]
        public ObservableCollection<FlyoutItem> _playlistMenuItems = [];

        [ObservableProperty]
        private ObservableCollection<TItem> _items = [];

        [ObservableProperty]
        private ObservableCollection<object> _selectedItems = [];

        [ObservableProperty]
        public bool _allItemsSelected;
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

            _ = LoadPlaylistMenuAsync();
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

        protected virtual async Task LoadPlaylistMenuAsync()
        {
            var playlists = await DataService.GetAllPlaylists();
            PlaylistMenuItems.Clear();

            try
            {
                PlaylistMenuItems.Add(new FlyoutItem
                {
                    Text = ResourceService.GetString("PlaylistBaseViewModel_MenuPlaylists_NewPlaylistText"),
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

        [RelayCommand]
        public virtual void PlayTrack(object? listItemData) { }

        [RelayCommand]
        public virtual void PlayAll() { }

        [RelayCommand]
        public virtual void PlayAllShuffle() { }

        [RelayCommand]
        public virtual void PlaySelected() { }

        [RelayCommand]
        public virtual void PlayAsNext() { }

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

        public virtual Task AppendSelectedTracksToPlaylistAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
