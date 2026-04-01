using System.Net;
using System.Net.Sockets;
using BSE.Tunes.Shared.Services.Abstractions;

namespace BSE.Tunes.Shared.Services.Services;

/// <summary>
/// Provides a local HTTP proxy service for streaming audio tracks through a dynamically started server. 
/// The proxy forwards requests to the actual API endpoint, handling authentication and supporting range requests for seeking.
/// </summary>
/// <remarks>
/// The LocalProxyService is intended for scenarios where audio tracks must be streamed through a
/// local endpoint, such as when a client application requires a local URL for playback. The service manages the
/// lifecycle of the proxy server, including starting, stopping, and resource cleanup. Thread safety is not
/// guaranteed; callers should ensure that methods are not called concurrently from multiple threads.
/// </remarks>
public class LocalProxyService : IDisposable
{
    private HttpListener? _listener;
    private readonly IRequestService _requestService;
    private readonly ISettingsService _settingsService;
    private bool _isRunning;
    private string _proxyBaseUrl = string.Empty;

    /// <summary>
    /// Initializes a new instance of the LocalProxyService class with the specified request and settings services.
    /// </summary>
    /// <param name="requestService">The service used to handle HTTP or network requests required by the proxy.</param>
    /// <param name="settingsService">The service that provides access to application or proxy configuration settings.</param>
    public LocalProxyService(IRequestService requestService, ISettingsService settingsService)
    {
        _requestService = requestService;
        _settingsService = settingsService;
    }

    /// <summary>
    /// Starts the local proxy server and returns the base URL to be used for streaming audio tracks.
    /// The URL will be in the format http://localhost:{port}/, where {port} is a randomly selected available port.
    /// The proxy will forward requests to the actual API endpoint, adding authentication headers as needed.
    /// </summary>
    /// <returns>Returns the base URL of the local proxy server.</returns>
    public Task<string> StartAsync()
    {
        if (_isRunning)
            return Task.FromResult(_proxyBaseUrl);

        // Use a random available port
        int port = GetAvailablePort();
        _proxyBaseUrl = $"http://localhost:{port}/";

        _listener = new HttpListener();
        _listener.Prefixes.Add(_proxyBaseUrl);
        _listener.Start();
        _isRunning = true;

        // Start listening in background
        _ = Task.Run(async () => await ListenAsync());

        Console.WriteLine($"Local proxy started at {_proxyBaseUrl}");
        return Task.FromResult(_proxyBaseUrl);
    }

    /// <summary>
    /// Gets the full proxy URL for a given track GUID.
    /// This URL can be used as the source for streaming the audio track through the local proxy.
    /// </summary>
    /// <param name="trackGuid">The unique identifier of the track.</param>
    /// <returns>The complete proxy URL for the specified track.</returns>
    public string GetProxyUrl(Guid trackGuid)
    {
        return $"{_proxyBaseUrl}{trackGuid}";
    }

    /// <summary>
    /// Disposes the local proxy server, stopping it and releasing any resources.
    /// After calling this method, the proxy will no longer be available for streaming audio tracks.
    /// </summary>
    public void Dispose()
    {
        _isRunning = false;

        if (_listener != null)
        {
            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch
            {
                // Ignore disposal errors
            }
            finally
            {
                _listener = null;
            }
        }

        GC.SuppressFinalize(this);
    }

    private async Task ListenAsync()
    {
        while (_isRunning && _listener != null && _listener.IsListening)
        {
            try
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                _ = Task.Run(async () => await HandleRequestAsync(context));
            }
            catch (HttpListenerException)
            {
                // Expected when stopping the listener
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Proxy error: {ex.Message}");
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            // Extract track GUID from URL path (e.g., /guid-here)
            var path = context.Request.Url?.AbsolutePath.Trim('/');

            if (string.IsNullOrEmpty(path) || !Guid.TryParse(path, out Guid trackGuid))
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
                return;
            }

            // Build actual API URL
            var builder = new UriBuilder(_settingsService.ServiceEndPoint);
            builder.Path = Path.Combine(builder.Path, $"/api/files/audio/{trackGuid}");

            // Forward range header if present (for seeking support)
            var request = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
            if (context.Request.Headers["Range"] != null)
            {
                request.Headers.Add("Range", context.Request.Headers["Range"]);
            }

            // Get authenticated HttpClient
            var httpClient = await _requestService.GetHttpClientAsync();
            
            // Make authenticated request to real API
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            // Copy response status and headers
            context.Response.StatusCode = (int)response.StatusCode;
            context.Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "audio/mpeg";

            if (response.Content.Headers.ContentLength.HasValue)
            {
                context.Response.ContentLength64 = response.Content.Headers.ContentLength.Value;
            }

            // Copy Accept-Ranges header for seeking support
            if (response.Headers.Contains("Accept-Ranges"))
            {
                context.Response.AddHeader("Accept-Ranges", string.Join(", ", response.Headers.GetValues("Accept-Ranges")));
            }

            // Copy content-range header for partial content
            if (response.Content.Headers.ContentRange != null)
            {
                context.Response.AddHeader("Content-Range", response.Content.Headers.ContentRange.ToString());
            }

            // Stream the content
            using var apiStream = await response.Content.ReadAsStreamAsync();
            await apiStream.CopyToAsync(context.Response.OutputStream);

            context.Response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling proxy request: {ex.Message}");
            try
            {
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
            catch
            {
                // Ignore if response already closed
            }
        }
    }

    private static int GetAvailablePort()
    {
        using var socket = new TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        int port = ((IPEndPoint)socket.LocalEndpoint).Port;
        socket.Stop();
        return port;
    }
}