using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace BSE.Tunes.Maui.Client.Handlers.TabbedContainer
{
    public partial class TabbedContainerHandler : ViewHandler<ITabbedView, PlatformView>, ITabbedViewHandler
    {
        public TabbedContainerHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
        {
        }

        protected override PlatformView CreatePlatformView()
        {
            throw new NotImplementedException();
        }
    }
}
