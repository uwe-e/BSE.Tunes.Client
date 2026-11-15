namespace BSE.Tunes.WinUI.Client.Services
{
    public class VersionService : IVersionService
    {
        public string GetVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
        }
    }
}
