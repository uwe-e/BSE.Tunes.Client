using BSE.Tunes.Shared.Services.Services;

namespace BSE.Tunes.Maui.Client.Services
{
    public class FileSystemProvider : IFileSystemProvider
    {
        public string CacheDirectory => FileSystem.CacheDirectory;
    }
}
