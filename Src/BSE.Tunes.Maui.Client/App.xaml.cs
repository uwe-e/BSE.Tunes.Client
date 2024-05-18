
using BSE.Tunes.Maui.Client.Resources.Styles;
using Microsoft.Maui.ApplicationModel;

namespace BSE.Tunes.Maui.Client
{
    public partial class App : Application
    {
        private AppTheme _currentTheme;

        public App()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

        }
    }
}
