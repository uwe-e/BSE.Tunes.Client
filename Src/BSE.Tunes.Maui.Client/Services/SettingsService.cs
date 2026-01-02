using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Models.IdentityModel;
using System.Text.Json;

namespace BSE.Tunes.Maui.Client.Services
{
    public class SettingsService : ISettingsService
    {
        private const string UserAccessToken = "user_token";

        public string ServiceEndPoint
        {
            get => AppSettings.ServiceEndPoint;
            set => AppSettings.ServiceEndPoint = value;
        }

        public User User
        {
            get => AppSettings.User;
            set => AppSettings.User = value;
        }

        public string Token
        {
            get => User?.Token;
            set
            {
                var user = User;
                if (user != null)
                {
                    user.Token = value;
                    User = user;
                }
            }
        }

        public async Task<UserToken> GetUserTokenAsync()
        {
            var userToken = await SecureStorage.GetAsync(UserAccessToken).ConfigureAwait(false);

            return string.IsNullOrEmpty(userToken) ? default : JsonSerializer.Deserialize(userToken, TunesJsonSerializerContext.Default.UserToken);
        }

        public async Task SetUserTokenAsync(UserToken userToken)
        {
            await SecureStorage
            .SetAsync(UserAccessToken, userToken is not null ? JsonSerializer.Serialize(userToken, TunesJsonSerializerContext.Default.UserToken) : string.Empty)
            .ConfigureAwait(false);
        }
    }
}
