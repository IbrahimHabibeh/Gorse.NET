using Gorse.NET.Models;
using RestSharp;

namespace Gorse.NET;

public partial class Gorse
{
    private static string BuildRecommendationQuery(
        int n,
        int offset = 0,
        string? category = null,
        string? userId = null)
    {
        var parameters = new List<string>
        {
            $"n={n}"
        };

        if (offset > 0)
            parameters.Add($"offset={offset}");

        if (!string.IsNullOrWhiteSpace(category))
            parameters.Add($"category={Uri.EscapeDataString(category)}");

        if (!string.IsNullOrWhiteSpace(userId))
            parameters.Add($"user-id={Uri.EscapeDataString(userId)}");

        return string.Join("&", parameters);
    }

    public string[]? GetRecommend(string userId, int n = 10)
    {
        return _client.Request<string[], Object>(Method.Get, $"api/recommend/{userId}?n={n}", null);
    }

    public Task<string[]?> GetRecommendAsync(string userId, int n = 10)
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

    public Task<List<UserScore>> GetItemNeighborsAsync(
        string itemId,
        int n = 12,
        int offset = 0,
        string recommender = "neighbors",
        string? category = null,
        string? userId = null)
    {
        var query = BuildRecommendationQuery(n, offset, category, userId);
        return _client.RequestAsync<List<UserScore>, object>(
            Method.Get,
            $"api/item-to-item/{Uri.EscapeDataString(recommender)}/{Uri.EscapeDataString(itemId)}?{query}",
            null)!;
    }

    public Task<List<UserScore>> GetRecommendLatestAsync(
        int n = 10,
        int offset = 0,
        string? category = null,
        string? userId = null)
    {
        var query = BuildRecommendationQuery(n, offset, category, userId);
        return _client.RequestAsync<List<UserScore>, object>(Method.Get, $"api/latest?{query}", null)!;
    }

    public Task<List<UserScore>> GetCollaborativeFilteringAsync(
        string userId,
        int n = 10,
        int offset = 0,
        string? category = null,
        string? removeReadUserId = null)
    {
        var query = BuildRecommendationQuery(n, offset, category, removeReadUserId);
        return _client.RequestAsync<List<UserScore>, object>(
            Method.Get,
            $"api/collaborative-filtering/{Uri.EscapeDataString(userId)}?{query}",
            null)!;
    }

    public Task<List<UserScore>> GetUserToUserAsync(
        string name,
        string userId,
        int n = 10,
        int offset = 0)
    {
        return _client.RequestAsync<List<UserScore>, object>(
            Method.Get,
            $"api/user-to-user/{Uri.EscapeDataString(name)}/{Uri.EscapeDataString(userId)}?n={n}&offset={offset}",
            null)!;
    }

    public Task<List<UserScore>> GetNonPersonalizedAsync(
        string name,
        int n = 10,
        int offset = 0,
        string? removeReadUserId = null)
    {
        var query = BuildRecommendationQuery(n, offset, userId: removeReadUserId);
        return _client.RequestAsync<List<UserScore>, object>(
            Method.Get,
            $"api/non-personalized/{Uri.EscapeDataString(name)}?{query}",
            null)!;
    }
}
