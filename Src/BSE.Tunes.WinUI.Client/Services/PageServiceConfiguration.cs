using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Services;

/// <summary>
/// Configuration class for PageService that stores the mappings between navigation keys and Page types.
/// </summary>
public class PageServiceConfiguration
{
    private readonly Dictionary<string, Type> _pages = new();

    /// <summary>
    /// Gets the read-only dictionary of page mappings.
    /// </summary>
    public IReadOnlyDictionary<string, Type> Pages => _pages;

    /// <summary>
    /// Adds a navigation mapping between a ViewModel and Page.
    /// </summary>
    /// <typeparam name="TViewModel">The ViewModel type</typeparam>
    /// <typeparam name="TPage">The Page type</typeparam>
    /// <param name="pageKey">The navigation key</param>
    /// <exception cref="ArgumentException">Thrown when the key or page type is already registered</exception>
    public void AddMapping<TViewModel, TPage>(string pageKey)
        where TViewModel : ObservableObject
        where TPage : Page
    {
        lock (_pages)
        {
            if (_pages.ContainsKey(pageKey))
            {
                throw new ArgumentException($"The key '{pageKey}' is already configured in PageService");
            }

            var pageType = typeof(TPage);
            if (_pages.ContainsValue(pageType))
            {
                var existingKey = _pages.First(p => p.Value == pageType).Key;
                throw new ArgumentException($"The type '{pageType.Name}' is already configured with key '{existingKey}'");
            }

            _pages.Add(pageKey, pageType);
        }
    }
}