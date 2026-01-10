using Gorse.NET.Models;
using RestSharp;

namespace Gorse.NET;

public partial class GorseClient
{
    public string[]? GetRecommend(string userId, int? n = 10)
    {
        return _client.Request<string[], Object>(Method.Get, $"api/recommend/{userId}?n={n}", null);
    }

    public Task<string[]?> GetRecommendAsync(string userId, int? n = 10)
    {
        return _client.RequestAsync<string[], Object>(Method.Get, $"api/recommend/{userId}?n={n}", null);
    }

    public List<UserScore> GetUserNeighbors(string userId, int n = 100, int offset = 0)
    {
        return _client.Request<List<UserScore>, object>(Method.Get, $"api/user/{userId}/neighbors?n={n}&offset={offset}", null)!;
    }

    public Task<List<UserScore>> GetUserNeighborsAsync(string userId, int n = 100, int offset = 0)
    {
        return _client.RequestAsync<List<UserScore>, object>(Method.Get, $"api/user/{userId}/neighbors?n={n}&offset={offset}", null)!;
    }

    public Task<List<UserScore>> GetRecommendLatestAsync(int? n = 10)
    {
        return _client.RequestAsync<List<UserScore>, object>(Method.Get, $"api/latest?n={n}", null)!;
    }
}