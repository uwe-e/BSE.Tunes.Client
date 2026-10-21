using BSE.Tunes.WinUI.Client.Extensions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace BSE.Tunes.WinUI.Client.Controls
{
    public partial class CarouselPanel : Panel
    {
        private const int BUFFER_ITEMS = 2;
        
        public CarouselPanel()
        {
            this.UseLayoutRounding = true;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Center;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_items.Count == 0)
            {
                return base.MeasureOverride(availableSize);
            }

            this.ArrangePanes(availableWidth: availableSize.Width);

            int index = this.Index;
            int itemCount = _items.Count;
            int paneCount = base.Children.Count;
            int leftCount = (paneCount - 1) / 2;

            for (int n = 0; n < paneCount; n++)
            {
                int paneIndex = (index + n).Mod(paneCount);
                
                if (base.Children[paneIndex] is not ContentControl pane)
                    continue;

                int itemIndex = (index + n - leftCount).Mod(itemCount);
                object newContent = _items[itemIndex];
                
                // ✅ Only update if content has changed or template has changed
                bool contentChanged = !ReferenceEquals(pane.Content, newContent);
                bool templateChanged = pane.ContentTemplate != this.ItemTemplate;
                
                if (contentChanged || templateChanged)
                {
                    if (this.ItemTemplate is not null)
                    {
                        pane.ContentTemplate = this.ItemTemplate;
                    }
                    
                    pane.Content = newContent;
                    pane.Tag = itemIndex;
                }
                
                pane.Measure(new Size(this.ItemWidth, this.ItemHeight));
            }

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_items.Count == 0)
            {
                return base.ArrangeOverride(finalSize);
            }

            int index = this.Index;
            int paneCount = base.Children.Count;
            double itemWidth = this.ItemWidth;
            double x = index * itemWidth - (paneCount * itemWidth - finalSize.Width) / 2.0;

            for (int n = 0; n < paneCount; n++)
            {
                int paneIndex = (index + n).Mod(paneCount);
                
                if (base.Children[paneIndex] is ContentControl pane)
                {
                    pane.Arrange(new Rect(x, 0, itemWidth, finalSize.Height));
                    x += itemWidth;
                }
            }

            return new Size(0, finalSize.Height);
        }   

        private void ArrangePanes(double availableWidth)
        {
            double visibleWidth = availableWidth;
            
            // WinUI 3: Use XamlRoot to get window size
            if (this.XamlRoot is not null)
            {
                var windowBounds = this.XamlRoot.Size;
                visibleWidth = Math.Min(windowBounds.Width, availableWidth);
            }
            
            double viewportWidth = visibleWidth + BUFFER_ITEMS * this.ItemWidth;
            int visibleItems = (int)Math.Ceiling(viewportWidth / this.ItemWidth);
            
            // Ensure odd number of items for symmetric layout
            int totalItems = visibleItems + (visibleItems + 1) % 2;
            int diff = totalItems - base.Children.Count;

            if (diff > 0)
            {
                // Add missing panes
                for (int n = 0; n < diff; n++)
                {
                    base.Children.Add(CreatePane());
                }
            }
            else if (diff < 0)
            {
                // Remove excess panes
                for (int n = 0; n < -diff; n++)
                {
                    if (base.Children[^1] is ContentControl lastPane)
                    {
                        lastPane.Tapped -= OnPaneTapped;
                        // Clear content to release resources
                        lastPane.Content = null;
                        lastPane.ContentTemplate = null;
                    }
                    base.Children.RemoveAt(base.Children.Count - 1);
                }
            }
        }

        private ContentControl CreatePane()
        {
            var pane = new ContentControl
            {
                UseLayoutRounding = true,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
            };
            pane.Tapped += OnPaneTapped;
            return pane;
        }
    }
}
