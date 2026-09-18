using System.Text.Json;
using RestSharp;

namespace Gorse.NET.Utilities;
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

    public RetType? RequestWithHeaders<RetType, ReqType>(Method method, string resource, ReqType? req, Dictionary<string, string>? headers) where ReqType : class
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
        var response = _client.Execute(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new GorseException(message: response.Content, statusCode: response.StatusCode);
        }
        // Handle case where response content is null
        if (response.Content == null)
        {
            return default;
        }
        // Deserialize response content to the expected type
        try
        {
            return JsonSerializer.Deserialize<RetType>(response.Content);
        }
        catch (JsonException jsonEx) // Specific error handling for JSON deserialization
        {
            throw new GorseException(
                message: $"Deserialization failed: {jsonEx}. \nResponse content: {response.Content}. \nStatus code: {response.StatusCode}",
                statusCode: response.StatusCode
            );
        }
        catch (Exception ex) // General error handling for any other exceptions
        {
            throw new GorseException(
                message: $"An error occurred while processing the response: {ex}. \nResponse content: {response.Content}. \nStatus code: {response.StatusCode}",
                statusCode: response.StatusCode
            );
        }
    }

    public async Task<RetType?> RequestAsync<RetType, ReqType>(Method method, string resource, ReqType? req) where ReqType : class
    {
        return await RequestWithHeadersAsync<RetType, ReqType>(method, resource, req, null);
    }

    public async Task<RetType?> RequestWithHeadersAsync<RetType, ReqType>(Method method, string resource, ReqType? req, Dictionary<string, string>? headers) where ReqType : class
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
        var response = await _client.ExecuteAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new GorseException(message: response.Content, statusCode: response.StatusCode);
        }
        // Handle case where response content is null
        if (response.Content == null)
        {
            return default;
        }
        // Deserialize response content to the expected type
        try
        {
            return JsonSerializer.Deserialize<RetType>(response.Content);
        }
        catch (JsonException jsonEx) // Specific error handling for JSON deserialization
        {
            throw new GorseException(
                message: $"Deserialization failed: {jsonEx}. \nResponse content: {response.Content}. \nStatus code: {response.StatusCode}",
                statusCode: response.StatusCode
            );
        }
        catch (Exception ex) // General error handling for any other exceptions
        {
            throw new GorseException(
                message: $"An error occurred while processing the response: {ex}. \nResponse content: {response.Content}. \nStatus code: {response.StatusCode}",
                statusCode: response.StatusCode
            );
        }
    }
}
