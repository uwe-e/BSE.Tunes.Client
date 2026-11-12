using BSE.Tunes.WinUI.Client.Collections;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class SearchTracksPageViewModel : PlaylistBaseViewModel<TrackItem>
    {
        private readonly IResourceService _resourceService;
        private readonly INavigationService _navigationService;
        private readonly IDataService _dataService;

        [ObservableProperty]
        public partial TrackItem? SelectedTrack { get; set; }

        [ObservableProperty]
        internal partial FuncIncrementalLoadingCollection<TrackItem> Tracks { get; set; } = default!;

        [ObservableProperty]
        public partial string? TitleText { get; set; }

        public SearchTracksPageViewModel(
            IResourceService resourceService,
            INavigationService navigationService,
            IMediaManager mediaManager,
            IDialogService dialogService,
            IImageService imageService,
            IDataService dataService) : base(dataService, imageService!, mediaManager!, dialogService!, resourceService)
        {
            _resourceService = resourceService;
            _navigationService = navigationService;
            _dataService = dataService;
        }


        public override void OnNavigatedTo(object parameter)
        {
            if (parameter is string queryText)
            {
                TitleText = string.Format(
                    _resourceService.GetString("SearchTracksPage_Title_Text"), queryText);

                _ = LoadTracksFromSearch(queryText);
            }

            base.OnNavigatedTo(parameter);
        }
        
        protected override int GetTrackId(TrackItem item) => item.Id;

        public override void PlayTrack(object? listItemData)
        {
            if (listItemData is TrackItem trackItem)
            {
                var trackIds = new ObservableCollection<int> { trackItem.Id };
                _ = MediaManager.PlayTracksAsync(trackIds, PlayerMode.Song);
            }
        }

        private async Task LoadTracksFromSearch(string queryText)
        {
            const int pageSize = 30;

            Tracks = new FuncIncrementalLoadingCollection<TrackItem>(
                async (pageIndex, pageSize, cancellationToken) =>
                {
                    var result = await _dataService.GetTrackSearchResults(
                        queryText,
                        pageIndex + 1,
                        pageSize);
                    
                    if (result?.Items is not { Count: > 0 } items)
                    {
                        return [];
                    }

                    var trackItems = new List<TrackItem>(items.Count);

                    for (int i = 0; i < items.Count; i++)
                    {
                        var track = items[i];
                        if (track != null)
                        {
                            trackItems.Add(TrackItem.FromTrack(track));
                        }
                    }

                    return trackItems;
                },
                pageSize);

            await Tracks.LoadMoreItemsAsync((uint)pageSize);
        }
    }
}
