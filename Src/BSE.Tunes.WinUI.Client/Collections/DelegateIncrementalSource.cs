using CommunityToolkit.WinUI.Collections;

namespace BSE.Tunes.WinUI.Client.Collections;

public sealed class DelegateIncrementalSource<T> : IIncrementalSource<T>
{
    private readonly Func<int, int, CancellationToken, Task<IEnumerable<T>>> _getPagedItemsAsync;

    public DelegateIncrementalSource(Func<int, int, CancellationToken, Task<IEnumerable<T>>> getPagedItemsAsync)
    {
        _getPagedItemsAsync = getPagedItemsAsync;
    }

    public Task<IEnumerable<T>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        return _getPagedItemsAsync(pageIndex, pageSize, cancellationToken);
    }
}