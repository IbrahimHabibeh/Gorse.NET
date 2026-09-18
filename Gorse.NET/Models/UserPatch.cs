using System.Text.Json.Serialization;

namespace Gorse.NET.Models;

/// <summary>
/// Partial update of a user for PATCH api/user/{user-id}. Only the properties
/// that are set are sent.
/// </summary>
public class UserPatch
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Labels { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Comment { get; set; }
}
