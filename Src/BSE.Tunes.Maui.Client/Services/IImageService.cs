﻿namespace BSE.Tunes.Maui.Client.Services
{
    public interface IImageService
    {
        string GetBitmapSource(Guid albumId, bool asThumbnail = false);
        Task<string> GetStitchedBitmapSourceAsync(int playlistId, int width = 300, bool asThumbnail = false);
        Task RemoveStitchedBitmaps(int playlistId);
    }
}
