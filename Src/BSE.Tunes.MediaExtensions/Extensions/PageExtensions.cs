namespace BSE.Tunes.MediaExtensions.Extensions
{
    public static class PageExtensions
    {
        public static Page GetCurrentPage(this Page currentPage)
        {
            if (currentPage.NavigationProxy.ModalStack.LastOrDefault() is Page modal)
            {
                return modal;
            }
            return currentPage;
        }
    }
}
