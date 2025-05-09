using AVKit;
using CommunityToolkit.Maui.Core.Handlers;
using CommunityToolkit.Maui.Core.Views;

namespace BSE.Tunes.MediaExtensions.Handlers;

public class MediaElementHandler : CommunityToolkit.Maui.Core.Handlers.MediaElementHandler
{
    AVPlayerViewController? playerViewController;

    protected override MauiMediaElement CreatePlatformView()
    {
        mediaManager ??= new(MauiContext,
            VirtualView,
            Dispatcher.GetForCurrentThread() ?? throw new InvalidOperationException($"{nameof(IDispatcher)} cannot be null"));
        // Create and return your custom platform view here

        (_, playerViewController) = mediaManager.CreatePlatformView();

        return new(playerViewController, VirtualView);
        //return null;
        //return new MauiMediaElement();
    }
}