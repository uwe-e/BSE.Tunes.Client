using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BSE.Tunes.WinUI.Client.Controls
{
    public class ConditionalMenuFlyout : MenuFlyout
    {
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register(
                nameof(IsVisible),
                typeof(bool),
                typeof(ConditionalMenuFlyout),
                new PropertyMetadata(true));

        public bool IsVisible
        {
            get => (bool)GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        public ConditionalMenuFlyout()
        {
            Opening += OnMenuFlyoutOpening;
        }

        private void OnMenuFlyoutOpening(object sender, object e)
        {
            if (!IsVisible)
            {
                // Prevent the flyout from opening by immediately hiding it
                Hide();
            }
        }
    }
}