using BSE.Maui.Controls;
using BSE.Tunes.Maui.Client.Services;
using BSE.Tunes.Maui.Client.ViewModels;
using BSE.Tunes.Maui.Client.Views;
using BSE.Tunes.MediaExtensions;
using FFImageLoading.Maui;
using Microsoft.Extensions.Logging;
using Xe.AcrylicView;

namespace BSE.Tunes.Maui.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            //.UseMauiCommunityToolkitMediaElement()
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseFFImageLoading()
                .UseAcrylicView()
                .UseBSEControls()
                .UseBSEMediaElementExtension()
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
                        container.RegisterForNavigation<MainPage, MainPageViewModel>();
                        container.RegisterForNavigation<SplashPage>();
                        container.RegisterForNavigation<ServiceEndpointWizzardPage>();
                        container.RegisterForNavigation<LoginWizzardPage>();
                        container.RegisterForNavigation<HomePage, HomePageViewModel>();
                        container.RegisterForNavigation<AlbumsPage, AlbumsPageViewModel>();
                        container.RegisterForNavigation<PlaylistsPage, PlaylistsPageViewModel>();
                        container.RegisterForNavigation<PlaylistDetailPage, PlaylistDetailPageViewModel>();
                        container.RegisterForNavigation<NewPlaylistDialogPage, NewPlaylistDialogPageViewModel>();
                        container.RegisterForNavigation<NowPlayingPage, NowPlayingPageViewModel>();
                        container.RegisterForNavigation<SettingsPage, SettingsPageViewModel>();
                        container.RegisterForNavigation<LoginSettingsPage>();
                        container.RegisterForNavigation<ServiceEndpointSettingsPage>();
                        container.RegisterForNavigation<CacheSettingsPage>();
                        container.RegisterForNavigation<AlbumDetailPage, AlbumDetailPageViewModel>();
                        container.RegisterForNavigation<PlaylistActionToolbarPage, PlaylistActionToolbarPageViewModel>();
                        container.RegisterForNavigation<PlaylistSelectorDialogPage, PlaylistSelectorDialogPageViewModel>();
                        container.RegisterForNavigation<SearchPage, SearchPageViewModel>();
                        container.RegisterForNavigation<SearchAlbumsPage, SearchAlbumsPageViewModel>();
                        container.RegisterForNavigation<SearchTracksPage, SearchTracksPageViewModel>();
                        container.RegisterForRegionNavigation<AlbumsCarouselView, AlbumsCarouselViewModel>();
                        container.RegisterForRegionNavigation<FeaturedAlbumsView, FeaturedAlbumsViewModel>();
                        container.RegisterForRegionNavigation<FeaturedPlaylistsView, FeaturedPlaylistsViewModel>();
                        container.RegisterForRegionNavigation<RandomPlayerButtonView, RandomPlayerButtonViewModel>();
                        container.RegisterSingleton<IRequestService, RequestService>();
                        container.RegisterSingleton<IResourceService, ResourceService>();
                        container.RegisterSingleton<IDataService, DataService>();
                        container.RegisterSingleton<ISettingsService, SettingsService>();
                        container.RegisterSingleton<IStorageService, StorageService>();
                        container.RegisterSingleton<IImageService, ImageService>();
                        container.RegisterSingleton<ITimerService, TimerService>();
                        /*
                         * since we use the ffimageloading features,
                         * it's no longer enough to clear the file cache, 
                         * but we also need to clear the cache of a ffimageloading image
                         */
                        container.RegisterSingleton<IImageCacheService, ImageCacheService>();
                        container.RegisterSingleton<IAppInfoService, AppInfoService>();
                        container.RegisterSingleton<IAuthenticationService, AuthenticationService>();
                        container.RegisterSingleton<IMediaService, MediaService>();
                        container.RegisterSingleton<IMediaManager, MediaManager>();
                        //Must be a scoped registration
                        container.RegisterScoped<IFlyoutNavigationService, FlyoutNavigationService>();


                    })
                    //.OnInitialized(containerProvider =>
                    //{
                    //    var regionManager = containerProvider.Resolve<IRegionManager>();
                    //    regionManager.RequestNavigate("AlbumsCarousel", nameof(AlbumsCarouselView));
                    //    //regionManager.RegisterViewWithRegion("AlbumsCarouselView", nameof(AlbumsCarouselView));
                    //})
                    .CreateWindow(navigationService => navigationService.CreateBuilder()
                    .AddSegment<SplashPageViewModel>()
                    .NavigateAsync(HandleNavigationError));
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                }).ConfigureMauiHandlers((handlers) =>
                {
#if IOS
                    //handlers.AddHandler(typeof(ExtendedTabbedPage), typeof(BSE.Tunes.Maui.Client.Platforms.iOS.Renderers.ExtendedTabbedRenderer));
                    //handlers.AddHandler(typeof(TabbedPageContainer), typeof(BSE.Tunes.Maui.Client.Platforms.iOS.TabbedContainerRenderer));
#endif
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            //builder.Services.AddTransient<PlaylistActionToolbarPage>();
            //builder.Services.AddTransient<PlaylistActionToolbarPageViewModel>();
            // builder.UseMauiCompatibility();

            return builder.Build();
        }

        private static void HandleNavigationError(Exception ex)
        {
            Console.WriteLine(ex);
            System.Diagnostics.Debugger.Break();
        }
    }
}
