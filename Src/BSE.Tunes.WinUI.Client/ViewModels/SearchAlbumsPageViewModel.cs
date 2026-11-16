
using BSE.Tunes.WinUI.Client.Collections;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Models;
using BSE.Tunes.WinUI.Client.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;


namespace BSE.Tunes.WinUI.Client.ViewModels
{
    public partial class SearchAlbumsPageViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly IImageService _imageService;
        private readonly IResourceService _resourceService;
        private INavigationService _navigationService;

        [ObservableProperty]
        internal partial FuncIncrementalLoadingCollection<CarouselItem> Albums { get; set; } = default!;

        [ObservableProperty]
        public partial string? TitleText { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CarouselItem> SelectedItems { get; set; } = new ObservableCollection<CarouselItem>();

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        public SearchAlbumsPageViewModel(
            IDataService dataService,
            IImageService imageService,
            IResourceService resourceService,
            INavigationService navigationService)
        {
            _dataService = dataService;
            _imageService = imageService;
            _resourceService = resourceService;
            _navigationService = navigationService;
        }

        public async override void OnNavigatedTo(object parameter)
        {
            if (parameter is string queryText)
            {
                TitleText = string.Format(
                    _resourceService.GetString("SearchAlbumsPage_Title_Text"), queryText);

                _ = LoadAlbumsFromSearchAsync(queryText);
            }

            base.OnNavigatedTo(parameter);
        }

        private async Task LoadAlbumsFromSearchAsync(string queryText)
        {
            const int pageSize = 30;

            Albums = new FuncIncrementalLoadingCollection<CarouselItem>(
                async (pageIndex, pageSize, cancellationToken) =>
                {
                    var result = await _dataService.GetAlbumSearchResults(
                        queryText,
                        pageIndex + 1,
                        pageSize);

                    if (result?.Items is not { Count: > 0 } items)
                    {
                        return [];
                    }

                    var carouselItems = new List<CarouselItem>(items.Count);
                    for (int i = 0; i < items.Count; i++)
                    {
                        var album = items[i];
                        if (album != null)
                        {
                            carouselItems.Add(new CarouselItem
                            {
                                Title = album.Title ?? string.Empty,
                                SubTitle = album.Artist?.Name ?? string.Empty,
                                ImagePath = _imageService.GetBitmapSource(album.AlbumId, true),
                                Data = album
                            });
                        }
                    }

                    return carouselItems;
                },
                pageSize);

            IsBusy = true;

            await Albums.LoadMoreItemsAsync((uint)pageSize);

            IsBusy = false;
        }

        [RelayCommand]
        private async Task SelectItemAsync(CarouselItem? item)
        {
            if (item?.Data != null)
            {
                await _navigationService.NavigateToAsync(nameof(AlbumDetailPage), item.Data);
            }
        }
    }
}
