using System.Globalization;
using Gorse.NET.Models;
using RestSharp;

namespace Gorse.NET;

public partial class Gorse
{
    /// <summary>
    /// Insert feedback. Duplicate feedback will have their values summed.
    /// Use <see cref="UpsertFeedback(IEnumerable{Feedback})"/> for overwrite semantics.
    /// </summary>
    public Result InsertFeedback(IEnumerable<Feedback> feedbacks)
    {
        return _client.Request<Result, IEnumerable<Feedback>>(Method.Post, "api/feedback", feedbacks)!;
    }

    /// <inheritdoc cref="InsertFeedback"/>
    public Task<Result> InsertFeedbackAsync(IEnumerable<Feedback> feedbacks, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, IEnumerable<Feedback>>(Method.Post, "api/feedback", feedbacks, cancellationToken)!;
    }

    /// <summary>
    /// Upsert feedback. Duplicate feedback will be overwritten (not summed).
    /// Use <see cref="InsertFeedback(IEnumerable{Feedback})"/> for additive/sum semantics.
    /// </summary>
    public Result UpsertFeedback(IEnumerable<Feedback> feedbacks)
    {
        return _client.Request<Result, IEnumerable<Feedback>>(Method.Put, "api/feedback", feedbacks)!;
    }

    /// <inheritdoc cref="UpsertFeedback"/>
    public Task<Result> UpsertFeedbackAsync(IEnumerable<Feedback> feedbacks, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, IEnumerable<Feedback>>(Method.Put, "api/feedback", feedbacks, cancellationToken)!;
    }

    public FeedbacksResponse GetFeedbacks(int n = 10, string cursor = "")
    {
        return _client.Request<FeedbacksResponse, object>(Method.Get, "api/feedback" + Page(n, cursor), null)!;
    }

    public Task<FeedbacksResponse> GetFeedbacksAsync(int n = 10, string cursor = "", CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<FeedbacksResponse, object>(Method.Get, "api/feedback" + Page(n, cursor), null, cancellationToken)!;
    }

    public FeedbacksResponse GetFeedbacksWithFeedbackType(string feedbackType, int n = 10, string cursor = "")
    {
        return _client.Request<FeedbacksResponse, object>(Method.Get, $"api/feedback/{Seg(feedbackType)}" + Page(n, cursor), null)!;
    }

    public Task<FeedbacksResponse> GetFeedbacksWithFeedbackTypeAsync(string feedbackType, int n = 10, string cursor = "",
        CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<FeedbacksResponse, object>(Method.Get, $"api/feedback/{Seg(feedbackType)}" + Page(n, cursor), null, cancellationToken)!;
    }

    public Feedback GetUserItemFeedbacksWithFeedbackType(string userId, string itemId, string feedbackType)
    {
        return _client.Request<Feedback, object>(Method.Get, TypedFeedbackResource(feedbackType, userId, itemId), null)!;
    }

    public Task<Feedback> GetUserItemFeedbacksWithFeedbackTypeAsync(string userId, string itemId, string feedbackType,
        CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Feedback, object>(Method.Get, TypedFeedbackResource(feedbackType, userId, itemId), null, cancellationToken)!;
    }

    /// <summary>All feedback types recorded between one user and one item.</summary>
    public List<Feedback> GetUserItemFeedbacks(string userId, string itemId)
    {
        return _client.Request<List<Feedback>, object>(Method.Get, $"api/feedback/{Seg(userId)}/{Seg(itemId)}", null) ?? [];
    }

    /// <inheritdoc cref="GetUserItemFeedbacks"/>
    public Task<List<Feedback>> GetUserItemFeedbacksAsync(string userId, string itemId, CancellationToken cancellationToken = default)
    {
        return OrEmpty(_client.RequestAsync<List<Feedback>, object>(Method.Get, $"api/feedback/{Seg(userId)}/{Seg(itemId)}", null, cancellationToken));
    }

    public Result DeleteUserItemFeedbacks(string userId, string itemId)
    {
        return _client.Request<Result, object>(Method.Delete, $"api/feedback/{Seg(userId)}/{Seg(itemId)}", null)!;
    }

    public Task<Result> DeleteUserItemFeedbacksAsync(string userId, string itemId, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, object>(Method.Delete, $"api/feedback/{Seg(userId)}/{Seg(itemId)}", null, cancellationToken)!;
    }

    public Result DeleteUserItemFeedbacksWithFeedbackType(string userId, string itemId, string feedbackType)
    {
        return _client.Request<Result, object>(Method.Delete, TypedFeedbackResource(feedbackType, userId, itemId), null)!;
    }

    public Task<Result> DeleteUserItemFeedbacksWithFeedbackTypeAsync(string userId, string itemId, string feedbackType,
        CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, object>(Method.Delete, TypedFeedbackResource(feedbackType, userId, itemId), null, cancellationToken)!;
    }

    private static string Page(int n, string cursor) =>
        Query(("n", n.ToString(CultureInfo.InvariantCulture)), ("cursor", cursor));

    private static string TypedFeedbackResource(string feedbackType, string userId, string itemId) =>
        $"api/feedback/{Seg(feedbackType)}/{Seg(userId)}/{Seg(itemId)}";

}
