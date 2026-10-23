using BSE.Tunes.Maui.Client.Events;
using BSE.Tunes.Maui.Client.ViewModels;
using System.Runtime.CompilerServices;

namespace BSE.Tunes.Maui.Client.Extensions
{
    public static class AlbumInfoSelectionExtensions
    {
        private static readonly ConditionalWeakTable<object, SubscriptionToken> _subscriptions = [];

        public static void SubscribeToAlbumSelection(
            this IAlbumInfoSelectionHandler handler,
            IEventAggregator eventAggregator)
        {
            if (!_subscriptions.TryGetValue(handler, out var token) || token == null)
            {
                var newToken = eventAggregator
                    .GetEvent<AlbumInfoSelectionEvent>()
                    .Subscribe(
                        handler.HandleShowAlbum,
                        filter: context => context.Mode == AlbumSelectionMode.Direct);

                _subscriptions.AddOrUpdate(handler, newToken);
            }
        }

        public static void UnsubscribeFromAlbumSelection(
            this IAlbumInfoSelectionHandler handler)
        {
            if (_subscriptions.TryGetValue(handler, out var token))
            {
                token?.Dispose();
                _subscriptions.Remove(handler);
            }
        }
    }
}