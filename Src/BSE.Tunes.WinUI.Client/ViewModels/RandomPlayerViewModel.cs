using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class RandomPlayerViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        private readonly Contracts.Services.IResourceService _resourceService;
        
        [ObservableProperty]
        private string _text;

        [ObservableProperty]
        private bool _isBusy;

        public RandomPlayerViewModel(IDataService dataService, Contracts.Services.IResourceService resourceService)
        {
            _dataService = dataService;
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
            await LoadSystemInfo();
            
            IsBusy = false;

        }

        private async Task LoadSystemInfo()
        {
            int countTracks = await _dataService.GetAvailableTrackCount();
            Text = string.Format(
                _resourceService.GetString("MainPage_RandomPlayer_Button_Text"), countTracks);
        }

    }
}
