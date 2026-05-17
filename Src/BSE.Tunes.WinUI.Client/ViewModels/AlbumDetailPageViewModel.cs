using BSE.Tunes.Shared.Services.Extensions;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Messages;
using BSE.Tunes.WinUI.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class AlbumDetailPageViewModel : PlaylistBaseViewModel<TrackItem>
    {
        [ObservableProperty]
        private Album? _album;

        [ObservableProperty]
        private TrackItem? _selectedTrack;

        public AlbumDetailPageViewModel(
            IDataService dataService,
            IImageService imageService,
            IMediaManager mediaManager,
            IDialogService dialogService,
            IResourceService resourceService) : base(dataService, imageService, mediaManager, dialogService, resourceService)
        {
        }

        public override void OnNavigatedTo(object parameter)
        {
            base.OnNavigatedTo(parameter);

            if (parameter is Album album)
            {
                _ = LoadAlbumAsync(album.Id);
            }
        }

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
