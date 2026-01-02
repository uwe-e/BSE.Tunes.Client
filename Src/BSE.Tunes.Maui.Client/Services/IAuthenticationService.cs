using BSE.Tunes.Maui.Client.Models.IdentityModel;

namespace BSE.Tunes.Maui.Client.Services
{
    public interface IAuthenticationService
    {
        TokenResponse TokenResponse
        {
            get;
        }
        Task<bool> SignInAsync(string userName, string password);
        Task<string> GetAuthTokenAsync();

        Task<TokenResponse> RequestRefreshTokenAsync(string refreshToken);
    }
}
