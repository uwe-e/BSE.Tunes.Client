namespace BSE.Tunes.Shared.Services.Services
{
    public interface IFileSystemProvider
    {
        /// <summary>
        /// Get the directory path where the application can store cached files,
        /// such as images and audio files. This directory should be used for temporary storage of files
        /// that can be recreated or downloaded again if needed.
        /// The implementation should ensure that the cache directory is properly managed,
        /// including handling cleanup of old or unused files to prevent excessive storage usage.
        /// </summary>
        string CacheDirectory { get; }
    }
}
