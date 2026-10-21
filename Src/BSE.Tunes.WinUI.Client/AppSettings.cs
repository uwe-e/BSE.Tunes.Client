using System.Text.Json;
using Windows.Storage;

namespace BSE.Tunes.WinUI.Client;

public static class AppSettings
{
    private static ApplicationDataContainer LocalSettings => 
        ApplicationData.Current.LocalSettings;

    public static string ServiceEndPoint
    {
        get => LocalSettings.Values[nameof(ServiceEndPoint)] as string;
        set => LocalSettings.Values[nameof(ServiceEndPoint)] = value;
    }
    public static User User
    {
        get
        {
            var json = LocalSettings.Values[nameof(User)] as string;
            return json != null ? JsonSerializer.Deserialize<User>(json) : default;
        }
        set
        {
            var json = JsonSerializer.Serialize(value);
            LocalSettings.Values[nameof(User)] = json;
        }
    }
}