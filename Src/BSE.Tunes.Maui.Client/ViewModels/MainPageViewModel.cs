using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class MainPageViewModel: PlayerBaseViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDataService _dataService;
        private Uri _coverSource;
        private DelegateCommand<Track> _selectTrackCommand;

        public DelegateCommand<Track> SelectTrackCommand => _selectTrackCommand
            ??= new DelegateCommand<Track>(SelectTrack, CanSelectTrack);

        

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
            if (track != null)
            {
                Uri coverSource = _dataService.GetImage(track.Album.AlbumId, true);
                if (coverSource != null && !coverSource.Equals(CoverSource))
                {
                    CoverSource = coverSource;
                }
            }
        }
        
        private bool CanSelectTrack(Track track)
        {
            return track != null;
        }

        private async void SelectTrack(Track track)
        {
            var navigationParams = new NavigationParameters
            {
                { "source", track },
                { KnownNavigationParameters.UseModalNavigation, true }
            };
            await NavigationService.NavigateAsync(nameof(NowPlayingPage), navigationParams);
        }
    }
}
