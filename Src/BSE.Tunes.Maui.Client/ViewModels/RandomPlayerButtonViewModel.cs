using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class RandomPlayerButtonViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMediaManager _mediaManager;
        private readonly IResourceService _resourceService;
        private readonly IDataService _dataService;
        private ObservableCollection<int> _trackIds;
        private DelegateCommand _playRandomCommand;
        private string _text;

        public DelegateCommand PlayRandomCommand => _playRandomCommand ??= new DelegateCommand(PlayRandom, CanPlayRandom);

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                SetProperty<string>(ref _text, value);
            }
        }

        public RandomPlayerButtonViewModel(INavigationService navigationService,
            IEventAggregator eventAggregator,
            IMediaManager mediaManager,
            IResourceService resourceService,
            IDataService dataService) : base(navigationService)
        {
            _eventAggregator = eventAggregator;
            _mediaManager = mediaManager;
            _resourceService = resourceService;
            _dataService = dataService;

            LoadData();
        }

        private void LoadData()
        {
            Task.Run(async () =>
            {
                await LoadDataAsync();
            });
        }

        private async Task LoadDataAsync()
        {
            ObservableCollection<int> trackIds = await _dataService.GetTrackIdsByGenre();
            if (trackIds != null)
            {
                _trackIds = trackIds.ToRandomCollection();
                int trackId = _trackIds.FirstOrDefault();
                if (trackId > 0)
                {
                    var track = await _dataService.GetTrackById(trackId);
                    if (track != null)
                    {
                        _eventAggregator.GetEvent<TrackChangedEvent>().Publish(track);
                    }
                }
                _mediaManager.Playlist = _trackIds.ToNavigableCollection();
                PlayRandomCommand.RaiseCanExecuteChanged();
            }
            await LoadSystemInfo();

            IsBusy = false;
        }

        private async Task LoadSystemInfo()
        {
            var sysInfo = await _dataService.GetSystemInfo();
            if (sysInfo != null)
            {
                Text = string.Format(_resourceService.GetString("HomePage_RandomPlayerButton_Button_Text"), sysInfo.NumberTracks);
            }
        }

        private bool CanPlayRandom()
        {
            return _trackIds?.Count > 0;
        }

        private void PlayRandom()
        {
            _trackIds = _trackIds?.ToRandomCollection();
            if (_trackIds != null)
            {
                _mediaManager.PlayTracks(new ObservableCollection<int>(_trackIds), PlayerMode.Random);
            }
        }
    }
}
