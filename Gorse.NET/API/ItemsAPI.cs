using System.Globalization;
using Gorse.NET.Models;
using RestSharp;

namespace Gorse.NET;

public partial class Gorse
{
    public Result InsertItem(Item item)
    {
        return _client.Request<Result, Item>(Method.Post, "api/item", item)!;
    }

    public Task<Result> InsertItemAsync(Item item, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, Item>(Method.Post, "api/item", item, cancellationToken)!;
    }

    public Result InsertItems(IEnumerable<Item> items)
    {
        return _client.Request<Result, IEnumerable<Item>>(Method.Post, "api/items", items)!;
    }

    public Task<Result> InsertItemsAsync(IEnumerable<Item> items, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, IEnumerable<Item>>(Method.Post, "api/items", items, cancellationToken)!;
    }

    public Item GetItem(string itemId)
    {
        return _client.Request<Item, object>(Method.Get, $"api/item/{Seg(itemId)}", null)!;
    }

    public Task<Item> GetItemAsync(string itemId, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Item, object>(Method.Get, $"api/item/{Seg(itemId)}", null, cancellationToken)!;
    }

    public ItemsResponse GetItems(int n, string cursor = "")
    {
        return _client.Request<ItemsResponse, object>(Method.Get, ItemsResource(n, cursor), null)!;
    }

    public Task<ItemsResponse> GetItemsAsync(int n, string cursor = "", CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<ItemsResponse, object>(Method.Get, ItemsResource(n, cursor), null, cancellationToken)!;
    }

    public ItemsResponse SearchItems(string query, int n)
    {
        return _client.Request<ItemsResponse, object>(Method.Get, SearchItemsResource(query, n), null)!;
    }

    public Task<ItemsResponse> SearchItemsAsync(string query, int n, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<ItemsResponse, object>(Method.Get, SearchItemsResource(query, n), null, cancellationToken)!;
    }

    public Result DeleteItem(string itemId)
    {
        return _client.Request<Result, object>(Method.Delete, $"api/item/{Seg(itemId)}", null)!;
    }

    public Task<Result> DeleteItemAsync(string itemId, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, object>(Method.Delete, $"api/item/{Seg(itemId)}", null, cancellationToken)!;
    }

    /// <summary>
    /// Patches an item with every property of <paramref name="itemToUpdate"/>, including
    /// defaults: an unset IsHidden hides nothing but un-hides a hidden item, empty
    /// Categories clear the stored ones. Prefer the <see cref="ItemPatch"/> overload.
    /// </summary>
    public Result UpdateItem(string itemId, Item itemToUpdate)
    {
        return _client.Request<Result, object>(Method.Patch, $"api/item/{Seg(itemId)}", itemToUpdate)!;
    }

    /// <inheritdoc cref="UpdateItem(string, Item)"/>
    public Task<Result> UpdateItemAsync(string itemId, Item itemToUpdate, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, object>(Method.Patch, $"api/item/{Seg(itemId)}", itemToUpdate, cancellationToken)!;
    }

    /// <summary>Patches only the properties set on <paramref name="patch"/>.</summary>
    public Result UpdateItem(string itemId, ItemPatch patch)
    {
        return _client.Request<Result, ItemPatch>(Method.Patch, $"api/item/{Seg(itemId)}", patch)!;
    }

    /// <inheritdoc cref="UpdateItem(string, ItemPatch)"/>
    public Task<Result> UpdateItemAsync(string itemId, ItemPatch patch, CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, ItemPatch>(Method.Patch, $"api/item/{Seg(itemId)}", patch, cancellationToken)!;
    }

    /// <summary>
    /// Merges label keys into an item without resending the rest of it: keys in
    /// <paramref name="labels"/> replace the stored keys, null values delete keys and
    /// all other labels are kept. Typical use is replacing an embedding vector.
    /// Requires the VideoHub Gorse fork with <c>[videohub] label_patch = true</c>;
    /// other servers answer 404. A missing item also answers 404.
    /// </summary>
    public Result PatchItemLabels(string itemId, IReadOnlyDictionary<string, object?> labels)
    {
        return _client.Request<Result, IReadOnlyDictionary<string, object?>>(
            Method.Patch, $"api/item/{Seg(itemId)}/labels", labels)!;
    }

    /// <inheritdoc cref="PatchItemLabels"/>
    public Task<Result> PatchItemLabelsAsync(string itemId, IReadOnlyDictionary<string, object?> labels,
        CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<Result, IReadOnlyDictionary<string, object?>>(
            Method.Patch, $"api/item/{Seg(itemId)}/labels", labels, cancellationToken)!;
    }

    private static string ItemsResource(int n, string cursor) =>
        "api/items" + Query(("n", n.ToString(CultureInfo.InvariantCulture)), ("cursor", cursor));

    private static string SearchItemsResource(string query, int n) =>
        "api/items" + Query(("q", query), ("n", n.ToString(CultureInfo.InvariantCulture)));
}
