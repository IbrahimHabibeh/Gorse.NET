using System.Net;
using System.Text;
using System.Text.Json;
using Gorse.NET.Models;
using Gorse.NET.Utilities;

namespace Gorse.NET.Tests;

/// <summary>
/// Tests that do not need a Gorse server: a stub handler records the HTTP request the
/// client produces and replays a canned response.
/// </summary>
public class OfflineTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string ResponseBody { get; set; } = "{\"RowAffected\":1}";
        public TimeSpan Delay { get; set; } = TimeSpan.Zero;

        public HttpMethod? Method { get; private set; }
        public string? PathAndQuery { get; private set; }
        public string? Body { get; private set; }
        public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Method = request.Method;
            PathAndQuery = request.RequestUri!.PathAndQuery;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            foreach (var header in request.Headers)
            {
                Headers[header.Key] = string.Join(",", header.Value);
            }
            if (Delay > TimeSpan.Zero)
            {
                await Task.Delay(Delay, cancellationToken);
            }
            return new HttpResponseMessage(StatusCode)
            {
                Content = new StringContent(ResponseBody, Encoding.UTF8, "application/json"),
            };
        }
    }

    private StubHandler handler = null!;
    private HttpClient httpClient = null!;
    private Gorse client = null!;

    [SetUp]
    public void SetUp()
    {
        handler = new StubHandler();
        httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://gorse.test:8087/") };
        client = new Gorse(httpClient, "secret");
    }

    [TearDown]
    public void TearDown()
    {
        client.Dispose();
        httpClient.Dispose();
        handler.Dispose();
    }

    [Test]
    public async Task SendsApiKeyAndJsonBody()
    {
        await client.InsertItemsAsync([new Item { ItemId = "a", Categories = ["youtube"], Labels = new Dictionary<string, object?> { ["embedding"] = new[] { 0.5, 1.0 } } }]);

        Assert.That(handler.Method, Is.EqualTo(HttpMethod.Post));
        Assert.That(handler.PathAndQuery, Is.EqualTo("/api/items"));
        Assert.That(handler.Headers["X-API-Key"], Is.EqualTo("secret"));
        using var body = JsonDocument.Parse(handler.Body!);
        var item = body.RootElement[0];
        Assert.That(GetProperty(item, "ItemId").GetString(), Is.EqualTo("a"));
        Assert.That(GetProperty(item, "Labels").GetProperty("embedding").GetArrayLength(), Is.EqualTo(2));
    }

    [Test]
    public async Task PatchItemLabelsSendsOnlyTheGivenKeysIncludingNulls()
    {
        var result = await client.PatchItemLabelsAsync("item/1", new Dictionary<string, object?>
        {
            ["embedding"] = new[] { 0.25, 0.75 },
            ["stale"] = null,
        });

        Assert.That(result.RowAffected, Is.EqualTo(1));
        Assert.That(handler.Method, Is.EqualTo(HttpMethod.Patch));
        Assert.That(handler.PathAndQuery, Is.EqualTo("/api/item/item%2F1/labels"));
        using var body = JsonDocument.Parse(handler.Body!);
        Assert.That(body.RootElement.EnumerateObject().Select(property => property.Name), Is.EquivalentTo(new[] { "embedding", "stale" }));
        Assert.That(body.RootElement.GetProperty("stale").ValueKind, Is.EqualTo(JsonValueKind.Null));
        Assert.That(body.RootElement.GetProperty("embedding")[1].GetDouble(), Is.EqualTo(0.75));
    }

    [Test]
    public async Task ItemPatchOmitsUnsetProperties()
    {
        await client.UpdateItemAsync("a", new ItemPatch { IsHidden = true });

        Assert.That(handler.Method, Is.EqualTo(HttpMethod.Patch));
        Assert.That(handler.PathAndQuery, Is.EqualTo("/api/item/a"));
        using var body = JsonDocument.Parse(handler.Body!);
        var names = body.RootElement.EnumerateObject().Select(property => property.Name).ToList();
        Assert.That(names, Has.Count.EqualTo(1));
        Assert.That(names[0], Is.EqualTo("isHidden").IgnoreCase);
        Assert.That(body.RootElement.EnumerateObject().First().Value.GetBoolean(), Is.True);
    }

    [Test]
    public async Task UserPatchOmitsUnsetProperties()
    {
        await client.UpdateUserAsync("u", new UserPatch { Labels = new Dictionary<string, object?> { ["lang"] = "en" } });

        using var body = JsonDocument.Parse(handler.Body!);
        Assert.That(body.RootElement.EnumerateObject().Count(), Is.EqualTo(1));
        Assert.That(GetProperty(body.RootElement, "Labels").GetProperty("lang").GetString(), Is.EqualTo("en"));
    }

    [Test]
    public async Task RecommendationQueriesEscapeAndSkipEmptyParameters()
    {
        handler.ResponseBody = "[{\"Id\":\"x\",\"Score\":0.5}]";

        var neighbors = await client.GetItemNeighborsAsync("item 1", n: 5, recommender: "label neighbors", category: "a&b", userId: "u/1");
        Assert.That(handler.PathAndQuery, Is.EqualTo("/api/item-to-item/label%20neighbors/item%201?n=5&category=a%26b&user-id=u%2F1"));
        Assert.That(neighbors[0].Id, Is.EqualTo("x"));
        Assert.That(neighbors[0].Score, Is.EqualTo(0.5));

        await client.GetRecommendLatestAsync(n: 3, offset: 6);
        Assert.That(handler.PathAndQuery, Is.EqualTo("/api/latest?n=3&offset=6"));

        handler.ResponseBody = "{\"Cursor\":\"\",\"Items\":[]}";
        await client.GetItemsAsync(10);
        Assert.That(handler.PathAndQuery, Is.EqualTo("/api/items?n=10"));
    }

    [Test]
    public async Task GetRecommendRequestsScoredResponse()
    {
        handler.ResponseBody = "[{\"Id\":\"x\",\"Score\":2}]";

        var scores = await client.GetRecommendAsync("u1", categories: ["a", "b"], n: 2);

        Assert.That(handler.Headers["X-API-Version"], Is.EqualTo("2"));
        Assert.That(handler.PathAndQuery, Is.EqualTo("/api/recommend/u1?category=a&category=b&n=2"));
        Assert.That(scores![0].Score, Is.EqualTo(2));
    }

    [Test]
    public void ErrorResponsesCarryStatusAndBody()
    {
        handler.StatusCode = HttpStatusCode.NotFound;
        handler.ResponseBody = "a: item not found";

        var exception = Assert.ThrowsAsync<GorseException>(() => client.GetItemAsync("a"));
        Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(exception.Message, Is.EqualTo("a: item not found"));

        var syncException = Assert.Throws<GorseException>(() => client.GetItem("a"));
        Assert.That(syncException!.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public void MalformedResponsesAreReportedAsGorseException()
    {
        handler.ResponseBody = "not json";
        var exception = Assert.ThrowsAsync<GorseException>(() => client.GetItemAsync("a"));
        Assert.That(exception!.Message, Does.Contain("Deserialization failed"));
        Assert.That(exception.InnerException, Is.InstanceOf<JsonException>());
    }

    [Test]
    public void CancellationSurfacesAsOperationCanceled()
    {
        handler.Delay = TimeSpan.FromSeconds(30);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        Assert.That(
            async () => await client.GetItemNeighborsAsync("a", cancellationToken: cancellation.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task ReadinessIsDeserialized()
    {
        handler.ResponseBody = "{\"Ready\":true,\"DataStoreError\":null,\"CacheStoreError\":null,\"DataStoreConnected\":true,\"CacheStoreConnected\":true}";

        var health = await client.GetReadinessAsync();

        Assert.That(handler.PathAndQuery, Is.EqualTo("/api/health/ready"));
        Assert.That(health.Ready, Is.True);
        Assert.That(health.DataStoreConnected && health.CacheStoreConnected, Is.True);
    }

    [Test]
    public async Task UserItemFeedbackUsesThePathRoute()
    {
        handler.ResponseBody = "[{\"FeedbackType\":\"like\",\"UserId\":\"u\",\"ItemId\":\"i\",\"Value\":1,\"Timestamp\":\"2026-01-01T00:00:00Z\"}]";

        var feedback = await client.GetUserItemFeedbacksAsync("u", "i");

        Assert.That(handler.PathAndQuery, Is.EqualTo("/api/feedback/u/i"));
        Assert.That(feedback, Has.Count.EqualTo(1));
        Assert.That(feedback[0].FeedbackType, Is.EqualTo("like"));
    }

    // RestSharp serializes with web defaults (camelCase); Gorse matches field names
    // case-insensitively, so the tests do the same.
    private static JsonElement GetProperty(JsonElement element, string name) =>
        element.EnumerateObject().First(property => string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)).Value;
}
