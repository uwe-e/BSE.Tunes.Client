using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using Windows.Foundation;
using System.Threading;

namespace BSE.Tunes.WinUI.Client.Collections
{
    public class IncrementalObservableCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        private readonly uint _totalCount;
        private readonly Func<uint, IAsyncOperation<LoadMoreItemsResult>> _loadMoreItemsFunc;
        private uint _loadedCount;

        private Task<LoadMoreItemsResult>? _currentLoadOperation;
        private CancellationTokenSource? _cts;

        public IncrementalObservableCollection(uint totalCount, Func<uint, IAsyncOperation<LoadMoreItemsResult>> loadMoreItemsFunc)
        {
            _totalCount = totalCount;
            _loadMoreItemsFunc = loadMoreItemsFunc ?? throw new ArgumentNullException(nameof(loadMoreItemsFunc));
            _loadedCount = 0;
        }

        public bool HasMoreItems => _loadedCount < _totalCount;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            _currentLoadOperation = InternalLoadMoreItemsAsync(count);
            return _currentLoadOperation.AsAsyncOperation();
        }

        private async Task<LoadMoreItemsResult> InternalLoadMoreItemsAsync(uint count)
        {
            if (!HasMoreItems)
                return new LoadMoreItemsResult { Count = 0 };
            
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            
            try
            {
                var result = await _loadMoreItemsFunc(count);
                if (!_cts.Token.IsCancellationRequested)
                {
                    _loadedCount += result.Count;
                }
                return result;
            }
            catch (OperationCanceledException)
            {
                return new LoadMoreItemsResult { Count = 0 };
            }
        }
    }
}