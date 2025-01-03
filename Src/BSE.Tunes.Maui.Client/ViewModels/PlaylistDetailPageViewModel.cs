using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using System.Collections.ObjectModel;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class PlaylistDetailPageViewModel : TracklistBaseViewModel
    {
        private Playlist _playlist;
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly ISettingsService _settingsService;

        public Playlist Playlist
        {
            get => _playlist;
            set => SetProperty<Playlist>(ref _playlist, value);
        }

        public PlaylistDetailPageViewModel(
            INavigationService navigationService,
            IFlyoutNavigationService flyoutNavigationService,
            IDataService dataService,
            IMediaManager mediaManager,
            IImageService imageService,
            IEventAggregator eventAggregator,
            ISettingsService settingsService) : base(navigationService, flyoutNavigationService, dataService, mediaManager, imageService, eventAggregator)
        {
            _dataService = dataService;
            _imageService = imageService;
            _settingsService = settingsService;
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            Playlist playlist = parameters.GetValue<Playlist>("playlist");
            LoadData(playlist);
        }

        protected override void PlayTrack(GridPanel panel)
        {
            if (panel?.Data is PlaylistEntry entry)
            {
                PlayTracks(new List<int>
                {
                    entry.TrackId
                }, PlayerMode.Song);
            }
        }

        protected override void PlayAll()
        {
            PlayTracks(GetTrackIds(), PlayerMode.Playlist);
        }

        protected override void PlayAllRandomized()
        {
            PlayTracks(GetTrackIds().ToRandomCollection(), PlayerMode.Playlist);
        }

        protected override ObservableCollection<int> GetTrackIds()
        {
            return new ObservableCollection<int>(Items.Select(track => ((PlaylistEntry)track.Data).TrackId));
        }

        protected override Task RemoveFromPlaylistAsync(PlaylistActionContext managePlaylistContext)
        {
            return base.RemoveFromPlaylistAsync(managePlaylistContext);
        }

        private void LoadData(Playlist playlist)
        {
            _ = LoadDataAsync(playlist);
        }

        private async Task LoadDataAsync(Playlist playlist)
        {
            if (playlist != null)
            {
                Items.Clear();
                ImageSource = null;

                Playlist = await _dataService.GetPlaylistById(playlist.Id, _settingsService.User.UserName);
                if (Playlist != null)
                {
                    ImageSource = await _imageService.GetStitchedBitmapSource(playlist.Id);

                    foreach (var entry in Playlist.Entries?.OrderBy(pe => pe.SortOrder))
                    {
                        if (entry != null)
                        {
                            Items.Add(new GridPanel
                            {
                                Id = entry.Id,
                                Title = entry.Name,
                                SubTitle = entry.Artist,
                                ImageSource = _imageService.GetBitmapSource(entry.AlbumId, true),
                                Data = entry
                            });
                        }
                    }

                    PlayAllCommand.RaiseCanExecuteChanged();
                    PlayAllRandomizedCommand.RaiseCanExecuteChanged();

                    IsBusy = false;
                }
            }
        }
    }
}
