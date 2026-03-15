using BSE.Tunes.WinUI.Client.Contracts.Services;
using Microsoft.Extensions.Options;

namespace BSE.Tunes.WinUI.Client.Services;

public class PageService : IPageService
{
    private readonly IReadOnlyDictionary<string, Type> _pages;

    public PageService(IOptions<PageServiceConfiguration> configuration)
    {
        _pages = configuration.Value.Pages;
    }

    public Type GetPageType(string key)
    {
        if (!_pages.TryGetValue(key, out var pageType))
        {
            var availableKeys = string.Join(", ", _pages.Keys);
            throw new ArgumentException(
                $"Page not found: '{key}'. Did you forget to call AddTransientForNavigation? Available keys: {availableKeys}");
        }

        return pageType;
    }
}
