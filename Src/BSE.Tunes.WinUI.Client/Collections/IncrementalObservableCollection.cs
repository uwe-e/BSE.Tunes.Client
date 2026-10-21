using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;

namespace BSE.Tunes.WinUI.Client.Collections
{
    public class IncrementalObservableCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading, IDisposable
    {
        private readonly uint _totalCount;
        private readonly Func<uint, IAsyncOperation<LoadMoreItemsResult>> _loadMoreItemsFunc;
        private uint _loadedCount;

        private readonly SemaphoreSlim _loadSemaphore = new(1, 1);
        private CancellationTokenSource? _cts;
        private bool _isDisposed;

        public IncrementalObservableCollection(uint totalCount, Func<uint, IAsyncOperation<LoadMoreItemsResult>> loadMoreItemsFunc)
        {
            _totalCount = totalCount;
            _loadMoreItemsFunc = loadMoreItemsFunc ?? throw new ArgumentNullException(nameof(loadMoreItemsFunc));
            _loadedCount = 0;
        }

        public bool HasMoreItems => !_isDisposed && _loadedCount < _totalCount;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (_isDisposed)
            {
                return Task.FromResult(new LoadMoreItemsResult { Count = 0 }).AsAsyncOperation();
            }

            return AsyncInfo.Run(async cancellationToken =>
            {
                if (!HasMoreItems || _isDisposed)
                    return new LoadMoreItemsResult { Count = 0 };

                // Prevent concurrent load operations
                await _loadSemaphore.WaitAsync(cancellationToken);
                try
                {
                    if (_isDisposed)
                        return new LoadMoreItemsResult { Count = 0 };

                    // Cancel any previous operation
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                    var result = await _loadMoreItemsFunc(count).AsTask(_cts.Token);
                    
                    if (!_cts.Token.IsCancellationRequested && !_isDisposed)
                    {
                        _loadedCount += result.Count;
                    }
                    
                    return result;
                }
                catch (OperationCanceledException)
                {
                    return new LoadMoreItemsResult { Count = 0 };
                }
                catch (AccessViolationException)
                {
                    // Suppress during app shutdown - WinRT interop cleanup timing issue
                    return new LoadMoreItemsResult { Count = 0 };
                }       
                finally
                {
                    _loadSemaphore.Release();
                }
            });
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _cts?.Cancel();
            _cts?.Dispose();
            _loadSemaphore.Dispose();
        }
    }
}