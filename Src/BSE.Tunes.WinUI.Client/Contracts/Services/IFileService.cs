
namespace BSE.Tunes.WinUI.Client.Contracts.Services;

public interface IFileService
{
    void Delete(string folderPath, string fileName);
    Task<T?> ReadAsync<T>(string folderPath, string fileName);
    Task SaveAsync<T>(string folderPath, string fileName, T content);
}
