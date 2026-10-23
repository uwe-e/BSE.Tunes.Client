using BSE.Tunes.Shared.Services.Models;
using BSE.Tunes.Shared.Services.Models.IdentityModel;

namespace BSE.Tunes.Shared.Services.Abstractions
{
    public interface ISettingsService
    {
        // Required by DataService and AuthenticationService
        string ServiceEndPoint { get; set; }
        
        // Required by AuthenticationService
        User User { get; set; }
        //string Token { get; set; }
        
        Task<UserToken?> GetUserTokenAsync();
        Task SetUserTokenAsync(UserToken userToken);
    }
}