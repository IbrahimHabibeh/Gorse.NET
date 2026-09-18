using System.Globalization;
using Gorse.NET.Models;
using RestSharp;

namespace Gorse.NET;

public partial class Gorse
{
    private static readonly Dictionary<string, string> ScoredRecommendHeaders = new() { { "X-API-Version", "2" } };

    /// <summary>
    /// Get recommendation with scores for a user.
    /// Uses X-API-Version: 2 header to return scores.
    /// </summary>
    public List<UserScore>? GetRecommend(string userId, IEnumerable<string>? categories = null,
        string? writeBackType = null, string? writeBackDelay = null, int? n = null, int? offset = null)
    {
        return _client.RequestWithHeaders<List<UserScore>, object>(Method.Get,
            GetRecommendResource(userId, categories, writeBackType, writeBackDelay, n, offset), null,
            ScoredRecommendHeaders);
    }

    /// <inheritdoc cref="GetRecommend"/>
    public Task<List<UserScore>?> GetRecommendAsync(string userId, IEnumerable<string>? categories = null,
        string? writeBackType = null, string? writeBackDelay = null, int? n = null, int? offset = null,
        CancellationToken cancellationToken = default)
    {
        return _client.RequestWithHeadersAsync<List<UserScore>, object>(Method.Get,
            GetRecommendResource(userId, categories, writeBackType, writeBackDelay, n, offset), null,
            ScoredRecommendHeaders, cancellationToken);
    }

    private static string GetRecommendResource(string userId, IEnumerable<string>? categories,
        string? writeBackType, string? writeBackDelay, int? n, int? offset)
    {
        var parameters = new List<(string, string?)>();
        if (categories != null)
        {
            parameters.AddRange(categories.Select(category => ("category", (string?)category)));
        }
        parameters.Add(("write-back-type", writeBackType));
        parameters.Add(("write-back-delay", writeBackDelay));
        parameters.Add(("n", n?.ToString(CultureInfo.InvariantCulture)));
        parameters.Add(("offset", offset?.ToString(CultureInfo.InvariantCulture)));
        return $"api/recommend/{Seg(userId)}" + Query(parameters.ToArray());
    }

    public List<UserScore> GetUserNeighbors(string userId, int n = 100, int offset = 0)
    {
        return _client.Request<List<UserScore>, object>(Method.Get,
            $"api/user/{Seg(userId)}/neighbors" + ScoreQuery(n, offset), null)!;
    }

    public Task<List<UserScore>> GetUserNeighborsAsync(string userId, int n = 100, int offset = 0,
        CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<List<UserScore>, object>(Method.Get,
            $"api/user/{Seg(userId)}/neighbors" + ScoreQuery(n, offset), null, cancellationToken)!;
    }

    /// <summary>
    /// Neighbors of an item from a named item-to-item recommender. With the VideoHub
    /// Gorse fork and <c>[videohub] incremental_item_to_item = true</c> this also answers
    /// for items that were inserted after the last item-to-item job. When
    /// <paramref name="userId"/> is set, items that user has already read are removed.
    /// </summary>
    public Task<List<UserScore>> GetItemNeighborsAsync(
        string itemId,
        int n = 12,
        int offset = 0,
        string recommender = "neighbors",
        string? category = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<List<UserScore>, object>(Method.Get,
            $"api/item-to-item/{Seg(recommender)}/{Seg(itemId)}" + ScoreQuery(n, offset, category, userId),
            null, cancellationToken)!;
    }

    public Task<List<UserScore>> GetRecommendLatestAsync(
        int n = 10,
        int offset = 0,
        string? category = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<List<UserScore>, object>(Method.Get,
            "api/latest" + ScoreQuery(n, offset, category, userId), null, cancellationToken)!;
    }

    public Task<List<UserScore>> GetCollaborativeFilteringAsync(
        string userId,
        int n = 10,
        int offset = 0,
        string? category = null,
        string? removeReadUserId = null,
        CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<List<UserScore>, object>(Method.Get,
            $"api/collaborative-filtering/{Seg(userId)}" + ScoreQuery(n, offset, category, removeReadUserId),
            null, cancellationToken)!;
    }

    public Task<List<UserScore>> GetUserToUserAsync(
        string name,
        string userId,
        int n = 10,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<List<UserScore>, object>(Method.Get,
            $"api/user-to-user/{Seg(name)}/{Seg(userId)}" + ScoreQuery(n, offset), null, cancellationToken)!;
    }

    public Task<List<UserScore>> GetNonPersonalizedAsync(
        string name,
        int n = 10,
        int offset = 0,
        string? removeReadUserId = null,
        CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<List<UserScore>, object>(Method.Get,
            $"api/non-personalized/{Seg(name)}" + ScoreQuery(n, offset, userId: removeReadUserId),
            null, cancellationToken)!;
    }

    private static string ScoreQuery(int n, int offset = 0, string? category = null, string? userId = null) =>
        Query(
            ("n", n.ToString(CultureInfo.InvariantCulture)),
            ("offset", offset > 0 ? offset.ToString(CultureInfo.InvariantCulture) : null),
            ("category", category),
            ("user-id", userId));
}
