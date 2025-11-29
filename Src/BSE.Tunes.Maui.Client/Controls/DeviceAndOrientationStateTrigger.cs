namespace BSE.Tunes.Maui.Client.Controls
{
    public class DeviceAndOrientationStateTrigger : StateTriggerBase
    {
        public static readonly BindableProperty IdiomProperty =
            BindableProperty.Create(nameof(Idiom), typeof(string), typeof(DeviceAndOrientationStateTrigger), DeviceIdiom.Phone.ToString(),
                propertyChanged: (b, o, n) => ((DeviceAndOrientationStateTrigger)b).UpdateState());

        public string Idiom
        {
            get => (string)GetValue(IdiomProperty);
            set => SetValue(IdiomProperty, value);
        }

        public static readonly BindableProperty OrientationProperty =
            BindableProperty.Create(nameof(Orientation), typeof(DisplayOrientation), typeof(DeviceAndOrientationStateTrigger), DisplayOrientation.Portrait,
                propertyChanged: (b, o, n) => ((DeviceAndOrientationStateTrigger)b).UpdateState());

        public DisplayOrientation Orientation
        {
            get => (DisplayOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public DeviceAndOrientationStateTrigger()
        {
            DeviceDisplay.MainDisplayInfoChanged += OnDisplayInfoChanged;
            UpdateState();
        }

        private void OnDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e) => UpdateState();

        private void UpdateState()
        {
            var orientation = DeviceDisplay.MainDisplayInfo.Orientation;
            var idiom = DeviceInfo.Idiom.ToString();

            bool isActive = idiom.Equals(Idiom, StringComparison.OrdinalIgnoreCase)
                 && orientation.Equals(Orientation);

            SetActive(isActive);
        }
    }
}
