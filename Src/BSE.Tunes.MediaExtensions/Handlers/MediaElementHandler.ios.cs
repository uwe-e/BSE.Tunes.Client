using AVKit;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Views;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Handlers;

namespace BSE.Tunes.MediaExtensions.Handlers;

/// <summary>
/// Handler class for <see cref="MediaElement"/>.
/// </summary>
public partial class MediaElementHandler : ViewHandler<MediaElement, Views.MauiMediaElement>, IDisposable
{
    AVPlayerViewController? playerViewController;

    protected MediaManager? MediaManager { get; set; }

    /// <summary>
	/// Initializes a new instance of the <see cref="MediaElementHandler"/> class.
	/// </summary>
    public MediaElementHandler()
        : base(PropertyMapper, CommandMapper)
    {
    }

    /// <summary>
	/// The default property mapper for this handler.
	/// </summary>
    public static IPropertyMapper<MediaElement, MediaElementHandler> PropertyMapper
        = new PropertyMapper<MediaElement, MediaElementHandler>(ViewMapper)
        {
            //[nameof(IMediaElement.ShouldShowPlaybackControls)] = MapShouldShowPlaybackControls,
            [nameof(IMediaElement.Source)] = MapSource
        };

    /// <summary>
	/// The default command mapper for this handler.
	/// </summary>
    public static CommandMapper<MediaElement, MediaElementHandler> CommandMapper = new(ViewCommandMapper)
    {
        ["StatusUpdated"] = MapStatusUpdated,
        ["PlayRequested"] = MapPlayRequested,
        ["PauseRequested"] = MapPauseRequested,
        //[nameof(MediaElement.SeekRequested)] = MapSeekRequested,
        ["StopRequested"] = MapStopRequested
    };



    protected override Views.MauiMediaElement CreatePlatformView()
    {
        MediaManager = new Views.MediaManager(MauiContext,
            VirtualView,
            Dispatcher.GetForCurrentThread() ?? throw new InvalidOperationException($"{nameof(IDispatcher)} cannot be null"));

        (_, playerViewController) = ((Views.MediaManager)MediaManager).CreateCustomPlatformView();

        try
        {
            return new(playerViewController, VirtualView);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MauiMediaElement ctor threw: {ex.GetType()}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            throw;
        }
    }

    protected override void DisconnectHandler(Views.MauiMediaElement platformView)
    {
        platformView.Dispose();
        Dispose();
        base.DisconnectHandler(platformView);
    }

    /// <summary>
	/// Maps the pause operation request between the abstract <see cref="MediaElement"/> and platform counterpart.
	/// </summary>
	/// <param name="handler">The associated handler.</param>
	/// <param name="mediaElement">The associated <see cref="MediaElement"/> instance.</param>
	/// <param name="args">The associated event arguments for this request.</param>
	/// <remarks><paramref name="args"/> is not used.</remarks>
    public static void MapPauseRequested(MediaElementHandler handler, MediaElement element, object? arg3)
    {
        handler.MediaManager?.Pause();
    }

    /// <summary>
	/// Maps the play operation request between the abstract <see cref="MediaElement"/> and platform counterpart.
	/// </summary>
	/// <param name="handler">The associated handler.</param>
	/// <param name="mediaElement">The associated <see cref="MediaElement"/> instance.</param>
	/// <param name="args">The associated event arguments for this request.</param>
	/// <remarks><paramref name="args"/> is not used.</remarks>
    public static void MapPlayRequested(MediaElementHandler handler, MediaElement mediaElement, object? args)
    {
        handler.MediaManager?.Play();
    }

    /// <summary>
	/// Maps the status update between the abstract <see cref="MediaElement"/> and platform counterpart.
	/// </summary>
	/// <param name="handler">The associated handler.</param>
	/// <param name="mediaElement">The associated <see cref="MediaElement"/> instance.</param>
	/// <param name="args">The associated event arguments for this request.</param>
	/// <remarks><paramref name="args"/> is not used.</remarks>
    public static void MapStatusUpdated(MediaElementHandler handler, MediaElement mediaElement, object? args)
    {
        handler.MediaManager?.UpdateStatus();
    }

    /// <summary>
	/// Maps the status update between the abstract <see cref="MediaElement"/> and platform counterpart.
	/// </summary>
	/// <param name="handler">The associated handler.</param>
	/// <param name="mediaElement">The associated <see cref="MediaElement"/> instance.</param>
	/// <param name="args">The associated event arguments for this request.</param>
	/// <remarks><paramref name="args"/> is not used.</remarks>
    public static void MapSource(MediaElementHandler handler, MediaElement mediaElement)
    {
        handler.MediaManager?.UpdateSource();
    }

    /// <summary>
    /// Maps the stop operation request between the abstract <see cref="MediaElement"/> and platform counterpart.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="mediaElement">The associated <see cref="MediaElement"/> instance.</param>
    /// <param name="args">The associated event arguments for this request.</param>
    /// <remarks><paramref name="args"/> is not used.</remarks>
    public static void MapStopRequested(MediaElementHandler handler, MediaElement mediaElement, object? args)
    {
        handler.MediaManager?.Stop();
    }
    //private static void MapShouldShowPlaybackControls(MediaElementHandler handler, MediaElement element)
    //{
    //    throw new NotImplementedException();
    //}

    /// <summary>
    /// Releases the managed and unmanaged resources used by the <see cref="MediaElement"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="MediaElement"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            MediaManager?.Dispose();
            MediaManager = null;
            PlatformDispose();
        }
    }
    partial void PlatformDispose();

    partial void PlatformDispose()
    {
        playerViewController?.Dispose();
        playerViewController = null;
    }
}