namespace BSE.Tunes.MediaExtensions.Extensions
{
    public static class PageExtensions
    {
        public static Page GetCurrentPage(this Page currentPpage)
        {
            if (currentPpage.NavigationProxy.ModalStack.LastOrDefault() is Page modal)
            {
                return modal;
            }
            return currentPpage;
        }
    }
}
