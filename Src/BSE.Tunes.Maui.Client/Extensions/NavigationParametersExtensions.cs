namespace BSE.Tunes.Maui.Client.Extensions
{
    public static class NavigationParametersExtensions
    {
        /// <summary>
        /// Determines whether the navigation request should be performed modally based on the specified navigation
        /// parameters.
        /// </summary>
        /// <remarks>This method checks for the presence of the 'UseModalNavigation' parameter in the
        /// provided navigation parameters. If the parameter is not present, the default value is false.</remarks>
        /// <param name="parameters">The navigation parameters to evaluate for modal navigation. Cannot be null.</param>
        /// <returns>true if the navigation should be performed modally; otherwise, false.</returns>
        public static bool IsModalNavigation(this INavigationParameters parameters)
        {
            return parameters.GetValue<bool>(KnownNavigationParameters.UseModalNavigation);
        }
    }
}