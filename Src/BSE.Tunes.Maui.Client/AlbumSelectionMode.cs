namespace BSE.Tunes.Maui.Client
{
    public enum AlbumSelectionMode
    {
        None = 0,
        Preparation = 1,  // From NowPlayingPage (needs to close dialog first)
        Direct = 2        // From HomePage/SettingsPage (direct navigation)
    }
}