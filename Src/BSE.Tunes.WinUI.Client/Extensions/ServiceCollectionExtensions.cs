using BSE.Tunes.WinUI.Client.Contracts.Services;
using BSE.Tunes.WinUI.Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a Page and ViewModel pair for navigation.
    /// Uses the full Page type name as the navigation key.
    /// </summary>
    /// <typeparam name="TViewModel">The ViewModel type</typeparam>
    /// <typeparam name="TPage">The Page type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="pageKey">Optional navigation key. If not provided, uses the full Page type name</param>
    public static IServiceCollection AddTransientForNavigation<TViewModel, TPage>(
        this IServiceCollection services,
        string? pageKey = null)
        where TViewModel : ObservableObject
        where TPage : Page
    {
        // Use full Page name as key: "SettingsPage", "EndpointConfigurationPage", etc.
        pageKey ??= typeof(TPage).Name;

        services.AddTransient<TViewModel>();
        services.AddTransient<TPage>();

        services.Configure<PageServiceConfiguration>(config =>
        {
            config.AddMapping<TViewModel, TPage>(pageKey);
        });

        return services;
    }
}