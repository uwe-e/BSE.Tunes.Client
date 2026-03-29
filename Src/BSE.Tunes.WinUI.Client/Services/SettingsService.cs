using BSE.Tunes.WinUI.Client.Contracts.Services;
using System.Text.Json;
using Windows.Security.Credentials;

namespace BSE.Tunes.WinUI.Client.Services
{
    public class SettingsService : ISettingsServiceExtended
    {
        private const string UserAccessToken = "user_token";
        private const string ResourceName = "BSE.Tunes.WinUI.Client";
        
        // Events from ISettingsServiceExtended
        public event EventHandler? ServiceEndpointRemoved;
        public event EventHandler? UserAccountDeleted;
        
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
                return Task.FromResult<UserToken?>(null);
            }
        }

        public Task SetUserTokenAsync(UserToken userToken)
        {
            try
            {
                var vault = new PasswordVault();
                
                try
                {
                    var existing = vault.Retrieve(ResourceName, UserAccessToken);
                    vault.Remove(existing);
                }
                catch { /* Not found, that's fine */ }
                
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
                throw;
            }
            
            return Task.CompletedTask;
        }

        // ISettingsServiceExtended implementations
        public async Task ClearServiceEndpointAsync()
        {
            // CASCADE: When endpoint is removed, user account is no longer valid
            // Clear user account FIRST (without raising event - we'll raise endpoint event)
            await ClearUserDataSilentlyAsync();
            
            // Then clear endpoint
            ServiceEndPoint = string.Empty;
            
            // Raise ONLY the endpoint removed event
            // The event handler will navigate to EndpointConfigurationPage
            ServiceEndpointRemoved?.Invoke(this, EventArgs.Empty);
        }

        public async Task ClearUserAccountAsync()
        {
            // Clear only user account (endpoint remains)
            await ClearUserDataSilentlyAsync();
            
            // Raise user account deleted event
            // The event handler will navigate to LoginPage
            UserAccountDeleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Clears user data without raising events (for cascading operations)
        /// </summary>
        private async Task ClearUserDataSilentlyAsync()
        {
            User = null;
            await SetUserTokenAsync(null);
        }
    }
}