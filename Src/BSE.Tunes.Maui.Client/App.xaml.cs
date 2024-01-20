
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

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            _currentTheme = Current.RequestedTheme;
            SetTheme(_currentTheme);

            MauiExceptions.UnhandledException += (sender, args) =>
            {
                //_logger.LogCritical(e.ExceptionObject as Exception, "App failed to handle exception");
                //throw (Exception)e.ExceptionObject;
            };

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
        
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

        }
    }
}
