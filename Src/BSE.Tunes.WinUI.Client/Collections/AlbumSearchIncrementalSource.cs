using BSE.Tunes.WinUI.Client.Models;
using CommunityToolkit.WinUI.Collections;

namespace BSE.Tunes.WinUI.Client.Collections;

public sealed class AlbumSearchIncrementalSource : IIncrementalSource<CarouselItem>
{
    private readonly IDataService _dataService;
    private readonly IImageService _imageService;
    private readonly string _queryText;
    private readonly CancellationToken _searchCancellationToken;
    private readonly Action<bool>? _hasMoreItemsChanged;

    public AlbumSearchIncrementalSource(
        IDataService dataService,
        IImageService imageService,
        string queryText,
        CancellationToken searchCancellationToken,
        Action<bool>? hasMoreItemsChanged = null)
    {
        _dataService = dataService;
        _imageService = imageService;
        _queryText = queryText;
        _searchCancellationToken = searchCancellationToken;
        _hasMoreItemsChanged = hasMoreItemsChanged;
    }

    public async Task<IEnumerable<CarouselItem>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _searchCancellationToken);

        var result = await _dataService.GetAlbumSearchResults(_queryText, pageIndex + 1, pageSize, linkedCts.Token);
        _hasMoreItemsChanged?.Invoke(result?.HasNextPage ?? false);

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
    }
}