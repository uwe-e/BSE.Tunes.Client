using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;
using System.ComponentModel;
using UIKit;
using TabbedPage = Microsoft.Maui.Controls.TabbedPage;

namespace BSE.Tunes.Maui.Client.Platforms.iOS
{
    public class TabbedContainerRenderer : Microsoft.Maui.Controls.Handlers.Compatibility.TabbedRenderer//, IPlatformViewHandler
    {
        private UIView _bottomView;
        private IMauiContext _mauiContext;

        BSE.Tunes.Maui.Client.Controls.TabbedPageContainer Page => Element as BSE.Tunes.Maui.Client.Controls.TabbedPageContainer;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            UIApplication.SharedApplication.BeginReceivingRemoteControlEvents();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (Element == null)
                return;

            if (Element.Parent is BaseShellItem)
                Element.Layout(View.Bounds.ToRectangle());

            if (!Element.Bounds.IsEmpty)
            {
                View.Frame = new System.Drawing.RectangleF((float)Element.X, (float)Element.Y, (float)Element.Width, (float)Element.Height);
            }

            var frame = View.Frame;
            var tabBarFrame = TabBar.Frame;
            if (_bottomView != null)
            {
                _bottomView.Frame = new System.Drawing.RectangleF((float)Element.X, (float)(frame.Top + frame.Height - tabBarFrame.Height - 60), (float)Element.Width, (float)60);
                
                var audioPlayerFrame = _bottomView.Frame;
                
                Page.ContainerArea = new Rect(0, 0, frame.Width, frame.Height - audioPlayerFrame.Height - tabBarFrame.Height);
            }
        }

        public override void RemoteControlReceived(UIEvent theEvent)
        {
            base.RemoteControlReceived(theEvent);
            // The AudioPlayer view does not receive a RemoteControlReceived event.
            // Because of this we execute that event from here.
            //Console.WriteLine($"{nameof(RemoteControlReceived)} {theEvent.Subtype} ");
            _bottomView?.RemoteControlReceived(theEvent);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Tabbed != null)
                {
                    Tabbed.PropertyChanged -= OnPropertyChanged;
                }
            }
            base.Dispose(disposing);
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Page == null)
            {
                return;
            }
            try
            {
                //Tabbed.PropertyChanged += OnPropertyChanged;
                SetupUserInterface();
                UpdatePlayerBackgroundColor();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"\t\t\tERROR: {exception.Message}");
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TabbedPage.BarBackgroundColorProperty.PropertyName)
            {
                UpdatePlayerBackgroundColor();
            }
        }

        

        private void SetupUserInterface()
        {


            IVisualElementRenderer audioPlayerRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.GetRenderer(Page.BottomView);
            if (audioPlayerRenderer == null)
            {
                audioPlayerRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.CreateRenderer(Page.BottomView);
                Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.SetRenderer(Page.BottomView, audioPlayerRenderer);
            }
            _bottomView = audioPlayerRenderer.NativeView;
            _bottomView.Hidden = false;

            View.AddSubview(_bottomView);
        }
        
        private void UpdatePlayerBackgroundColor()
        {
            //_bottomView.BackgroundColor = ((TabbedPage)Element).BarBackgroundColor.ToUIColor();
        }

        //void IElementHandler.SetMauiContext(IMauiContext mauiContext)
        //{
        //    _mauiContext = mauiContext;
        //}

        //void IElementHandler.SetVirtualView(Microsoft.Maui.IElement view)
        //{
        //    SetElement((VisualElement)view);
        //}
    }
}