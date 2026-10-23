using Android.App;
using Android.Runtime;
using Android.Util;

namespace BSE.Tunes.Maui.Client
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

#if DEBUG
            ConfigureAndroidLogging();
#endif

            // Memory optimizations
            ConfigureMemoryManagement();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        private void ConfigureAndroidLogging()
        {
            // Suppress verbose GC and memory logs
            Java.Lang.JavaSystem.SetProperty("dalvik.vm.verbose-gc", "false");

            // Reduce log verbosity for common noisy tags
            SetMinimumLogLevel("art", LogPriority.Warn);
            SetMinimumLogLevel("dalvikvm", LogPriority.Error);
            SetMinimumLogLevel("Choreographer", LogPriority.Warn);
            SetMinimumLogLevel("Timeline", LogPriority.Warn);
            SetMinimumLogLevel("MediaElement", LogPriority.Info);
            SetMinimumLogLevel("ExoPlayer", LogPriority.Warn);

            // Suppress GC-specific logs
            SetMinimumLogLevel("System.gc", LogPriority.Error);
        }

        private void ConfigureMemoryManagement()
        {
            // Increase heap size limit (helps reduce GC frequency)
            Java.Lang.JavaSystem.SetProperty("dalvik.vm.heapsize", "512m");

            // Optimize GC for throughput over low pause times
            Java.Lang.JavaSystem.SetProperty("dalvik.vm.heapstartsize", "16m");
            Java.Lang.JavaSystem.SetProperty("dalvik.vm.heapgrowthlimit", "256m");
        }

        private void SetMinimumLogLevel(string tag, LogPriority priority)
        {
            try
            {
                // This only affects logs visible in logcat, not actual GC behavior
                Log.IsLoggable(tag, priority);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set log level for {tag}: {ex.Message}");
            }
        }
    }
}
