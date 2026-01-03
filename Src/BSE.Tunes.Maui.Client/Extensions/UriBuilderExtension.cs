namespace BSE.Tunes.Maui.Client.Extensions
{
    public static class UriBuilderExtension
    {
        internal static void AppendToPath(this UriBuilder builder, string pathToAdd)
        {
            var completePath = Path.Combine(builder.Path, pathToAdd);
            builder.Path = completePath;
        }

        public static void AppendQueryParameter(this UriBuilder builder, string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                return;
            }

            var query = builder.Query;
            if (query.Length > 1)
            {
                query += "&";
            }
            else
            {
                query = "?";
            }
            builder.Query = query + Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(value);
        }

        public static void AppendQueryParameters(this UriBuilder builder, Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return;
            }

            foreach (var parameter in parameters)
            {
                builder.AppendQueryParameter(parameter.Key, parameter.Value);
            }
        }
    }
}
