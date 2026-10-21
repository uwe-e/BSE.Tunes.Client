using BSE.Tunes.WinUI.Client.Contracts.Services;
using Microsoft.Windows.ApplicationModel.Resources;

namespace BSE.Tunes.WinUI.Client.Services;

public class ResourceService : IResourceService
{
    private readonly ResourceLoader _resourceLoader;

    public ResourceService()
    {
        _resourceLoader = new ResourceLoader();
    }

    public string GetString(string key)
    {
        return _resourceLoader.GetString(key);
    }
}