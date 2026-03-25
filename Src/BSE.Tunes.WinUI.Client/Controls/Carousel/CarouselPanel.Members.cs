using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Windows.Input;

namespace BSE.Tunes.WinUI.Client.Controls
{
    partial class CarouselPanel
    {
        public event EventHandler<IntEventArgs>? SelectedIndexChanged;

        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        private static void ItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CarouselPanel control)
            {
                control.InvalidateMeasure();
            }
        }

        public static readonly DependencyProperty ItemTemplateProperty = 
            DependencyProperty.Register(
                nameof(ItemTemplate), 
                typeof(DataTemplate), 
                typeof(CarouselPanel), 
                new PropertyMetadata(null, ItemTemplateChanged));

        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        private static void ItemWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CarouselPanel control)
            {
                control.InvalidateMeasure();
            }
        }

        public static readonly DependencyProperty ItemWidthProperty = 
            DependencyProperty.Register(
                nameof(ItemWidth), 
                typeof(double), 
                typeof(CarouselPanel), 
                new PropertyMetadata(400.0, ItemWidthChanged));

        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        private static void ItemHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CarouselPanel control)
            {
                control.InvalidateMeasure();
            }
        }

        public static readonly DependencyProperty ItemHeightProperty = 
            DependencyProperty.Register(
                nameof(ItemHeight), 
                typeof(double), 
                typeof(CarouselPanel), 
                new PropertyMetadata(300.0, ItemHeightChanged));

        public ICommand? ItemClickCommand
        {
            get => (ICommand?)GetValue(ItemClickCommandProperty);
            set => SetValue(ItemClickCommandProperty, value);
        }

        public static readonly DependencyProperty ItemClickCommandProperty = 
            DependencyProperty.Register(
                nameof(ItemClickCommand), 
                typeof(ICommand), 
                typeof(CarouselPanel), 
                new PropertyMetadata(null));

        private void OnPaneTapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is not ContentControl contentControl)
                return;

            if (contentControl.Tag is int tagValue)
            {
                SelectedIndexChanged?.Invoke(this, new IntEventArgs(tagValue));
            }

            if (ItemClickCommand?.CanExecute(contentControl.Content) == true)
            {
                ItemClickCommand.Execute(contentControl.Content);
            }
        }
    }
}
