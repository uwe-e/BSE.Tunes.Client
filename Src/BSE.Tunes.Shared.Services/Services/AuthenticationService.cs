using BSE.Tunes.Shared.Services.Extensions;
using BSE.Tunes.Shared.Services.Models.IdentityModel;
using System.Net.Http.Json;

namespace BSE.Tunes.Shared.Services
{
    /// <summary>
    /// Provides authentication services for signing in users and retrieving authentication tokens using user
    /// credentials and token refresh mechanisms.
    /// </summary>
    /// <param name="settingsService">The settings service used to manage user information and authentication
    /// tokens. Cannot be null.</param>
    public class AuthenticationService(Abstractions.ISettingsService settingsService) : IAuthenticationService
    {
        private readonly Abstractions.ISettingsService _settingsService = settingsService;

        /// <summary>
        /// Attempts to sign in a user asynchronously using the specified user name and password.
        /// </summary>
        /// <param name="userName">The user name to use for authentication. Cannot be null or empty.</param>
        /// <param name="password">The password associated with the specified user name. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result is <see langword="true"/> if the sign-in
        /// is successful; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the user token cannot be retrieved
        /// during the sign-in process.</exception>
        public async Task<bool> SignInAsync(string userName, string password)
        {
            var fields = new Dictionary<string, string>
            {
                { OAuth2Constants.UserName, userName },
                { OAuth2Constants.Password, password },
                { OAuth2Constants.GrantType, OAuth2Constants.GrantTypes.Password }
            };

            try
            {
                var builder = new UriBuilder(this._settingsService.ServiceEndPoint);
                builder.AppendToPath("login");

                var userToken = await RequestAsync(builder.Uri, fields).ConfigureAwait(false);
                if (userToken is null)
                {
                    throw new InvalidOperationException("Failed to retrieve user token.");
                }
                _settingsService.User = new Models.User
                {
                    UserName = userName
                };
                await _settingsService
                    .SetUserTokenAsync(
                    new UserToken
                    {
                        AccessToken = userToken.AccessToken,
                        RefreshToken = userToken.RefreshToken,
                        ExpiresAt = userToken.ExpiresAt
                    }).ConfigureAwait(false);
                
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Gets the authentication token for the currently authenticated user asynchronously.
        /// If the token is close to expiration, it attempts to refresh it using the refresh token.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result is the authentication token as a string.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user is not authenticated.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the user token cannot be retrieved during
        /// the refresh process.</exception>
        public async Task<string> GetAuthTokenAsync()
        {
            var userToken = await _settingsService.GetUserTokenAsync().ConfigureAwait(false);

            if (userToken is null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            if (userToken.ExpiresAt.Subtract(DateTimeOffset.Now).TotalMinutes > 5)
            {
                return userToken.AccessToken;
            }

            var userId = userToken.AccessToken.ParseJwtToken().Subject ?? string.Empty;

            var builder = new UriBuilder(this._settingsService.ServiceEndPoint);
            builder.AppendToPath("refresh");

            var fields = new Dictionary<string, string>
            {
                { OAuth2Constants.RefreshToken, userToken.RefreshToken },
                { OAuth2Constants.UserId, userId }
            };

            try
            {
                userToken = await RequestAsync(builder.Uri, fields).ConfigureAwait(false);
                if (userToken is null)
                {
                    throw new InvalidOperationException("Failed to retrieve user token.");
                }
                
                await _settingsService
                    .SetUserTokenAsync(
                    new UserToken
                    {
                        AccessToken = userToken.AccessToken,
                        RefreshToken = userToken.RefreshToken,
                        ExpiresAt = userToken.ExpiresAt
                    }).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }

            return userToken.AccessToken;
        }

        private async Task<UserToken?> RequestAsync(Uri uri, Dictionary<string, string> fields)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.PostAsJsonAsync(uri, fields);

            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<UserToken>();

        }
    }
}
