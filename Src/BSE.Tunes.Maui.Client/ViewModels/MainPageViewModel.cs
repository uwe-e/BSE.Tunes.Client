using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class MainPageViewModel: PlayerBaseViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDataService _dataService;
        private Uri _coverSource;

        public Uri CoverSource
        {
            get { return _coverSource; }
            set { SetProperty<Uri>(ref _coverSource, value); }
        }

        public MainPageViewModel(
        INavigationService navigationService,
        IEventAggregator eventAggregator,
        IMediaManager mediaManager,
        IDataService dataService) : base(navigationService, eventAggregator, mediaManager)
        {
            _eventAggregator = eventAggregator;
            _dataService = dataService;
            _eventAggregator.GetEvent<TrackChangedEvent>().Subscribe(args => {
                if (args is Track track)
                {
                    CurrentTrack = track;
                    LoadCoverSource(CurrentTrack);
                }
            }, ThreadOption.UIThread);
        }

        protected override void OnTrackChanged(Track track)
        {
           LoadCoverSource(track);
        }

        private void LoadCoverSource(Track track)
        {
            Task.Run(async () =>
            {
                await LoadCoverSourceAsync(track);
            });
        }

        private async Task LoadCoverSourceAsync(Track currentTrack)
        {
            if (currentTrack != null)
            {
                Uri coverSource = _dataService.GetImage(currentTrack.Album.AlbumId, true);
                if (coverSource != null && !coverSource.Equals(CoverSource))
                {
                    CoverSource = coverSource;
                }
            }
        }
    }
}
