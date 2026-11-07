using CommunityToolkit.WinUI.Collections;

namespace BSE.Tunes.WinUI.Client.Collections;

public sealed class FuncIncrementalLoadingCollection<T> : IncrementalLoadingCollection<DelegateIncrementalSource<T>, T>
{
    public FuncIncrementalLoadingCollection(
        Func<int, int, CancellationToken, Task<IEnumerable<T>>> getPagedItemsAsync,
        int itemsPerPage,
        Action? onStartLoading = null,
        Action? onEndLoading = null,
        Action<Exception>? onError = null)
        : base(new DelegateIncrementalSource<T>(getPagedItemsAsync), itemsPerPage, onStartLoading, onEndLoading, onError)
    {
    }
}