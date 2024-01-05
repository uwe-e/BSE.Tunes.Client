using BSE.Tunes.Maui.Client.Models;

namespace BSE.Tunes.Maui.Client.Services
{
    public interface ISettingsService
    {
        User User
        {
            get; set;
        }

        string ServiceEndPoint
        {
            get; set;
        }

        string Token
        {
            get; set;
        }
    }
}
