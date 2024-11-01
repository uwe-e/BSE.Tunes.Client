
using BSE.Tunes.Maui.Client.Services;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class PlayerBaseViewModel(
        INavigationService navigationService,
        IEventAggregator eventAggregator,
        IMediaManager mediaManager) : ViewModelBase(navigationService)
    {
        private readonly IEventAggregator _eventAggregator = eventAggregator;
        private readonly IMediaManager _mediaManager = mediaManager;
        private DelegateCommand _playCommand;

        public DelegateCommand PlayCommand => _playCommand ?? new DelegateCommand(Play);

        private void Play()
        {

        }
    }
}
