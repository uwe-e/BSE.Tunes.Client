using BSE.Tunes.Shared.Services.Services;

namespace BSE.Tunes.WinUI.Client.Contracts.Services
{
    public class FileSystemProvider : IFileSystemProvider
    {
        public string CacheDirectory => Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path;
    }
}
