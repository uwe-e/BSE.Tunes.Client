using AVKit;
using BSE.Tunes.MediaExtensions.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Handlers;
using System.Diagnostics.CodeAnalysis;
using UIKit;

namespace BSE.Tunes.MediaExtensions.Views
{
    public class MauiMediaElement : UIView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MauiMediaElement"/> class.
        /// </summary>
        /// <param name="playerViewController"><The <see cref="AVPlayerViewController"/> that acts as the platform media player.</param>
        /// <param name="virtualView">The <see cref="MediaElement"/> used as the VirtualView for this <see cref="MauiMediaElement"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="playerViewController" <c>.View</c> /> is <see langword="null"/>.</exception>
        public MauiMediaElement(AVPlayerViewController playerViewController, MediaElement virtualView)
        {
            ArgumentNullException.ThrowIfNull(playerViewController.View);
            playerViewController.View.Frame = Bounds;

            UIViewController? viewController;

            // If any of the Parents in the VisualTree of MediaElement uses a UIViewController for their PlatformView, use it as the child ViewController
            // This enables support for UI controls like CommunityToolkit.Maui.Popup whose PlatformView is a UIViewController (e.g. `public class MauiPopup : UIViewController`)
            // To find the UIViewController, we traverse `MediaElement.Parent` until a Parent using UIViewController is located
            if (virtualView.TryFindParentPlatformView(out UIViewController? parentUIViewController))
            {
                viewController = parentUIViewController;
            }
            // If none of the Parents in the VisualTree of MediaElement use a UIViewController, we can use the ViewController in the PageHandler
            // To find the PageHandler, we traverse `MediaElement.Parent` until the Page is located
            else if (virtualView.TryFindParent<Page>(out var page)
                && page.Handler is PageHandler { ViewController: not null } pageHandler)
            {
                viewController = pageHandler.ViewController;
            }
            // If the parent Page cannot be found, MediaElement is being used inside a DataTemplate.
            else
            {
                if (!TryGetCurrentPage(out var currentPage))
                {
                    throw new InvalidOperationException("Cannot find current page");
                }

                viewController = Platform.GetCurrentUIViewController();
            }

            if (viewController?.View is not null)
            {
                // Zero out the safe area insets of the AVPlayerViewController
                UIEdgeInsets insets = viewController.View.SafeAreaInsets;
                playerViewController.AdditionalSafeAreaInsets =
                    new UIEdgeInsets(insets.Top * -1, insets.Left, insets.Bottom * -1, insets.Right);

                // Add the View from the AVPlayerViewController to the parent ViewController
                viewController.AddChildViewController(playerViewController);
            }

            AddSubview(playerViewController.View);

        }

        private static bool TryGetCurrentPage([NotNullWhen(true)] out Page? currentPage)
        {
            currentPage = null;

            if (Application.Current?.Windows is null)
            {
                return false;
            }

            if (Application.Current.Windows.Count is 0)
            {
                throw new InvalidOperationException("Unable to find active Window");
            }

            if (Application.Current.Windows.Count > 1)
            {
                // We are unable to determine which Window contains the ItemsView that contains the MediaElement when multiple ItemsView are being used in the same page
                // TODO: Add support for MediaElement in an ItemsView in a multi-window application
                throw new NotSupportedException("MediaElement is not currently supported in multi-window applications");
            }
            
            if (Application.Current.Windows[0].Page is Page page)
            {
                currentPage = PageExtensions.GetCurrentPage(page);
                return true;
            }
            return false;
        }
    }
}
