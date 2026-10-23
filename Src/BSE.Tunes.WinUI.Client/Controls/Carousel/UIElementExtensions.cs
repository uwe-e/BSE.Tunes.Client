using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace BSE.Tunes.WinUI.Client.Controls
{
    public static class UIElementExtensions
    {
        private const string TRANSFORM_NAME = "CarouselTransform";

        public static double GetTranslateX(this UIElement element)
        {
            if (element.RenderTransform is CompositeTransform transform)
            {
                return transform.TranslateX;
            }
            return 0;
        }

        public static void TranslateX(this UIElement element, double value)
        {
            EnsureTransform(element);

            if (element.RenderTransform is CompositeTransform transform)
            {
                transform.TranslateX = value;
            }
        }

        public static void TranslateDeltaX(this UIElement element, double delta)
        {
            EnsureTransform(element);

            if (element.RenderTransform is CompositeTransform transform)
            {
                transform.TranslateX += delta;
            }
        }

        public static async Task AnimateXAsync(this UIElement element, double to, double durationMs)
        {
            EnsureTransform(element);

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(animation, element.RenderTransform);
            Storyboard.SetTargetProperty(animation, "TranslateX");
            storyboard.Children.Add(animation);

            var tcs = new TaskCompletionSource<bool>();
            storyboard.Completed += (s, e) => tcs.SetResult(true);
            storyboard.Begin();

            await tcs.Task;
        }

        public static void FadeIn(this UIElement element, double durationMs = 300)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(durationMs)
            };

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        public static void FadeOut(this UIElement element, double durationMs = 300)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(durationMs)
            };

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private static void EnsureTransform(UIElement element)
        {
            if (element.RenderTransform is not CompositeTransform)
            {
                element.RenderTransform = new CompositeTransform();
            }
        }
    }
}