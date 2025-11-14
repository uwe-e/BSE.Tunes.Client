using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSE.Tunes.MediaExtensions.Extensions
{
    public static class PageExtensions
    {
        public static Page GetCurrentPage(this Page currenPpage)
        {
            if (currenPpage.NavigationProxy.ModalStack.LastOrDefault() is Page modal)
            {
                return modal;
            }
            //else if (page is FlyoutPage fp)
            //{
            //    return GetCurrentPage(fp.Detail);
            //}
            //else if (page is Shell shell && shell.CurrentItem?.CurrentItem is IShellSectionController ssc)
            //{
            //    return ssc.PresentedPage;
            //}
            //else if (currenPpage is IPageContainer<Page> pc)
            //{
            //    return GetCurrentPage(pc.CurrentPage);
            //}
            //else
            {
                return currenPpage;
            }
        }
    }
}
