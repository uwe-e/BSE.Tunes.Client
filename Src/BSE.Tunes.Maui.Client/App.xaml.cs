using BSE.Tunes.Maui.Client.Events;

namespace BSE.Tunes.Maui.Client
{
    public partial class App : Application
    {
        private readonly IEventAggregator _eventAggregator;
        public App()
        {
            InitializeComponent();
            
            _eventAggregator = ContainerLocator.Current.Resolve<IEventAggregator>();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            Window window = base.CreateWindow(activationState);
            window.Destroying += (sender, e) =>
            {
                _eventAggregator.GetEvent<CleanUpResourcesEvent>().Publish();
            };
            return window;
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

        }
    }
}
