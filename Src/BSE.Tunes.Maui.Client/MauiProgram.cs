using BSE.Tunes.Maui.Client.Views;
using BSE.Tunes.Maui.Client.Controls;
using Microsoft.Extensions.Logging;
using BSE.Tunes.Maui.Client.ViewModels;

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
                    })
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
