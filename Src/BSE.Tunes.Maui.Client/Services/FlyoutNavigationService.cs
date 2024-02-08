using BSE.Tunes.Maui.Client.Controls;
using Prism.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BSE.Tunes.Maui.Client.Services
{
    public class FlyoutNavigationService : PageNavigationService, IFlyoutNavigationService
    {
        private readonly IContainerProvider _containerProvider;
        private readonly IEnumerable<ViewRegistration> _registrations;
        private BottomFlyoutPage? _flyoutPage;

        public FlyoutNavigationService(
            IContainerProvider container,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            IContainerProvider containerProvider,
            IEnumerable<ViewRegistration> registrations,
            IPageAccessor pageAccessor) : base(container, windowManager, eventAggregator, pageAccessor)
        {
            _containerProvider = containerProvider;
            _registrations = registrations;
        }

        public async Task<INavigationResult> CloseFlyoutAsync()
        {
            if (_flyoutPage != null)
            {
                await _flyoutPage.DisappearingAnimation();
            }

            var page = GetCurrentPage();


            //var poppedPage1 = await DoPop(page.Navigation, false, false);
            //var pagesToDestroy = page.Navigation.NavigationStack.ToList(); // get all pages to destroy

            var poppedPage = await DoPop(page.Navigation, true, false);
            if (poppedPage != null)
            {
                MvvmHelpers.DestroyPage(poppedPage);
            }
            //_flyoutPage = null;
            //return await GoBackToAsync(nameof(page), null);
            return new NavigationResult();
        }

        public async Task<INavigationResult> ShowFlyoutAsync(string name, INavigationParameters parameters)
        {
            try
            {


                //ViewRegistration registration = _registrations.LastOrDefault(x => x.Name == name);
                //if (registration != null)
                {
                    //var page = Activator.CreateInstance(registration.View);

                    var page = CreatePageFromSegment(name);
                    if (page is BottomFlyoutPage flyoutPage)
                    {
                        _flyoutPage = flyoutPage;

                        var useModalNavigation = true;
                        var animated = false;
                        var uri = UriParsingHelper.Parse(name);
                        var navigationSegments = UriParsingHelper.GetUriSegments(uri);

                        var nextSegment = navigationSegments.Dequeue();

                        await ProcessNavigation(flyoutPage, navigationSegments, parameters, useModalNavigation, animated);

                        var currentPage = GetCurrentPage();

                        //await DoNavigateAction(page, nextSegment, flyoutPage, parameters, async () =>
                        //{
                        //    await DoPush(GetCurrentPage(), flyoutPage, useModalNavigation, animated);
                        //});
                        //var pagesToDestroy = currentPage.Navigation.NavigationStack.ToList();


                        await DoNavigateAction(currentPage, nextSegment, flyoutPage, parameters, async () =>
                        {
                            await DoPush(currentPage, flyoutPage, useModalNavigation, animated);
                        });

                        await flyoutPage.AppearingAnimation();
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"{ex.Message} {ex.InnerException?.Message}");
            }
            return new NavigationResult(true);


            //return await NavigateAsync(new System.Uri(name), parameters);
        }


    }
}
