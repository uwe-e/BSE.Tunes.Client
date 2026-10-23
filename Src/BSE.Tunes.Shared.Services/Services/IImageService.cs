namespace BSE.Tunes.Shared.Services
{
    public interface IImageService
    {
        string GetBitmapSource(Guid albumId, bool asThumbnail = false);
        Task<string> GetComposedBitmapSourceAsync(int playlistId, IList<string> albumCoverIds, int width = 300, bool asThumbnail = false);
        Task RemoveComposedBitmaps(int playlistId);
    }
}
