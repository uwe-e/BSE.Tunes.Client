using BSE.Tunes.Maui.Client.Models;
using BSE.Tunes.Maui.Client.Utils;
using System.Reflection;
using System.Resources;

namespace BSE.Tunes.Maui.Client
{
    public static class AppSettings
    {
        const string ResourceId = "BSE.Tunes.XApp.Resx.AppResources";

        public static readonly Lazy<ResourceManager> ResourceManager =
            new Lazy<ResourceManager>(() => new ResourceManager(
                ResourceId,
                IntrospectionExtensions.GetTypeInfo(typeof(AppSettings)).Assembly
                )
            );

        public static string ServiceEndPoint
        {
            get => Preferences.Get(nameof(ServiceEndPoint), null);
            set => Preferences.Set(nameof(ServiceEndPoint), value);
        }

        public static User User
        {
            get => PreferencesHelpers.Get(nameof(User), default(User));
            set => PreferencesHelpers.Set(nameof(User), value);
        }
    }
}
