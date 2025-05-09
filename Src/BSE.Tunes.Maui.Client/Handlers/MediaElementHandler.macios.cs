//using CommunityToolkit.Maui.Core.Handlers;
//using CommunityToolkit.Maui.Core.Views;
//using CommunityToolkit.Maui.Views;
//using Microsoft.Maui.Handlers;
//using AVKit;

////
////namespace CommunityToolkit.Maui.Core.Handlers;

////public partial class MediaElementHandler : ViewHandler<MediaElement, MauiMediaElement>, IDisposable
////{
////    protected override MauiMediaElement CreatePlatformView()
////    {
////        throw new NotImplementedException();
////    }
////}
//namespace BSE.Tunes.Maui.Client.Handlers;

//public class MyMediaElementHandler : MediaElementHandler
//{
//    AVPlayerViewController? playerViewController;

//    protected override MauiMediaElement CreatePlatformView()
//    {
//        mediaManager ??= new(MauiContext,
//            VirtualView,
//            Dispatcher.GetForCurrentThread() ?? throw new InvalidOperationException($"{nameof(IDispatcher)} cannot be null"));

//        (_, playerViewController) = mediaManager.CreatePlatformView();

//        return new(playerViewController, VirtualView);
//        //med
//        // Create and return your custom platform view here
//        //return new MauiMediaElement();
//    }
//    //protected override MauiMediaElement CreatePlatformView()
//    //{
//    //    return new MauiMediaElement();
//    //}
//    //public void Dispose()
//    //{
//    //    // Dispose of any resources if needed
//    //}
//}