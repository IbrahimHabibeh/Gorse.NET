using Gorse.NET.Utilities;
using RestSharp;

namespace Gorse.NET;

/// <summary>
/// Client for the Gorse REST API. One instance is thread-safe and meant to be
/// shared for the lifetime of the application.
/// </summary>
public partial class Gorse : IDisposable
{
    private readonly RestClient _restClient;
    private readonly RequestClient _client;

    /// <param name="endpoint">Base URL of a Gorse server or master node, e.g. http://127.0.0.1:8087.</param>
    /// <param name="apiKey">Value of the X-API-Key header; empty when the server has no API key.</param>
    /// <param name="timeout">Per-request timeout. RestSharp's default applies when null.</param>
    public Gorse(string endpoint, string apiKey, TimeSpan? timeout = null)
        : this(new RestClient(new RestClientOptions(endpoint) { Timeout = timeout }), apiKey)
    {
    }

    /// <summary>
    /// Uses a caller-owned <see cref="HttpClient"/> (for example one created by
    /// IHttpClientFactory). Its BaseAddress must point at Gorse. The HttpClient is
    /// not disposed with this instance.
    /// </summary>
    public Gorse(HttpClient httpClient, string apiKey)
        : this(new RestClient(httpClient, disposeHttpClient: false), apiKey)
    {
    }

    private Gorse(RestClient restClient, string apiKey)
    {
        _restClient = restClient;
        if (!string.IsNullOrEmpty(apiKey))
        {
            _restClient.AddDefaultHeader("X-API-Key", apiKey);
        }
        _client = new RequestClient(_restClient);
    }

    public void Dispose()
    {
        _restClient.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Escapes one path segment (ids, recommender names, feedback types).</summary>
    private static string Seg(string value) => Uri.EscapeDataString(value);

    /// <summary>Builds a query string from the parameters that have a value.</summary>
    private static string Query(params (string Name, string? Value)[] parameters)
    {
        var parts = parameters
            .Where(parameter => !string.IsNullOrEmpty(parameter.Value))
            .Select(parameter => $"{parameter.Name}={Uri.EscapeDataString(parameter.Value!)}")
            .ToList();
        return parts.Count == 0 ? "" : "?" + string.Join("&", parts);
    }
}
