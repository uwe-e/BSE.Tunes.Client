using BSE.Tunes.Maui.Client.Models.IdentityModel;

namespace BSE.Tunes.Maui.Client.Services
{
    public interface IAuthenticationService
    {
        TokenResponse TokenResponse
        {
            get;
        }
        Task<bool> LoginAsync(string userName, string password);
        Task<TokenResponse> RequestRefreshTokenAsync(string refreshToken);
    }
}
