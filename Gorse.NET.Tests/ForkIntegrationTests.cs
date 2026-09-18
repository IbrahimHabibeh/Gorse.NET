using System.Text.Json;
using Gorse.NET.Models;
using Gorse.NET.Utilities;

namespace Gorse.NET.Tests;

/// <summary>
/// Tests for the endpoints of the VideoHub Gorse fork. They run only when
/// GORSE_FORK_TEST_ENDPOINT points at a fork server with label_patch and
/// incremental_item_to_item enabled and an "embedding" item-to-item recommender named
/// "neighbors" whose embedding_dimensions is 0 or 256.
/// </summary>
[Category("Integration")]
public class ForkIntegrationTests
{
    private static readonly string? Endpoint = Environment.GetEnvironmentVariable("GORSE_FORK_TEST_ENDPOINT");

    private Gorse client = null!;

    [OneTimeSetUp]
    public void Connect()
    {
        if (string.IsNullOrEmpty(Endpoint))
        {
            Assert.Ignore("GORSE_FORK_TEST_ENDPOINT is not set; skipping tests that need the VideoHub Gorse fork.");
        }
        client = new Gorse(Endpoint, Environment.GetEnvironmentVariable("GORSE_FORK_TEST_API_KEY") ?? "");
    }

    [OneTimeTearDown]
    public void Disconnect()
    {
        client?.Dispose();
    }

    private static double[] Vector(double value) => Enumerable.Repeat(value, 256).ToArray();

    [Test]
    public async Task LabelPatchMergesKeysAndNewItemsHaveNeighborsImmediately()
    {
        var source = "sdk-fork-source-" + Guid.NewGuid();
        var near = "sdk-fork-near-" + Guid.NewGuid();
        try
        {
            await client.InsertItemsAsync([
                new Item { ItemId = source, Categories = ["sdk-test"], Labels = new Dictionary<string, object?> { ["title"] = "source", ["embedding"] = Vector(0.5) } },
                new Item { ItemId = near, Categories = ["sdk-test"], Labels = new Dictionary<string, object?> { ["title"] = "near" } },
            ]);

            // the second item has no vector yet, so it cannot be a neighbor
            var before = await client.GetItemNeighborsAsync(source, n: 50, category: "sdk-test");
            Assert.That(before.Select(score => score.Id), Does.Not.Contain(near));

            // patch only the embedding: the title must survive, the stale key must go
            await client.PatchItemLabelsAsync(near, new Dictionary<string, object?> { ["embedding"] = Vector(0.501), ["title2"] = "added" });
            await client.PatchItemLabelsAsync(near, new Dictionary<string, object?> { ["title2"] = null });
            var patched = await client.GetItemAsync(near);
            var labels = (JsonElement)patched.Labels!;
            Assert.That(labels.GetProperty("title").GetString(), Is.EqualTo("near"));
            Assert.That(labels.GetProperty("embedding").GetArrayLength(), Is.EqualTo(256));
            Assert.That(labels.TryGetProperty("title2", out _), Is.False);

            // neighbors are available without waiting for the item-to-item job
            var after = await client.GetItemNeighborsAsync(source, n: 50, category: "sdk-test");
            Assert.That(after.Select(score => score.Id), Does.Contain(near));

            // hiding through ItemPatch keeps the labels and removes the item from neighbor lists
            await client.UpdateItemAsync(near, new ItemPatch { IsHidden = true });
            var hidden = await client.GetItemAsync(near);
            Assert.That(hidden.IsHidden, Is.True);
            Assert.That(((JsonElement)hidden.Labels!).GetProperty("title").GetString(), Is.EqualTo("near"));
            var afterHide = await client.GetItemNeighborsAsync(source, n: 50, category: "sdk-test");
            Assert.That(afterHide.Select(score => score.Id), Does.Not.Contain(near));
        }
        finally
        {
            await client.DeleteItemAsync(source);
            await client.DeleteItemAsync(near);
        }
    }

    [Test]
    public void LabelPatchOfAMissingItemIsNotFound()
    {
        var exception = Assert.ThrowsAsync<GorseException>(() =>
            client.PatchItemLabelsAsync("sdk-fork-missing-" + Guid.NewGuid(), new Dictionary<string, object?> { ["a"] = "b" }));
        Assert.That(exception!.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ReadinessReportsConnectedStores()
    {
        var health = await client.GetReadinessAsync();
        Assert.That(health.Ready, Is.True);
        Assert.That(health.DataStoreConnected && health.CacheStoreConnected, Is.True);
    }
}
