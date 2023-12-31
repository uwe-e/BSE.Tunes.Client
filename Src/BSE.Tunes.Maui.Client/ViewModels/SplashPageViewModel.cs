using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSE.Tunes.Maui.Client.ViewModels
{
    internal class SplashPageViewModel : IPageLifecycleAware
    {
        private INavigationService _navigationService { get; }

        public SplashPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void OnAppearing()
        {
            //_navigationService.CreateBuilder()
            //    .UseAbsoluteNavigation()
            //    .AddSegment<RootPageViewModel>()
            //    .Navigate();
        }

        public void OnDisappearing()
        {

        }
    }
}
