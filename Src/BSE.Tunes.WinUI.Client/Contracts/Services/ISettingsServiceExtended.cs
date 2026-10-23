namespace BSE.Tunes.WinUI.Client.Contracts.Services;

/// <summary>
/// Extended settings service for WinUI-specific functionality.
/// Adds events and methods for monitoring critical settings changes.
/// </summary>
public interface ISettingsServiceExtended : ISettingsService
{
    /// <summary>
    /// Raised when the service endpoint is removed.
    /// Note: User account is automatically cleared when this happens (cascading).
    /// </summary>
    event EventHandler? ServiceEndpointRemoved;

    /// <summary>
    /// Raised when only the user account is deleted (endpoint remains).
    /// </summary>
    event EventHandler? UserAccountDeleted;

    /// <summary>
    /// Clears service endpoint and cascades to clear user account.
    /// Raises ServiceEndpointRemoved event.
    /// </summary>
    Task ClearServiceEndpointAsync();

    /// <summary>
    /// Clears only the user account (endpoint remains).
    /// Raises UserAccountDeleted event.
    /// </summary>
    Task ClearUserAccountAsync();
}