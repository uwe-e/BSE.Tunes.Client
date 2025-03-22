using BSE.Tunes.Maui.Client.Models;

namespace BSE.Tunes.Maui.Client.Extensions
{
    public static class EventAggregatorExtension
    {
        public static SubscriptionToken ShowAlbum(this PubSubEvent<UniqueAlbum> pubSubEvent, Action<UniqueAlbum> action)
        {
            return pubSubEvent.Subscribe(action, ThreadOption.UIThread);
        }
    }
}
