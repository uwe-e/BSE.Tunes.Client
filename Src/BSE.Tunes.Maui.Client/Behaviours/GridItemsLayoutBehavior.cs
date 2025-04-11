using BSE.Tunes.Maui.Client.Views.Templates;

namespace BSE.Tunes.Maui.Client.Behaviours
{
    public static class GridItemsLayoutBehavior
    {
        public static readonly BindableProperty EnableResponsiveSpanProperty =
            BindableProperty.CreateAttached(
                "EnableResponsiveSpan",
                typeof(bool),
                typeof(GridItemsLayoutBehavior),
                false,
                propertyChanged: OnEnableResponsiveSpanChanged);

        public static bool GetEnableResponsiveSpan(BindableObject view) =>
            (bool)view.GetValue(EnableResponsiveSpanProperty);

        public static void SetEnableResponsiveSpan(BindableObject view, bool value) =>
            view.SetValue(EnableResponsiveSpanProperty, value);

        private static void OnEnableResponsiveSpanChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is VisualElement element)
            {
                element.SizeChanged -= OnElementSizeChanged;
                if (newValue is bool isEnabled && isEnabled)
                {
                    element.SizeChanged += OnElementSizeChanged;
                }
            }
        }

        private static void OnElementSizeChanged(object sender, EventArgs e)
        {
            if (sender is CollectionView collectionView &&
                collectionView.ItemsLayout is GridItemsLayout gridLayout &&
                collectionView.ItemTemplate is DataTemplate dataTemplate)
            {
                double itemWidth = 100; // Customize as needed
                if (dataTemplate.LoadTemplate() is ViewItemTemplate viewItemTemplate)
                {
                    itemWidth = viewItemTemplate.HeightRequest;
                }

                // Calculate the new span based on available width
                int newSpan = Math.Max((int)(collectionView.Width / itemWidth), 1);

                // Update the span only if it has changed
                if (gridLayout.Span != newSpan)
                {
                    collectionView.ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical)
                    {
                        Span = newSpan
                    };
                }
            }
        }
    }
}
