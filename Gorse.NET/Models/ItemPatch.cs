using System.Text.Json.Serialization;

namespace Gorse.NET.Models;

/// <summary>
/// Partial update of an item for PATCH api/item/{item-id}. Only the properties
/// that are set are sent, so unset properties keep their stored value.
/// <see cref="Labels"/> replaces the whole label object; use
/// <see cref="Gorse.PatchItemLabelsAsync"/> to change individual label keys.
/// </summary>
public class ItemPatch
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsHidden { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Categories { get; set; }

    /// <summary>Gorse parses patch timestamps as RFC 3339, hence the explicit offset.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? Timestamp { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Labels { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Comment { get; set; }
}
