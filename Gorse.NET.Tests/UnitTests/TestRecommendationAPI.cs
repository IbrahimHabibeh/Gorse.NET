using Gorse.NET.Models;

namespace Gorse.NET.Tests;

public partial class Tests
{
    [Test]
    public void TestRecommend()
    {
        client.InsertUser(new Models.User { UserId = "3000" });
        var recommendations = client.GetRecommend("3000");
        Assert.That(recommendations, Is.Not.Null);
        Assert.That(recommendations[0].Id, Is.EqualTo("315"));
        Assert.That(recommendations[1].Id, Is.EqualTo("1432"));
        Assert.That(recommendations[2].Id, Is.EqualTo("918"));
    }

    [Test]
    public void TestRecommendMultipleCategories()
    {
        client.InsertUser(new Models.User { UserId = "5000" });
        var recommendations = client.GetRecommend("5000", categories: ["Drama", "Comedy"], n: 3);
        Assert.That(recommendations, Has.Count.EqualTo(3));
        foreach (var recommendation in recommendations)
        {
            var item = client.GetItem(recommendation.Id);
            Assert.That(item.Categories, Has.Some.Matches<string>(category => category is "Drama" or "Comedy"));
        }
    }

    [Test]
    public async Task TestRecommendAsync()
    {
        await client.InsertUserAsync(new User { UserId = "4000" });
        var recommendations = await client.GetRecommendAsync("4000");
        Assert.That(recommendations, Is.Not.Null);
        Assert.That(recommendations[0].Id, Is.EqualTo("315"));
        Assert.That(recommendations[1].Id, Is.EqualTo("1432"));
        Assert.That(recommendations[2].Id, Is.EqualTo("918"));
    }

    [Test]
    public async Task TestRecommendMultipleCategoriesAsync()
    {
        await client.InsertUserAsync(new User { UserId = "6000" });
        var recommendations = await client.GetRecommendAsync("6000", categories: ["Drama", "Comedy"], n: 3);
        Assert.That(recommendations, Has.Count.EqualTo(3));
        foreach (var recommendation in recommendations)
        {
            var item = await client.GetItemAsync(recommendation.Id);
            Assert.That(item.Categories, Has.Some.Matches<string>(category => category is "Drama" or "Comedy"));
        }
    }
}
