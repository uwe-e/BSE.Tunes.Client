using BSE.Tunes.WinUI.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class AlbumDetailPageViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private Album? _album;

        [ObservableProperty]
        private ImageSource? _coverImageSource;

        [ObservableProperty]
        private ObservableCollection<TrackItem> _tracks = [];

        [ObservableProperty]
        private ObservableCollection<Track> _selectedItems = [];

        [ObservableProperty]
        private bool _isBusy;

        public AlbumDetailPageViewModel(
            IDataService dataService,
            IImageService imageService,
            IMessenger messenger)
        {
            _dataService = dataService;
            _imageService = imageService;
            _messenger = messenger;
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
                Album = await _dataService.GetAlbumById(albumId);

                if (Album != null)
                {
                    var imagePath = _imageService.GetBitmapSource(Album.AlbumId, false);
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        CoverImageSource = new BitmapImage(new Uri(imagePath));
                    }
                    await LoadTracksAsync(Album);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadTracksAsync(Album album)
        {
            Tracks.Clear();
            if (Album?.Tracks != null)
            {
                foreach (var track in Album.Tracks)
                {
                    if (track != null)
                    {
                        Tracks.Add(TrackItem.FromTrack(track));
                    }
                        
                    //Tracks.Add(new TrackItem
                    //{
                    //    TrackNumber = track.TrackNumber,
                    //    Title = track.Name ?? string.Empty,
                    //    Artist = Album.Artist?.Name ?? string.Empty,
                    //    Duration = track.Duration,
                    //    Data = track
                    //});
                }
            }

        }
    }
}
