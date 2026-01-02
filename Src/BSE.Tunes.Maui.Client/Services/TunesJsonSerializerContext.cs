using BSE.Tunes.Maui.Client.Models.IdentityModel;
using System.Text.Json.Serialization;

namespace BSE.Tunes.Maui.Client.Services
{
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString)]
    [JsonSerializable(typeof(UserToken))]

    internal partial class TunesJsonSerializerContext : JsonSerializerContext
    {
    }
}
