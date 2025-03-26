using BSE.Tunes.Maui.Client.Models.Contract;
using BSE.Tunes.Maui.Client.Services;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class NowPlayingPageViewModel : PlayerBaseViewModel
    {
        private string _coverImage;
        private ICommand _closeDialogCommand;
        private readonly IImageService _imageService;
        private readonly IMediaManager _mediaManager;

        public ICommand CloseDialogCommand => _closeDialogCommand ??= new DelegateCommand(async () =>
        {
            await CloseDialog();
        });

        public string CoverImage
        {
            get => _coverImage;
            set => SetProperty(ref _coverImage, value);
        }

        public NowPlayingPageViewModel(
            INavigationService navigationService,
            IEventAggregator eventAggregator,
            IDataService dataService,
            IImageService imageService,
            IMediaManager mediaManager) : base(navigationService, eventAggregator, mediaManager)
        {
            _imageService = imageService;
            _mediaManager = mediaManager;
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetValue<Track>("source") is Track currentTrack)
            {
                CurrentTrack = currentTrack;
                CoverImage = _imageService.GetBitmapSource(CurrentTrack.Album.AlbumId);
            }
            //Progress = _mediaManager.Progress;
            //MediaPlayerState = PlayerManager.AudioPlayerState;

            base.OnNavigatedTo(parameters);
        }

        protected override void OnTrackChanged(Track track)
        {
            if (track != null)
            {
                CoverImage = _imageService.GetBitmapSource(track.Album.AlbumId);
            }
        }

        private async Task CloseDialog()
        {
            var navigationParams = new NavigationParameters
            {
                { KnownNavigationParameters.UseModalNavigation, true }
            };
            await NavigationService.GoBackAsync(navigationParams);
        }
    }
}
