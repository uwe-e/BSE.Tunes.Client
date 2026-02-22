namespace BSE.Tunes.Shared.Services
{
    public interface IAuthenticationService
    {
        Task<bool> SignInAsync(string userName, string password);
        Task<string> GetAuthTokenAsync();
    }
}
