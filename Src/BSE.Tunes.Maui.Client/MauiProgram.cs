using BSE.Tunes.Maui.Client.Controls;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.ViewModels;
using BSE.Tunes.Maui.Client.Views;
using Microsoft.Extensions.Logging;

namespace BSE.Tunes.Maui.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UsePrism(prism =>
                {
                    //prism.OnAppStart(async navigationService =>
                    //{
                    //    var result = await navigationService.NavigateAsync("SplashPage");
                    //})
                    //prism.OnAppStart(navigationService => navigationService.CreateBuilder()
                    //.AddSegment<SplashPageViewModel>())
                    prism.RegisterTypes(container =>
                    {
                        container.RegisterForNavigation<MainPage>();
                        container.RegisterForNavigation<SplashPage>();
                        container.RegisterForNavigation<ServiceEndpointWizzardPage>();
                        container.RegisterForNavigation<LoginWizzardPage>();
                        container.RegisterForNavigation<HomePage>();
                        container.RegisterForNavigation<SettingsPage>();
                        container.RegisterForNavigation<LoginSettingsPage>();
                        container.RegisterForNavigation<AlbumDetailPage>();
                        container.RegisterForRegionNavigation<AlbumsCarouselView, AlbumsCarouselViewModel>();
                        container.RegisterForRegionNavigation<FeaturedAlbumsView, FeaturedAlbumsViewModel>();
                        container.RegisterSingleton<IRequestService, RequestService>();
                        container.RegisterSingleton<IResourceService, ResourceService>();
                        container.RegisterSingleton<IDataService, DataService>(); 
                        container.RegisterSingleton<ISettingsService, SettingsService>();
                        container.RegisterSingleton<IStorageService, StorageService>();
                        container.RegisterSingleton<IImageService, ImageService>();
                        container.RegisterSingleton<IAppInfoService, AppInfoService>();
                        container.RegisterSingleton<IAuthenticationService, AuthenticationService>();
                        container.RegisterSingleton<IFlyoutNavigationService, FlyoutNavigationService>();

                    })
                    //.OnInitialized(containerProvider =>
                    //{
                    //    var regionManager = containerProvider.Resolve<IRegionManager>();
                    //    regionManager.RequestNavigate("AlbumsCarousel", nameof(AlbumsCarouselView));
                    //    //regionManager.RegisterViewWithRegion("AlbumsCarouselView", nameof(AlbumsCarouselView));
                    //})
                    .OnAppStart(navigationService => navigationService.CreateBuilder()
                    .AddSegment<SplashPageViewModel>()
                    .NavigateAsync(HandleNavigationError));
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("ionicons.ttf", "ionicons");
                }).ConfigureMauiHandlers((handlers)=>
                {
#if IOS
                    handlers.AddHandler(typeof(ExtendedTabbedPage), typeof(BSE.Tunes.Maui.Client.Platforms.iOS.Renderers.ExtendedTabbedRenderer));
#endif
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void HandleNavigationError(Exception ex)
        {
            Console.WriteLine(ex);
            System.Diagnostics.Debugger.Break();
        }
    }
}
