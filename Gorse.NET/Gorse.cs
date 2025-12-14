using Gorse.NET.Utilities;
using RestSharp;

namespace Gorse.NET;

public partial class GorseClient
{
    private readonly RequestClient _client;
    public GorseClient(string endpoint, string apiKey)
    {
        var restClient = new RestClient(endpoint);
        restClient.AddDefaultHeader("X-API-Key", apiKey);
        _client = new RequestClient(restClient);
    }
}
