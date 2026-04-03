using BSE.Tunes.Shared.Services.Extensions;
using BSE.Tunes.WinUI.Client.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class RandomPlayerViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        private readonly IMediaManager _mediaManager;
        private readonly IMessenger _messenger;
        private readonly Contracts.Services.IResourceService _resourceService;
        private ObservableCollection<int> _trackIds;
        
        [ObservableProperty]
        private string _text;

        [ObservableProperty]
        private bool _isBusy;

        public RandomPlayerViewModel(
            IDataService dataService,
            IMediaManager mediaManager,
            IMessenger messenger,
            Contracts.Services.IResourceService resourceService)
        {
            _dataService = dataService;
            _mediaManager = mediaManager;
            _messenger = messenger;
            _resourceService = resourceService;

            LoadData();
        }

        private void LoadData()
        {
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsBusy = true;

            ObservableCollection<int> trackIds = new(
                //When GetTrackIdsByGenre returns null, we fallback to an empty list
                await _dataService.GetTrackIdsByGenre() ?? []
            );
            if (trackIds != null)
            {
                _trackIds = trackIds.ToRandomCollection();
                int trackId = _trackIds.FirstOrDefault();
                if (trackId > 0)
                {
                    var track = await _dataService.GetTrackById(trackId);
                    if (track != null)
                    {
                        _messenger.Send(new TrackChangedMessage(track));
                    }
                }
                _mediaManager.Playlist = _trackIds.ToNavigableCollection();
            }

            await LoadSystemInfo();

            ButtonPlayRandomCommand?.NotifyCanExecuteChanged();

            IsBusy = false;

        }

        private async Task LoadSystemInfo()
        {
            int countTracks = await _dataService.GetAvailableTrackCount();
            Text = string.Format(
                _resourceService.GetString("MainPage_RandomPlayer_Button_Text"), countTracks);
        }

        private bool CanPlayRandom()
        {
            return _trackIds?.Count > 0;
        }

        [RelayCommand(CanExecute = nameof(CanPlayRandom))]
        private async Task ButtonPlayRandomAsync()
        {
            _trackIds = _trackIds?.ToRandomCollection();
            if (_trackIds != null)
            {
                await _mediaManager.PlayTracksAsync(new ObservableCollection<int>(_trackIds), PlayerMode.Random);
            }
        }
    }
}
