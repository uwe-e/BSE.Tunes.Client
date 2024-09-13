using CommunityToolkit.Maui.Views;

namespace BSE.Tunes.Maui.Client.Services
{
    public class PlayerService : IPlayerService
    {
        public static readonly BindableProperty RegisterAsMediaServiceProperty =
            BindableProperty.Create("RegisterAsMediaService",
                typeof(bool),
                typeof(PlayerService),
                false,
                propertyChanged: RegisterAsMediaServicePropertyChanged);

        public static bool GetRegisterAsMediaService(MediaElement target)
        {
            return (bool)target.GetValue(RegisterAsMediaServiceProperty);
        }

        public static void SetRegisterAsMediaService(MediaElement target, bool value)
        {
            target.SetValue(RegisterAsMediaServiceProperty, value);
        }

        private static void RegisterAsMediaServicePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MediaElement mediaElement)
            {
                bool toRegister = Convert.ToBoolean(newValue);
                if (toRegister) {
                    
                    var playerService = Application.Current?.Handler.MauiContext?.Services.GetService<IPlayerService>();
                    if (playerService != null)
                    {
                        playerService.RegisterAsMediaService(mediaElement);
                    }
                }
            }
        }

        public void RegisterAsMediaService(MediaElement mediaElement)
        {

        }
    }
}
