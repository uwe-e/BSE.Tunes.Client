namespace BSE.Tunes.Maui.Client.Controls
{
    public class FlyoutButton : Button
    {
        public static readonly BindableProperty HorizontalContentAlignmentProperty =
            BindableProperty.Create(
                nameof(HorizontalContentAlignment),
                typeof(TextAlignment), typeof(FlyoutButton), TextAlignment.Start);

        public TextAlignment HorizontalContentAlignment
        {
            get { return (TextAlignment)GetValue(HorizontalContentAlignmentProperty); }
            set { SetValue(HorizontalContentAlignmentProperty, value); }
        }
    }
}
