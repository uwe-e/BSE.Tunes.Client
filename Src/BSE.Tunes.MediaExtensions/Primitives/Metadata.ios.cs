using AVFoundation;
using CommunityToolkit.Maui.Core;
using Foundation;
using MediaPlayer;
using UIKit;

namespace BSE.Tunes.MediaExtensions.Primitives;

public class Metadata
{
    static readonly UIImage defaultUIImage = new();

    static readonly MPNowPlayingInfo nowPlayingInfoDefault = new()
    {
        AlbumTitle = string.Empty,
        Title = string.Empty,
        Artist = string.Empty,
        PlaybackDuration = 0,
        IsLiveStream = false,
        PlaybackRate = 0,
        ElapsedPlaybackTime = 0,
        Artwork = new(boundsSize: new(0, 0), requestHandler: _ => defaultUIImage)
    };

    private readonly AVPlayer _player;

    public Metadata(AVPlayer player)
    {
        _player = player;
        //MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlayingInfoDefault;

        //var commandCenter = MPRemoteCommandCenter.Shared;

        //commandCenter.TogglePlayPauseCommand.Enabled = true;
        //commandCenter.TogglePlayPauseCommand.AddTarget(ToggleCommand);

        //commandCenter.PlayCommand.Enabled = true;
        //commandCenter.PlayCommand.AddTarget(PlayCommand);

        //commandCenter.PauseCommand.Enabled = true;
        //commandCenter.PauseCommand.AddTarget(PauseCommand);
    }

    /// <summary>
	/// The metadata for the currently playing media.
	/// </summary>
	//public MPNowPlayingInfo NowPlayingInfo { get; } = new();

    public static void ClearNowPlaying() => MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlayingInfoDefault;

    public void SetMetadata(AVPlayerItem? playerItem, IMediaElement? mediaElement)
    {
        if (mediaElement is null)
        {
            Metadata.ClearNowPlaying();
            return;
        }

        //NowPlayingInfo.Title = mediaElement.MetadataTitle;
        //NowPlayingInfo.Artist = mediaElement.MetadataArtist;
        //NowPlayingInfo.PlaybackDuration = playerItem?.Duration.Seconds ?? 0;
        //NowPlayingInfo.IsLiveStream = false;
        //NowPlayingInfo.PlaybackRate = mediaElement.Speed;
        //NowPlayingInfo.ElapsedPlaybackTime = playerItem?.CurrentTime.Seconds ?? 0;
        //NowPlayingInfo.Artwork = new(boundsSize: new(320, 240), requestHandler: _ => GetImage(mediaElement.MetadataArtworkUrl));
        //MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = NowPlayingInfo;
        MPNowPlayingInfo nowPlayingInfo = new MPNowPlayingInfo();
        nowPlayingInfo.Title = mediaElement.MetadataTitle;
        nowPlayingInfo.Artist = mediaElement.MetadataArtist;
        ////nowPlayingInfo.AlbumTitle = mediaElement.m.MetadataAlbum;

        MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlayingInfo;

        //var commandCenter = MPRemoteCommandCenter.Shared;

        //commandCenter.TogglePlayPauseCommand.Enabled = true;
        //commandCenter.TogglePlayPauseCommand.AddTarget(ToggleCommand);

        //commandCenter.PlayCommand.Enabled = true;
        //commandCenter.PlayCommand.AddTarget(PlayCommand);

        //commandCenter.PauseCommand.Enabled = true;
        //commandCenter.PauseCommand.AddTarget(PauseCommand);
    }

    static UIImage GetImage(string imageUri)
    {
        try
        {
            if (imageUri.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                return UIImage.LoadFromData(NSData.FromUrl(new NSUrl(imageUri))) ?? defaultUIImage;
            }
            return defaultUIImage;
        }
        catch
        {
            return defaultUIImage;
        }
    }

    MPRemoteCommandHandlerStatus PlayCommand(MPRemoteCommandEvent? commandEvent)
    {
        if (commandEvent is null)
        {
            return MPRemoteCommandHandlerStatus.CommandFailed;
        }

        _player?.Play();
        return MPRemoteCommandHandlerStatus.Success;
    }

    MPRemoteCommandHandlerStatus PauseCommand(MPRemoteCommandEvent? commandEvent)
    {
        if (commandEvent is null)
        {
            return MPRemoteCommandHandlerStatus.CommandFailed;
        }

        _player?.Pause();
        return MPRemoteCommandHandlerStatus.Success;
    }

    MPRemoteCommandHandlerStatus ToggleCommand(MPRemoteCommandEvent? commandEvent)
    {
        if (commandEvent is not null)
        {
            return MPRemoteCommandHandlerStatus.CommandFailed;
        }

        if (_player?.Rate is 0)
        {
            _player?.Play();
        }
        else
        {
            _player?.Pause();
        }

        return MPRemoteCommandHandlerStatus.Success;
    }

}
