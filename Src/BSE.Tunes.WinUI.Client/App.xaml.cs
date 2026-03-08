using BSE.Tunes.WinUI.Client.Services;
using BSE.Tunes.WinUI.Client.Views;
using Microsoft.UI.Xaml;
using Prism.DryIoc;
using Prism.Ioc;
using System.Xml.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BSE.Tunes.WinUI.Client
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : PrismApplication
    {
        //public Window MainWindow { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }
        private Window _mainWindow;




        protected override UIElement CreateShell()
        {
            //Container.RegisterInstance(MainWindow);

            var shell = Container.Resolve<ShellPage>();

            //shell.Loaded += (s, e) =>
            //{
            //    // Access the protected m_window field via reflection
            //    var window = GetWindowFromElement(shell);
            //    if (window != null)
            //    {
            //        var windowService = Container.Resolve<IWindowService>();
            //        windowService.RestoreWindowBounds(window);
            //        windowService.TrackWindow(window);
            //    }
            //};

            //var w = MainWindow;

            return shell;
        }


        protected override void OnInitialized()
        {
            base.OnInitialized();

            // Prism's PrismApplication should have a MainWindow property
            // Try accessing it after initialization
            //if (MainWindow != null)
            //{
            //    var windowService = Container.Resolve<IWindowService>();
            //    windowService.RestoreWindowBounds(MainWindow);
            //    windowService.TrackWindow(MainWindow);
            //}
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ShellPage>();
            containerRegistry.RegisterSingleton<IWindowService, WindowService>();
        }

        private static Window GetWindowFromElement(FrameworkElement element)
        {
            if (element.XamlRoot != null)
            {
                foreach (var window in Microsoft.UI.Xaml.Window.Current?.Content != null
                    ? new[] { Microsoft.UI.Xaml.Window.Current }
                    : System.Linq.Enumerable.Empty<Window>())
                {
                    if (window.Content?.XamlRoot == element.XamlRoot)
                    {
                        return window;
                    }
                }
            }
            return null;
        }
    

    }
}
