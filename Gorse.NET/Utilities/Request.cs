using System.Text.Json;
using RestSharp;

namespace Gorse.NET.Utilities;

/// <summary>
/// Thin transport over RestSharp: builds the request, maps non-success responses
/// and transport failures to <see cref="GorseException"/> and deserializes the body.
/// The synchronous and asynchronous paths share the same request and response handling.
/// </summary>
public class RequestClient
{
    private readonly RestClient _client;

    public RequestClient(RestClient client)
    {
        _client = client;
    }

    public RetType? Request<RetType, ReqType>(Method method, string resource, ReqType? req) where ReqType : class
    {
        return RequestWithHeaders<RetType, ReqType>(method, resource, req, null);
    }

    public RetType? RequestWithHeaders<RetType, ReqType>(Method method, string resource, ReqType? req,
        Dictionary<string, string>? headers) where ReqType : class
    {
        var response = _client.Execute(BuildRequest(method, resource, req, headers));
        return ReadResponse<RetType>(response);
    }

    public Task<RetType?> RequestAsync<RetType, ReqType>(Method method, string resource, ReqType? req,
        CancellationToken cancellationToken = default) where ReqType : class
    {
        return RequestWithHeadersAsync<RetType, ReqType>(method, resource, req, null, cancellationToken);
    }

    public async Task<RetType?> RequestWithHeadersAsync<RetType, ReqType>(Method method, string resource, ReqType? req,
        Dictionary<string, string>? headers, CancellationToken cancellationToken = default) where ReqType : class
    {
        var response = await _client.ExecuteAsync(BuildRequest(method, resource, req, headers), cancellationToken)
            .ConfigureAwait(false);
        // RestSharp reports a cancelled request as a failed response; callers expect
        // the standard cancellation contract instead of a GorseException.
        cancellationToken.ThrowIfCancellationRequested();
        return ReadResponse<RetType>(response);
    }

    private static RestRequest BuildRequest<ReqType>(Method method, string resource, ReqType? req,
        Dictionary<string, string>? headers) where ReqType : class
    {
        var request = new RestRequest(resource, method);
        if (req != null)
        {
            request.AddJsonBody(req);
        }
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.AddHeader(header.Key, header.Value);
            }
        }
        return request;
    }

    private static RetType? ReadResponse<RetType>(RestResponse response)
    {
        if (!response.IsSuccessStatusCode)
        {
            // Status code 0 means the request never produced an HTTP response
            // (connection refused, timeout, DNS failure): surface the transport error.
            throw new GorseException(
                message: string.IsNullOrEmpty(response.Content) ? response.ErrorMessage : response.Content,
                statusCode: response.StatusCode,
                innerException: response.ErrorException);
        }
        if (string.IsNullOrWhiteSpace(response.Content))
        {
            return default;
        }
        try
        {
            return JsonSerializer.Deserialize<RetType>(response.Content);
        }
        catch (JsonException jsonEx)
        {
            throw new GorseException(
                message: $"Deserialization failed: {jsonEx.Message}\nResponse content: {response.Content}\nStatus code: {response.StatusCode}",
                statusCode: response.StatusCode,
                innerException: jsonEx);
        }
    }
}
