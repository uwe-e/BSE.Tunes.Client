using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    public class RootPageViewModel
    {
        private INavigationService _navigationService { get; }

        public RootPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommandExecuted);
        }

        public DelegateCommand<string> NavigateCommand { get; }

        private void OnNavigateCommandExecuted(string uri)
        {
            _navigationService.NavigateAsync(uri)
                .OnNavigationError(ex => Console.WriteLine(ex));
        }
    }
}
