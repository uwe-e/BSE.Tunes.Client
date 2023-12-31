using BSE.Tunes.Maui.Client.ViewModels;
using BSE.Tunes.Maui.Client.Views;
using Microsoft.Extensions.Logging;
using Prism;
using Prism.Ioc;

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
                    prism.OnAppStart(async navigationService =>
                    {
                        var result = await navigationService.NavigateAsync("SplashPage");
                        //navigationService.CreateBuilder()
                        //.AddSegment<SplashPageViewModel>();
                    })
                    .RegisterTypes(container =>
                    {
                        container.RegisterForNavigation<MainPage>();
                        container.RegisterForNavigation<SplashPage>();
                    });
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
