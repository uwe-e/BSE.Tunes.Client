using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class TracklistBaseViewModel : ViewModelBase
    {
        private ObservableCollection<GridPanel>? _items;
        private string _imageSource;
        private ICommand _openFlyoutCommand;
        private ICommand _playCommand;
        private ICommand _playAllCommand;
        private ICommand _playAllRandomizedCommand;
        private readonly IFlyoutNavigationService _flyoutNavigationService;
        private readonly IMediaManager _mediaManager;

        public ICommand OpenFlyoutCommand => _openFlyoutCommand ??= new DelegateCommand<object>(OpenFlyout);

        public ICommand PlayCommand => _playCommand ??= new DelegateCommand<GridPanel>(PlayTrack);

        public ICommand PlayAllCommand => _playAllCommand ??= new DelegateCommand(PlayAll, CanPlayAll);

        public ICommand PlayAllRandomizedCommand => _playAllRandomizedCommand ??= new DelegateCommand(PlayAllRandomized, CanPlayAllRandomized);

        public ObservableCollection<GridPanel> Items => _items ??= [];

        public string ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                SetProperty<string>(ref _imageSource, value);
            }
        }

        public TracklistBaseViewModel(
            INavigationService navigationService,
            IFlyoutNavigationService flyoutNavigationService,
            IMediaManager mediaManager) : base(navigationService)
        {
            _flyoutNavigationService = flyoutNavigationService;
            _mediaManager = mediaManager;
        }

        protected async void OpenFlyout(object obj)
        {
            var source = new PlaylistActionContext
            {
                Data = obj,
            };

            if (obj is GridPanel item)
            {
                source.Data = item.Data;
            }

            var navigationParams = new NavigationParameters();
            navigationParams.Add("source", source);
            navigationParams.Add(KnownNavigationParameters.UseModalNavigation, true);
            navigationParams.Add(KnownNavigationParameters.Animated, false);

            await _flyoutNavigationService.ShowFlyoutAsync(nameof(PlaylistActionToolbarPage), navigationParams);
        }

        protected virtual void PlayTrack(GridPanel panel)
        {
        }

        protected virtual bool CanPlayAll()
        {
            return Items.Count > 0;
        }

        protected virtual void PlayAll()
        {
        }

        protected virtual bool CanPlayAllRandomized()
        {
            return CanPlayAll();
        }

        protected virtual void PlayAllRandomized()
        {
        }

        protected virtual void PlayTracks(IEnumerable<int> trackIds, PlayerMode playerMode)
        {
            _mediaManager.PlayTracks(
                new ObservableCollection<int>(trackIds), playerMode);
        }
    }
}
