using System.Net.Http.Headers;

namespace System.Net.Http
{
    public static class HttpClientExtensions
    {
        public static void SetToken(this HttpClient client, string scheme, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
        }

        public static void SetBearerToken(this HttpClient client, string token)
        {
            client.SetToken("Bearer", token);
        }
        public static void AddRange(this HttpClient request, long from, long to)
        {
            if (request != null)
            {
                request.DefaultRequestHeaders.Range = new RangeHeaderValue(from, to);
            }
        }
    }
}
