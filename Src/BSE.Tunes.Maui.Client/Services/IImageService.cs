namespace BSE.Tunes.Maui.Client.Services
{
    public interface IImageService
    {
        string GetBitmapSource(Guid albumId, bool asThumbnail = false);
    }
}
