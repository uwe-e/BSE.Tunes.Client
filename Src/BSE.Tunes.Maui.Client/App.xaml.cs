
using BSE.Tunes.Maui.Client.Resources.Styles;
using Microsoft.Maui.ApplicationModel;

namespace BSE.Tunes.Maui.Client
{
    public partial class App : Application
    {
        private AppTheme _currentTheme;

        public App()
        {
            RequestedThemeChanged += (s, a) => ConfigureTheme(a.RequestedTheme);

            InitializeComponent();

            _currentTheme = Current.RequestedTheme;
            SetTheme(_currentTheme);

        }

        private void ConfigureTheme(AppTheme requestedTheme)
        {
            if (requestedTheme != _currentTheme)
            {
                _currentTheme = requestedTheme;
                SetTheme(_currentTheme);
            }
        }

        private void SetTheme(AppTheme theme)
        {
            if (Current is not null)
            {
                var mergedDictionaries = Current.Resources.MergedDictionaries;
                if (mergedDictionaries != null)
                {
                    mergedDictionaries.Clear();

                    switch (theme)
                    {
                        case AppTheme.Dark:
                            mergedDictionaries.Add(new DarkTheme());
                            break;
                        case AppTheme.Light:
                        default:
                            mergedDictionaries.Add(new LightTheme());
                            break;
                    }
                }
            }
        }
    }
}
