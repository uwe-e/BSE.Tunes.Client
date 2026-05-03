using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;

namespace BSE.Tunes.WinUI.Client.Collections
{
    public class IncrementalObservableCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        private readonly uint _totalCount;
        private readonly Func<uint, IAsyncOperation<LoadMoreItemsResult>> _loadMoreItemsFunc;
        private uint _loadedCount;

        public IncrementalObservableCollection(uint totalCount, Func<uint, IAsyncOperation<LoadMoreItemsResult>> loadMoreItemsFunc)
        {
            _totalCount = totalCount;
            _loadMoreItemsFunc = loadMoreItemsFunc ?? throw new ArgumentNullException(nameof(loadMoreItemsFunc));
            _loadedCount = 0;
        }

        public bool HasMoreItems => _loadedCount < _totalCount;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return InternalLoadMoreItemsAsync(count).AsAsyncOperation();
        }

        private async Task<LoadMoreItemsResult> InternalLoadMoreItemsAsync(uint count)
        {
            if (!HasMoreItems)
            {
                return new LoadMoreItemsResult { Count = 0 };
            }

            var result = await _loadMoreItemsFunc(count);
            _loadedCount += result.Count;
            return result;
        }
    }
}