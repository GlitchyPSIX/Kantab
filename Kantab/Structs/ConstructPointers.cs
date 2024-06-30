using System.Text.Json.Serialization;

namespace Kantab.Structs; 

/// <summary>
/// It's like a kind of standee for a simple type.
/// </summary>
public struct ConstructPointers {
    [JsonPropertyName("large")]
    public string LargeConstructName { get; set; } = null;
    [JsonPropertyName("small")]
    public string SmallConstructName { get; set; } = null;

    [JsonConstructor]
    public ConstructPointers(){}


    public ConstructPointers(string largeConstructName, string smallConstructName) {
        LargeConstructName = largeConstructName;
        SmallConstructName = smallConstructName;
    }
}