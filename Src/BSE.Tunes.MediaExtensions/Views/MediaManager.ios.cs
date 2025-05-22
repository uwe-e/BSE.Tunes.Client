using AVFoundation;
using AVKit;
using BSE.Tunes.MediaExtensions.Extensions;
using BSE.Tunes.MediaExtensions.Primitives;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using CoreFoundation;
using Foundation;
using Microsoft.Extensions.Logging;
using UIKit;

namespace BSE.Tunes.MediaExtensions.Views
{
    public partial class MediaManager(IMauiContext context, IMediaElement mediaElement, IDispatcher dispatcher) : CommunityToolkit.Maui.Core.Views.MediaManager(context, mediaElement, dispatcher)
    {
        Metadata? metaData;

        /// <summary>
        /// The default <see cref="NSKeyValueObservingOptions"/> flags used in the iOS and macOS observers.
        /// </summary>
        protected NSKeyValueObservingOptions ValueObserverOptions => NSKeyValueObservingOptions.Initial | NSKeyValueObservingOptions.New;

        /// <summary>
        /// Observer that tracks when an error has occurred in the playback of the current item.
        /// </summary>
        protected new IDisposable? CurrentItemErrorObserver { get; set; }

        /// <summary>
        /// Observer that tracks when an error has occurred with media playback.
        /// </summary>
        protected new NSObject? ErrorObserver { get; set; }

        /// <summary>
        /// Observer that tracks when the media has failed to play to the end.
        /// </summary>
        protected new NSObject? ItemFailedToPlayToEndTimeObserver { get; set; }

        /// <summary>
        /// Observer that tracks when the playback of media has stalled.
        /// </summary>
        protected new NSObject? PlaybackStalledObserver { get; set; }

        /// <summary>
        /// Observer that tracks when the media has played to the end.
        /// </summary>
        protected new NSObject? PlayedToEndObserver { get; set; }

        /// <summary>
        /// Observer that tracks the status of the media.
        /// </summary>
        protected new IDisposable? StatusObserver { get; set; }

        /// <summary>
        /// Observer that tracks the time control status of the media.
        /// </summary>
        protected new IDisposable? TimeControlStatusObserver { get; set; }

        public (AVPlayer Player, AVPlayerViewController PlayerViewController) CreateCustomPlatformView()
        {
            Player = new();
            PlayerViewController = new()
            {
                Player = Player
            };

            // Pre-initialize Volume and Muted properties to the player object
            Player.Muted = MediaElement.ShouldMute;
            var volumeDiff = Math.Abs(Player.Volume - MediaElement.Volume);
            if (volumeDiff > 0.01)
            {
                Player.Volume = (float)MediaElement.Volume;
            }

            UIApplication.SharedApplication.BeginReceivingRemoteControlEvents();

            PlayerViewController.UpdatesNowPlayingInfoCenter = false;

            var avSession = AVAudioSession.SharedInstance();
            avSession.SetCategory(AVAudioSessionCategory.Playback);
            avSession.SetActive(true);

            AddStatusObservers();
            AddPlayedToEndObserver();
            AddErrorObservers();

            return (Player, PlayerViewController);
        }

        protected override void PlatformPlay()
        {
            base.PlatformPlay();
        }

        protected override void PlatformPause()
        {
            base.PlatformPause();
        }

