using BSE.Tunes.Maui.Client.Models;
#nullable enable
using BSE.Tunes.Maui.Client.Models.IdentityModel;

namespace BSE.Tunes.Maui.Client.Services
{
    public interface ISettingsService
    {
        User User { get; set; }

        string ServiceEndPoint { get; set; }

        string Token { get; set; }

        Task<UserToken?> GetUserTokenAsync();

        Task SetUserTokenAsync(UserToken? userToken);
    }
}
