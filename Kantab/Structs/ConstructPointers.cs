using System.Text.Json.Serialization;

namespace Kantab.Structs; 

/// <summary>
/// It's like a kind of standee for a simple type.
/// </summary>
public struct ConstructPointers {
    [JsonPropertyName("large")]
    public string LargeConstructName;
    [JsonPropertyName("small")]
    public string SmallConstructName;
}