using Prism.Common;

namespace BSE.Tunes.Maui.Client
{
    internal static class PageUtilities
    {
        public static Guid UniqueId
        {
            get; set;
        }

        public static bool IsCurrentPageTypeOf(Type name, Guid uniqueId)
        {
            
            if (IsCurrentPageTypeOf(name))
            {
                if (uniqueId.CompareTo(UniqueId) != 0)
                {
                    UniqueId = uniqueId;
                    //return IsCurrentPageTypeOf(name);
                    return true;
                }
            }
            
            //if (uniqueId.CompareTo(UniqueId) != 0)
            //{
            //    UniqueId = uniqueId;
            //    return IsCurrentPageTypeOf(name);
            //}
            return false;
        }

        public static bool IsCurrentPageTypeOf(Type name)
        {
            var currentPage = MvvmHelpers.GetCurrentPage(Application.Current.MainPage);
            Console.WriteLine($"CurrentPage: {currentPage.GetType()}");
            return name.Equals(currentPage.GetType());
        }
    }
}