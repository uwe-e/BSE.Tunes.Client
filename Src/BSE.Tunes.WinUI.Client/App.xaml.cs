using BSE.Tunes.Shared.Services.Services;
using BSE.Tunes.WinUI.Client.Activation;
using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Extensions;
using BSE.Tunes.WinUI.Client.Models;
using BSE.Tunes.WinUI.Client.Services;
using BSE.Tunes.WinUI.Client.Services.Mappers.Profiles;
using BSE.Tunes.WinUI.Client.ViewModels;
using BSE.Tunes.WinUI.Client.Views;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

namespace BSE.Tunes.WinUI.Client;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public IServiceProvider Services { get; private set; }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx? MainWindow { get; set; }

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDialogService, DialogService>();
            // Core Services
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IFileSystemProvider, FileSystemProvider>();
            // Media Services
            services.AddSingleton<LocalProxyService>();
            services.AddSingleton<IMediaService, MediaService>();
            services.AddSingleton<IMediaManager, MediaManager>();
            services.AddSingleton<ITimerService, TimerService>();


            // Shared Services from BSE.Tunes.Shared.Services
            services.AddSingleton<IRequestService, RequestService>();
            services.AddSingleton<IImageService, ImageService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IDataService, DataService>();
            services.AddSingleton<IMapper>(mapper =>
            {
                return new Mapper(
                    new DtoMappingProfile()
                );
            });
            services.AddSingleton<IResourceService, ResourceService>();

            // SettingsService registered as a single instance with multiple interface registrations
            services.AddSingleton<SettingsService>();
            services.AddSingleton<ISettingsService>(sp => sp.GetRequiredService<SettingsService>());
            services.AddSingleton<ISettingsServiceExtended>(sp => sp.GetRequiredService<SettingsService>());
            services.AddSingleton<SettingsMonitorService>();

            // Views and ViewModels - Single point of configuration! ✨
            services.AddTransientForNavigation<MainViewModel, MainPage>();
            services.AddTransientForNavigation<SettingsViewModel, SettingsPage>();
            services.AddTransientForNavigation<PersonalizationSettingsViewModel, PersonalizationSettingsPage>();
            services.AddTransientForNavigation<EndpointConfigurationViewModel, EndpointConfigurationPage>();
            services.AddTransientForNavigation<RemoveEndpointSettingsPageViewModel, RemoveEndpointSettingsPage>();
            services.AddTransientForNavigation<LoginPageViewModel, LoginPage>();
            services.AddTransientForNavigation<RemoveLoginSettingsPageViewModel, RemoveLoginSettingsPage>();
            services.AddTransientForNavigation<AlbumDetailPageViewModel, AlbumDetailPage>();
            services.AddTransientForNavigation<AlbumsPageViewModel, AlbumsPage>();
            services.AddTransientForNavigation<PlaylistsPageViewModel, PlaylistsPage>();
            services.AddTransientForNavigation<PlaylistDetailPageViewModel, PlaylistDetailPage>();

            // ShellPage and ViewModel (not used for navigation)
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            services.AddTransient<AlbumsCarouselViewModel>();
            services.AddTransient<RandomPlayerViewModel>();
            services.AddTransient<PlayerBarViewModel>();
            services.AddTransient<FeaturedAlbumsViewModel>();
            services.AddTransient<FeaturedPlaylistViewModel>();
            services.AddTransient<CreatePlaylistDialogViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));

            
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        // Initialize MainWindow here instead of static initialization for .NET 8/9 compatibility
        MainWindow = new MainWindow();
        MainWindow.Closed += OnMainWindowClosed;
        Services = Host.Services;
        await App.GetService<IActivationService>().ActivateAsync(args);
    }

    private void OnMainWindowClosed(object sender, WindowEventArgs args)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Window closing - cleaning up media resources");

            // Stop media manager
            var mediaManager = Host?.Services?.GetService(typeof(IMediaManager)) as IMediaManager;
            mediaManager?.Disconnect();

            // Stop timer service if it's running
            var timerService = Host?.Services?.GetService(typeof(ITimerService)) as ITimerService;
            timerService?.Stop();

            System.Diagnostics.Debug.WriteLine("Cleanup complete");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
        }

        // Don't await - let cleanup happen in background without blocking window closure
        //_ = Task.Run(() => CleanupResourcesAsync());
    }

}
