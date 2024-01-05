using BSE.Tunes.Maui.Client.Extensions;
using BSE.Tunes.Maui.Client.Models.IdentityModel;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BSE.Tunes.Maui.Client.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ISettingsService settingsService;
        public TokenResponse TokenResponse
        {
            get;
            private set;
        }

        public AuthenticationService(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public async Task<bool> LoginAsync(string userName, string password)
        {
            var succeeded = false;
            var fields = new Dictionary<string, string>
            {
                { OAuth2Constants.UserName, userName },
                { OAuth2Constants.Password, password },
                { OAuth2Constants.GrantType, OAuth2Constants.GrantTypes.Password }
            };

            try
            {
                this.TokenResponse = await RequestAsync(fields);
                this.settingsService.User = new Models.User
                {
                    UserName = userName,
                    Token = this.TokenResponse.RefreshToken
                };
                succeeded = true;
            }
            catch (Exception)
            {
                throw;
            }

            return succeeded;
        }

        public async Task<TokenResponse> RequestRefreshTokenAsync(string refreshToken)
        {
            var fields = new Dictionary<string, string>
            {
                { OAuth2Constants.GrantType, OAuth2Constants.GrantTypes.RefreshToken },
                { OAuth2Constants.RefreshToken, refreshToken }
            };

            try
            {
                this.TokenResponse = await RequestAsync(fields);
                this.settingsService.Token = TokenResponse.RefreshToken;
            }
            catch (Exception)
            {
                throw;
            }
            
            return this.TokenResponse;
        }

        private async Task<TokenResponse> RequestAsync(Dictionary<string, string> fields)
        {
            TokenResponse tokenResponse = null;
            var builder = new UriBuilder(this.settingsService.ServiceEndPoint);
            builder.AppendToPath("token");

            var request = new HttpRequestMessage(HttpMethod.Post, builder.Uri)
            {
                Content = new FormUrlEncodedContent(fields)
            };
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", EncodeCredential("BSEtunes", "f2186598-35f4-496d-9de0-41157a27642f"));

            var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest)
            {
                var content = await response.Content.ReadAsStringAsync();
                tokenResponse = new TokenResponse(content);
            }
            else
            {
                tokenResponse = new TokenResponse(response.StatusCode, response.ReasonPhrase);
            }
            if (tokenResponse.IsError)
            {
                throw new UnauthorizedAccessException(tokenResponse.Error);
            }
            return tokenResponse;
        }

        private static string EncodeCredential(string userName, string password)
        {
            Encoding encoding = Encoding.UTF8;
            string credential = String.Format("{0}:{1}", userName, password);

            return Convert.ToBase64String(encoding.GetBytes(credential));
        }
    }
}
