using BSE.Tunes.Shared.Services.Enums;

namespace BSE.Tunes.WinUI.Client.Messages;

/// <summary>
/// Message sent when cache has been modified
/// </summary>
public sealed class CacheChangedMessage
{
    public CacheChangeMode Mode { get; }

    public CacheChangedMessage(CacheChangeMode mode)
    {
        Mode = mode;
    }
}