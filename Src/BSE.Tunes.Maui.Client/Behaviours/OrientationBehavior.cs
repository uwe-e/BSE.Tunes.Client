namespace BSE.Tunes.Maui.Client.Behaviours
{
    public class OrientationBehavior : Behavior<VisualElement>
    {
        protected override void OnAttachedTo(VisualElement bindable)
        {
            base.OnAttachedTo(bindable);

            // Set initial state based on the current orientation
            OrientationBehavior.UpdateVisualState(DeviceDisplay.MainDisplayInfo.Orientation, bindable);

            // Subscribe to orientation changes
            DeviceDisplay.MainDisplayInfoChanged += (sender, e) =>
            {
                OrientationBehavior.UpdateVisualState(e.DisplayInfo.Orientation, bindable);
            };
        }

        protected override void OnDetachingFrom(VisualElement bindable)
        {
            base.OnDetachingFrom(bindable);

            // Unsubscribe from orientation changes
            DeviceDisplay.MainDisplayInfoChanged -= (sender, e) =>
            {
                OrientationBehavior.UpdateVisualState(e.DisplayInfo.Orientation, bindable);
            };
        }

        //private void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        //{
        //    UpdateVisualState(e.DisplayInfo.Orientation, (VisualElement)sender);
        //}

        private static void UpdateVisualState(DisplayOrientation orientation, VisualElement element)
        {
            string state = orientation == DisplayOrientation.Portrait ? "Portrait" : "Landscape";
            VisualStateManager.GoToState(element, state);
        }
    }
}
