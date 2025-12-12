namespace BSE.Tunes.Maui.Client.Extensions
{
    /// <summary>
    /// Common extensions for the <see cref="INavigationService"/>
    /// </summary>
    public static class NavigationServiceExtensions
    {
        /// <summary>
        /// Removes all regions and initiates navigation to the target specified by the <paramref name="name"/>.
        /// </summary>
        /// <param name="navigationService">Service for handling navigation between views</param>
        /// <param name="name">The name of the target to navigate to.</param>
        /// <param name="parameters">The navigation parameters</param>
        /// <returns></returns>
        public async static Task<INavigationResult> RestartAndNavigateAsync(this INavigationService navigationService, string name, INavigationParameters parameters = null)
        {
            var container = ContainerLocator.Container;
            var regionManager = container.Resolve<IRegionManager>();

            // Clear regions
            foreach (var region in regionManager.Regions.ToList())
                regionManager.Regions.Remove(region.Name);

            // navigate to the specified target
            return await navigationService.NavigateAsync(name, parameters);
        }
    }
    
}
