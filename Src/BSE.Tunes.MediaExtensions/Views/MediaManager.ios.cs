using AVFoundation;
using AVKit;
using BSE.Tunes.MediaExtensions.Extensions;
using BSE.Tunes.MediaExtensions.Primitives;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using Foundation;
using UIKit;

namespace BSE.Tunes.MediaExtensions.Views
{
    public partial class MediaManager : CommunityToolkit.Maui.Core.Views.MediaManager
    {
        Metadata metaDataExt;
        
        public MediaManager(IMauiContext context, IMediaElement mediaElement, IDispatcher dispatcher) : base(context, mediaElement, dispatcher)
        {

        }

        public (AVPlayer Player, AVPlayerViewController PlayerViewController) CreateCustomPlatformView()
        {
            Player = new();
            PlayerViewController = new()
            {
                Player = Player
            };

            UIApplication.SharedApplication.BeginReceivingRemoteControlEvents();

            var avSession = AVAudioSession.SharedInstance();
            avSession.SetCategory(AVAudioSessionCategory.Playback);
            avSession.SetActive(true);

            AddStatusObservers();


            return (Player, PlayerViewController);
        }

        /// <summary>
        /// The default <see cref="NSKeyValueObservingOptions"/> flags used in the iOS and macOS observers.
        /// </summary>
        protected NSKeyValueObservingOptions ValueObserverOptions => NSKeyValueObservingOptions.Initial | NSKeyValueObservingOptions.New;


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
            //MediaElement.CurrentStateChanged(MediaElementState.Opening);

            AVAsset? asset = null;
            if (Player is null)
            {
                return;
            }
            metaDataExt ??= new(Player);
            Metadata.ClearNowPlaying();
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

            metaDataExt.SetMetadata(PlayerItem, MediaElement);
            CurrentItemErrorObserver?.Dispose();

            Player.ReplaceCurrentItemWithPlayerItem(PlayerItem);

            if (PlayerItem is not null && PlayerItem.Error is null)
            {
                MediaElement.MediaOpened();

                //(MediaElement.MediaWidth, MediaElement.MediaHeight) = GetVideoDimensions(PlayerItem);

                if (MediaElement.ShouldAutoPlay)
                {
                    Player.Play();
                }
                //SetPoster();
            }








            //base.PlatformUpdateSource();
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

    }
}
