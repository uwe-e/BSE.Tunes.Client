using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using System.Reflection;

namespace BSE.Tunes.MediaExtensions.Extensions
{
    public static class MediaElementExtensions
    {
        public static void InvokeCurrentStateChanged(this IMediaElement mediaElement, MediaElementState state)
        {
            MethodInfo methodInfo = typeof(IMediaElement).GetMethod("CurrentStateChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo?.Invoke(mediaElement, new object[] { state });
        }
    }
}