        protected override void PlatformUpdateSource()
        {
            MediaElement.InvokeCurrentStateChanged(MediaElementState.Opening);

            AVAsset? asset = null;
            if (Player is null)
            {
                return;
            }
            //metaData ??= new(Player);
            //Metadata.ClearNowPlaying();
            
            PlayerViewController?.ContentOverlayView?.Subviews?.FirstOrDefault()?.RemoveFromSuperview();

            if (MediaElement.Source is UriMediaSource uriMediaSource)
            {
                var uri = uriMediaSource.Uri;
                if (!string.IsNullOrWhiteSpace(uri?.AbsoluteUri))
                {
                    asset = AVAsset.FromUrl(new NSUrl(uri.AbsoluteUri));
                }
            }
            else if (MediaElement.Source is FileMediaSource fileMediaSource)
            {
                var uri = fileMediaSource.Path;

                if (!string.IsNullOrWhiteSpace(uri))
                {
                    asset = AVAsset.FromUrl(NSUrl.CreateFileUrl(uri));
                }
            }
            
            PlayerItem = asset is not null
                ? new AVPlayerItem(asset) : null;


            //metaData.SetMetadata(PlayerItem, MediaElement);
            CurrentItemErrorObserver?.Dispose();

            Player.ReplaceCurrentItemWithPlayerItem(PlayerItem);


            CurrentItemErrorObserver = PlayerItem?.AddObserver("error",
            ValueObserverOptions, (NSObservedChange change) =>
            {
                if (Player.CurrentItem?.Error is null)
                {
                    return;
                }

                var message = $"{Player.CurrentItem?.Error?.LocalizedDescription} - " +
                              $"{Player.CurrentItem?.Error?.LocalizedFailureReason}";

                MediaElement.MediaFailed(
                    new MediaFailedEventArgs(message));

                Logger.LogError("{LogMessage}", message);
            });


            if (PlayerItem is not null && PlayerItem.Error is null)
            {
                MediaElement.MediaOpened();

                if (MediaElement.ShouldAutoPlay)
                {
                    Player.Play();
                }
            }
            else if (PlayerItem is null)
            {
                MediaElement.InvokeCurrentStateChanged(MediaElementState.None);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Player is not null)
                {
                    Player?.Pause();
                    Player?.ReplaceCurrentItemWithPlayerItem(null);
                    Player?.Dispose();
                    Player = null;
                }
                if (PlayerViewController is not null)
                {
                    PlayerViewController?.Dispose();
                    PlayerViewController = null;
                }
                PlayedToEndObserver?.Dispose();
                StatusObserver?.Dispose();
                TimeControlStatusObserver?.Dispose();
            }
            base.Dispose(disposing);
        }
        private void AddStatusObservers()
        {
            if (Player is null)
            {
                return;
            }

            //MutedObserver = Player.AddObserver("muted", ValueObserverOptions, MutedChanged);
            //VolumeObserver = Player.AddObserver("volume", ValueObserverOptions, VolumeChanged);
            StatusObserver = Player.AddObserver("status", ValueObserverOptions, StatusChanged);
            TimeControlStatusObserver = Player.AddObserver("timeControlStatus", ValueObserverOptions, TimeControlStatusChanged);

        }

        void AddErrorObservers()
        {
            DestroyErrorObservers();

            ItemFailedToPlayToEndTimeObserver = AVPlayerItem.Notifications.ObserveItemFailedToPlayToEndTime(ErrorOccurred);
            PlaybackStalledObserver = AVPlayerItem.Notifications.ObservePlaybackStalled(ErrorOccurred);
            ErrorObserver = AVPlayerItem.Notifications.ObserveNewErrorLogEntry(ErrorOccurred);
        }

        private void AddPlayedToEndObserver()
        {
            PlayedToEndObserver = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(PlayedToEnd);
        }

        void DestroyErrorObservers()
        {
            ItemFailedToPlayToEndTimeObserver?.Dispose();
            PlaybackStalledObserver?.Dispose();
            ErrorObserver?.Dispose();
        }

        void StatusChanged(NSObservedChange obj)
        {
            if (Player is null)
            {
                return;
            }

            var newState = Player.Status switch
            {
                AVPlayerStatus.Unknown => MediaElementState.Stopped,
                AVPlayerStatus.ReadyToPlay => MediaElementState.Paused,
                AVPlayerStatus.Failed => MediaElementState.Failed,
                _ => MediaElement.CurrentState
            };

            MediaElement.InvokeCurrentStateChanged(newState);
        }

        void TimeControlStatusChanged(NSObservedChange obj)
        {
            if (Player is null || Player.Status is AVPlayerStatus.Unknown
                || Player.CurrentItem?.Error is not null)
            {
                return;
            }

            var newState = Player.TimeControlStatus switch
            {
                AVPlayerTimeControlStatus.Paused => MediaElementState.Paused,
                AVPlayerTimeControlStatus.Playing => MediaElementState.Playing,
                AVPlayerTimeControlStatus.WaitingToPlayAtSpecifiedRate => MediaElementState.Buffering,
                _ => MediaElement.CurrentState
            };

            metaData?.SetMetadata(PlayerItem, MediaElement);

            MediaElement.InvokeCurrentStateChanged(newState);
        }

        void ErrorOccurred(object? sender, NSNotificationEventArgs args)
        {
            string message;

            var error = Player?.CurrentItem?.Error;
            if (error is not null)
            {
                message = error.LocalizedDescription;

                MediaElement.MediaFailed(new MediaFailedEventArgs(message));
                Logger.LogError("{LogMessage}", message);
            }
            else
            {
                // Non-fatal error, just log
                message = args.Notification?.ToString() ??
                    "Media playback failed for an unknown reason.";

                Logger?.LogWarning("{LogMessage}", message);
            }
        }

        void PlayedToEnd(object? sender, NSNotificationEventArgs args)
        {
            if (args.Notification.Object != PlayerViewController?.Player?.CurrentItem || Player is null)
            {
                return;
            }

            try
            {
                DispatchQueue.MainQueue.DispatchAsync(MediaElement.MediaEnded);
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "{LogMessage}", $"Failed to play media to end.");
            }
        }

    }
}
