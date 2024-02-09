using System.Globalization;

namespace BSE.Tunes.Maui.Client.Services
{
    public class ResourceService : IResourceService
    {
        public string? GetString(string key) => AppSettings.ResourceManager.Value.GetString(key, CultureInfo.CurrentCulture) ?? "Hallo";

        public string GetString(string key, string defaultValue)
        {
            string? translation = AppSettings.ResourceManager.Value.GetString(key, CultureInfo.CurrentCulture);
            if (string.IsNullOrEmpty(translation))
            {
                translation = defaultValue;
            }
            return translation;
        }
    }
}
