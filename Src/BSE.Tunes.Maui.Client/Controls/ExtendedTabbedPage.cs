namespace BSE.Tunes.Maui.Client.Controls
{
    public class ExtendedTabbedPage : TabbedPage
    {
        public static readonly BindableProperty AudioPlayerBarProperty
            = BindableProperty.Create(nameof(AudioPlayerBar),
                                      typeof(AudioPlayer),
                                      typeof(ExtendedTabbedPage),
                                      null,
                                      propertyChanged: OnAudioPlayerBarChanged);

        public AudioPlayer AudioPlayerBar
        {
            get { return (AudioPlayer)GetValue(AudioPlayerBarProperty); }
            set { SetValue(AudioPlayerBarProperty, value); }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            AudioPlayer audioPlayer = AudioPlayerBar;
            if (audioPlayer != null)
            {
                SetInheritedBindingContext(audioPlayer, BindingContext);
            }
        }

        private static void OnAudioPlayerBarChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var newElement = (Element)newValue;
            if (newElement != null)
            {
                BindableObject.SetInheritedBindingContext(newElement, bindable.BindingContext);
            }
        }
    }
}
