using BSE.Tunes.Maui.Client.Controls;
using Prism.Common;
using Prism.Events;
using Prism.Navigation;

namespace BSE.Tunes.Maui.Client.Services
{
    public class FlyoutNavigationService : PageNavigationService, IFlyoutNavigationService
    {
        private BottomFlyoutPage? _flyoutPage;

        public FlyoutNavigationService(
            IContainerProvider container,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            IPageAccessor pageAccessor) : base(container, windowManager, eventAggregator, pageAccessor)
        {
        }

        public async Task<INavigationResult> CloseFlyoutAsync()
        {
            if (_flyoutPage != null)
            {
                await _flyoutPage.DisappearingAnimation();
            }

            var page = GetCurrentPage();

            var poppedPage = await DoPop(page.Navigation, true, false);
            if (poppedPage != null)
            {
                MvvmHelpers.DestroyPage(poppedPage);
            }

            return new NavigationResult();
        }

        public async Task<INavigationResult> ShowFlyoutAsync(string name, INavigationParameters parameters)
        {
            try
            {
                var uri = UriParsingHelper.Parse(name);
                var navigationSegments = UriParsingHelper.GetUriSegments(uri);
                var nextSegment = navigationSegments.Dequeue();

                bool? useModalNavigation = null;
                if (parameters.TryGetValue<bool>(KnownNavigationParameters.UseModalNavigation, out var parameterModal))
                {
                    useModalNavigation = parameterModal;
                }

                bool? pageAnimation = null;
                if (parameters.TryGetValue<bool>(KnownNavigationParameters.Animated, out var parameterAnimation))
                {
                    pageAnimation = parameterAnimation;
                }

                var page = CreatePageFromSegment(nextSegment);
                if (page is BottomFlyoutPage flyoutPage)
                {
                    _flyoutPage = flyoutPage;
                    var currentPage = GetCurrentPage();
                    await ProcessNavigation(page, navigationSegments, parameters, useModalNavigation, pageAnimation);

                    await DoNavigateAction(currentPage, nextSegment, page, parameters, async () =>
                    {
                        await DoPush(currentPage, page, useModalNavigation, pageAnimation);
                    });

                    await flyoutPage.AppearingAnimation();
                }
                return new NavigationResult();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Trace.WriteLine($"{exception.Message} {exception.InnerException?.Message}");
                return new NavigationResult(exception);
            }
        }
    }
}
