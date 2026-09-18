# Gorse.NET (VideoHub fork)

.NET client for the [Gorse](https://github.com/gorse-io/gorse) recommender system.

This is VideoHub's fork of [gorse-io/Gorse.NET](https://github.com/gorse-io/Gorse.NET). It tracks
upstream `main` and adds what VideoHub needs: .NET 10, a current RestSharp, the REST endpoints
upstream does not cover yet, cancellation support and the endpoints of the
[VideoHub Gorse fork](https://github.com/IbrahimHabibeh/gorse/tree/videohub). It is consumed as a
git submodule and project reference, not from NuGet.

## Usage

```c#
using Gorse.NET;
using Gorse.NET.Models;

using var client = new Gorse("http://127.0.0.1:8087", "api_key");

await client.InsertUserAsync(new User
{
    UserId = "100",
    Labels = new { gender = "M", occupation = "engineer" },
});

await client.InsertItemsAsync(new[]
{
    new Item
    {
        ItemId = "200",
        Categories = new[] { "Comedy", "Animation" },
        Labels = new Dictionary<string, object?> { ["embedding"] = new[] { 0.1, 0.2, 0.3 } },
        Timestamp = DateTime.UtcNow,
        Comment = "item comment",
    },
});

await client.InsertFeedbackAsync(new[]
{
    new Feedback { FeedbackType = "like", UserId = "100", ItemId = "200", Timestamp = DateTime.UtcNow },
});

// Scored recommendations (X-API-Version: 2)
List<UserScore>? recommended = await client.GetRecommendAsync("100", n: 10);

// Neighbors from a named item-to-item recommender
List<UserScore> related = await client.GetItemNeighborsAsync("200", n: 12, recommender: "neighbors");
```

Every asynchronous method takes an optional `CancellationToken` as its last parameter. A cancelled
request throws `OperationCanceledException`; every other failure throws `GorseException`, whose
`StatusCode` is the HTTP status (0 when no response was received) and whose `Message` is the
response body or the transport error.

To share an `HttpClient` managed by `IHttpClientFactory` (handlers, timeouts, resilience):

```c#
services.AddHttpClient("gorse", http => http.BaseAddress = new Uri("http://gorse:8087/"));
services.AddSingleton(provider =>
    new Gorse(provider.GetRequiredService<IHttpClientFactory>().CreateClient("gorse"), apiKey));
```

## Partial updates

`UpdateItemAsync(string, Item)` and `UpdateUserAsync(string, User)` exist for upstream compatibility
but send every property, defaults included, so they can un-hide an item or clear its categories.
Use the patch models, which only send what is set:

```c#
await client.UpdateItemAsync("200", new ItemPatch { IsHidden = true });
await client.UpdateUserAsync("100", new UserPatch { Labels = new { lang = "en" } });
```

## VideoHub Gorse fork endpoints

| Method | Endpoint | Server requirement |
|---|---|---|
| `PatchItemLabelsAsync(itemId, labels)` | `PATCH api/item/{id}/labels` | `[videohub] label_patch = true` |
| `GetItemNeighborsAsync(...)` for brand-new items | `GET api/item-to-item/{name}/{id}` | `[videohub] incremental_item_to_item = true` |

`PatchItemLabelsAsync` merges top-level label keys: given keys replace stored keys, `null` deletes a
key, everything else is kept. It replaces an embedding without resending the item:

```c#
await client.PatchItemLabelsAsync("200", new Dictionary<string, object?> { ["embedding"] = vector });
```

A server without the feature, and a missing item, both answer 404.

## API coverage

| Area | Methods |
|---|---|
| Users | `InsertUser(s)`, `GetUser(s)`, `UpdateUser`, `DeleteUser` |
| Items | `InsertItem(s)`, `GetItem(s)`, `SearchItems`, `UpdateItem`, `PatchItemLabels`, `DeleteItem` |
| Feedback | `InsertFeedback` (additive), `UpsertFeedback` (overwrite), `GetFeedbacks`, `GetFeedbacksWithFeedbackType`, `GetUserItemFeedbacks`, `GetUserItemFeedbacksWithFeedbackType`, `DeleteUserItemFeedbacks`, `DeleteUserItemFeedbacksWithFeedbackType` |
| Recommendation | `GetRecommend`, `GetRecommendLatestAsync`, `GetCollaborativeFilteringAsync`, `GetItemNeighborsAsync`, `GetUserToUserAsync`, `GetUserNeighbors`, `GetNonPersonalizedAsync` |
| Health | `GetLivenessAsync`, `GetReadinessAsync` |

Synchronous and `Async` variants exist for the upstream methods; the fork's recommendation and
health additions are asynchronous only.

## Differences from upstream

- Targets .NET 10 with RestSharp 114.
- `CancellationToken` on every asynchronous method; cancellation is reported as cancellation.
- `Gorse` is `IDisposable`, accepts a request timeout or a caller-owned `HttpClient`.
- Path segments and query values are escaped everywhere.
- Transport failures report their cause instead of an empty message.
- List endpoints return an empty list when Gorse answers `null` for "no results".
- `GetUserItemFeedbacks` calls `api/feedback/{user-id}/{item-id}` and returns `List<Feedback>`.
  Upstream sends the ids as query parameters, which Gorse ignores, so it returned unrelated feedback.
- `Item.Labels` and `User.Labels` are `object?` so label objects (embeddings, nested labels) round-trip.
- Added: `UpsertFeedback`, `ItemPatch`/`UserPatch`, item-to-item, latest, collaborative filtering,
  user-to-user and non-personalized recommendations, health probes, the fork endpoints above.

## Tests

```bash
dotnet test
```

`OfflineTests` run against a stub HTTP handler and need nothing else. The upstream integration tests
(`Category=Integration`) need a Gorse server loaded with the upstream test data and run only when
`GORSE_TEST_ENDPOINT` is set:

```bash
curl -sL https://raw.githubusercontent.com/gorse-io/gorse/refs/heads/master/client/setup-test.sh | bash
GORSE_TEST_ENDPOINT=http://127.0.0.1:8088 dotnet test
```

`ForkIntegrationTests` cover the VideoHub Gorse fork endpoints (label patch, immediate neighbors,
`ItemPatch`, readiness) and run only when `GORSE_FORK_TEST_ENDPOINT` points at a fork server with
`[videohub] label_patch` and `incremental_item_to_item` enabled, for example VideoHub's compose stack:

```bash
GORSE_FORK_TEST_ENDPOINT=http://localhost:8088 dotnet test
```

## Syncing with upstream

```bash
git remote add upstream https://github.com/gorse-io/Gorse.NET.git
git fetch upstream
git merge upstream/main
dotnet test
```
