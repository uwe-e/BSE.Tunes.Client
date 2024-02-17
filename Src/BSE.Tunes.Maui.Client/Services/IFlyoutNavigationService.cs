using Prism.Navigation;

namespace BSE.Tunes.Maui.Client.Services
{
    public interface IFlyoutNavigationService : INavigationService
    {
        Task<INavigationResult> ShowFlyoutAsync(string name, INavigationParameters parameters);

        Task<INavigationResult> CloseFlyoutAsync();
    }
}
