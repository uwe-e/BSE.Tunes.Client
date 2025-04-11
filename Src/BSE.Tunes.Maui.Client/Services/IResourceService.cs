namespace BSE.Tunes.Maui.Client.Services
{
    public interface IResourceService
    {
        string GetString(string key);
        string GetString(string key, string defaultValue);
    }
}
