using BSE.Tunes.Shared.Services.Abstractions;
using BSE.Tunes.Shared.Services.Models;
using BSE.Tunes.Shared.Services.Models.IdentityModel;
using System.Text.Json;
using Windows.Security.Credentials;

namespace BSE.Tunes.WinUI.Client.Services
{
    public class SettingsService : ISettingsService
    {
        private const string UserAccessToken = "user_token";
        private const string ResourceName = "BSE.Tunes.WinUI.Client";
        
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

        public Task<UserToken?> GetUserTokenAsync()
        {
            try
            {
                var vault = new PasswordVault();
                var credential = vault.Retrieve(ResourceName, UserAccessToken);
                credential.RetrievePassword();
                
                return Task.FromResult(JsonSerializer.Deserialize<UserToken>(credential.Password));
            }
            catch (Exception)
            {
                // Credential not found
                return Task.FromResult<UserToken?>(null);
            }
        }

        public Task SetUserTokenAsync(UserToken userToken)
        {
            try
            {
                var vault = new PasswordVault();
                
                // Remove existing credential if present
                try
                {
                    var existing = vault.Retrieve(ResourceName, UserAccessToken);
                    vault.Remove(existing);
                }
                catch { /* Not found, that's fine */ }
                
                // Add new credential if not null
                if (userToken is not null)
                {
                    var credential = new PasswordCredential(
                        ResourceName, 
                        UserAccessToken, 
                        JsonSerializer.Serialize(userToken));
                    vault.Add(credential);
                }
            }
            catch (Exception)
            {
                // Handle storage failure
                throw;
            }
            
            return Task.CompletedTask;
        }
    }
}