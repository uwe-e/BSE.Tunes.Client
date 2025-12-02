using CommunityToolkit.Maui.Core;
using System.Diagnostics.CodeAnalysis;
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

        public static bool TryFindParent<T>(this Element? child, [NotNullWhen(true)] out T? parent) where T : VisualElement
        {
            while (child is not null)
            {
                if (child.Parent is T element)
                {
                    parent = element;
                    return true;
                }

                child = child.Parent;
            }

            parent = null;
            return false;
        }

        public static bool TryFindParentPlatformView<T>(this Element? child, [NotNullWhen(true)] out T? parentPlatformView)
        {
            while (child is not null)
            {
                if (child.Parent?.Handler?.PlatformView is T element)
                {
                    parentPlatformView = element;
                    return true;
                }

                child = child.Parent;
            }

            parentPlatformView = default;
            return false;
        }
    }
}
