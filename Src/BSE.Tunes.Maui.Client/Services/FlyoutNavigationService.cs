using Prism.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSE.Tunes.Maui.Client.Services
{
    public class FlyoutNavigationService : PageNavigationService, IFlyoutNavigationService
    {
        public FlyoutNavigationService(
            IContainerProvider container,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            IPageAccessor pageAccessor) : base(container, windowManager, eventAggregator, pageAccessor)
        {
        }

        public async Task<INavigationResult> CloseFlyoutAsync()
        {
            //throw new NotImplementedException();
            //NavigationParameters parameters = new NavigationParameters
            //{
            //    mo
            //}
            return await GoBackAsync(null);
        }

        public async Task<INavigationResult> ShowFlyoutAsync(string name, INavigationParameters parameters)
        {
            return await NavigateAsync(new System.Uri(name), parameters);
        }
    }
}
