

using Gorse.NET.Models;
using RestSharp;

namespace Gorse.NET;

public partial class Gorse
{
    /// <summary>
    /// Insert feedback. Duplicate feedback will have their values summed.
    /// Use UpsertFeedbackAsync for overwrite semantics.
    /// </summary>
    public Result InsertFeedback(IEnumerable<Feedback> feedbacks)
    {
        return _client.Request<Result, IEnumerable<Feedback>>(Method.Post, "api/feedback", feedbacks)!;
    }

    /// <summary>
    /// Insert feedback. Duplicate feedback will have their values summed.
    /// Use UpsertFeedbackAsync for overwrite semantics.
    /// </summary>
    public Task<Result> InsertFeedbackAsync(IEnumerable<Feedback> feedbacks)
    {
        return _client.RequestAsync<Result, IEnumerable<Feedback>>(Method.Post, "api/feedback", feedbacks)!;
    }

    /// <summary>
    /// Upsert feedback. Duplicate feedback will be overwritten (not summed).
    /// Use InsertFeedbackAsync for additive/sum semantics.
    /// </summary>
    public Result UpsertFeedback(IEnumerable<Feedback> feedbacks)
    {
        return _client.Request<Result, IEnumerable<Feedback>>(Method.Put, "api/feedback", feedbacks)!;
    }

    /// <summary>
    /// Upsert feedback. Duplicate feedback will be overwritten (not summed).
    /// Use InsertFeedbackAsync for additive/sum semantics.
    /// </summary>
    public Task<Result> UpsertFeedbackAsync(IEnumerable<Feedback> feedbacks)
    {
        return _client.RequestAsync<Result, IEnumerable<Feedback>>(Method.Put, "api/feedback", feedbacks)!;
    }

    public FeedbacksResponse GetFeedbacks(int n = 10, string cursor = "")
    {
        return _client.Request<FeedbacksResponse, Object>(Method.Get, $"api/feedback?n={n}&cursor={cursor}", null)!;
    }

    public Task<FeedbacksResponse> GetFeedbacksAsync(int n = 10, string cursor = "")
    {
        return _client.RequestAsync<FeedbacksResponse, Object>(Method.Get, $"api/feedback?n={n}&cursor={cursor}", null)!;
    }

    public FeedbacksResponse GetFeedbacksWithFeedbackType(string feedbackType, int n = 10, string cursor = "")
    {
        return _client.Request<FeedbacksResponse, Object>(Method.Get, $"api/feedback/{feedbackType}?n={n}&cursor={cursor}", null)!;
    }

    public Task<FeedbacksResponse> GetFeedbacksWithFeedbackTypeAsync(string feedbackType, int n = 10, string cursor = "")
    {
        return _client.RequestAsync<FeedbacksResponse, Object>(Method.Get, $"api/feedback/{feedbackType}?n={n}&cursor={cursor}", null)!;
    }

    public Feedback GetUserItemFeedbacksWithFeedbackType(string userId, string itemId, string feedbackType)
    {
        return _client.Request<Feedback, Object>(Method.Get, $"api/feedback/{feedbackType}/{userId}/{itemId}", null)!;
    }

    public Task<Feedback> GetUserItemFeedbacksWithFeedbackTypeAsync(string userId, string itemId, string feedbackType)
    {
        return _client.RequestAsync<Feedback, Object>(Method.Get, $"api/feedback/{feedbackType}/{userId}/{itemId}", null)!;
    }

    public FeedbacksResponse GetUserItemFeedbacks(string userId, string itemId, int n = 10, string cursor = "")
    {
        return _client.Request<FeedbacksResponse, Object>(Method.Get, $"api/feedback?user-id={userId}&item-id={itemId}&n={n}&cursor={cursor}", null)!;
    }

    public Task<FeedbacksResponse> GetUserItemFeedbacksAsync(string userId, string itemId, int n = 10, string cursor = "")
    {
        return _client.RequestAsync<FeedbacksResponse, Object>(Method.Get, $"api/feedback?user-id={userId}&item-id={itemId}&n={n}&cursor={cursor}", null)!;
    }

    public Result DeleteUserItemFeedbacks(string userId, string itemId)
    {
        return _client.Request<Result, Object>(Method.Delete, $"api/feedback/{userId}/{itemId}", null)!;
    }

    public Task<Result> DeleteUserItemFeedbacksAsync(string userId, string itemId)
    {
        return _client.RequestAsync<Result, Object>(Method.Delete, $"api/feedback/{userId}/{itemId}", null)!;
    }

    public Result DeleteUserItemFeedbacksWithFeedbackType(string userId, string itemId, string feedbackType)
    {
        return _client.Request<Result, Object>(Method.Delete, $"api/feedback/{feedbackType}/{userId}/{itemId}", null)!;
    }

    public Task<Result> DeleteUserItemFeedbacksWithFeedbackTypeAsync(string userId, string itemId, string feedbackType)
    {
        return _client.RequestAsync<Result, Object>(Method.Delete, $"api/feedback/{feedbackType}/{userId}/{itemId}", null)!;
    }
}