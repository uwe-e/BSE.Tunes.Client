using BSE.Tunes.Maui.Client.Services;
using Prism.Navigation;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class MainPageViewModel(
        INavigationService navigationService,
        IEventAggregator eventAggregator,
        IMediaManager mediaManager) : PlayerBaseViewModel(navigationService, eventAggregator, mediaManager)
    {
        private readonly IEventAggregator eventAggregator = eventAggregator;
    }
}
