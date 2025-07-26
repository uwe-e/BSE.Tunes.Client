namespace BSE.Tunes.MediaExtensions;

public static class AppHostBuilderExtensions
{
    public static MauiAppBuilder UseBSEMediaElementExtension(this MauiAppBuilder builder)
    {
        //builder.UseMauiCommunityToolkitMediaElement();
        builder.ConfigureMauiHandlers(handlers =>
        {
#if IOS
            handlers.AddHandler<CommunityToolkit.Maui.Views.MediaElement, BSE.Tunes.MediaExtensions.Handlers.MediaElementHandler>();
#endif
#if ANDROID || WINDOWS
            handlers.AddHandler<CommunityToolkit.Maui.Views.MediaElement, CommunityToolkit.Maui.Core.Handlers.MediaElementHandler>();
#endif
        });
        return builder;
    }
}