using System.Text.Json.Serialization;

namespace Kantab.Structs; 

public struct ConstructMetadata {
    [JsonPropertyName("name")]
    public string Name;

    [JsonPropertyName("author")]
    public string Author;

    [JsonPropertyName("description")]
    public string Description;

    public string FilesystemBase;
    
    // May consider making this one hold the actual constructs instead of pointers.
    [JsonPropertyName("constructs")]
    public ConstructPointers Constructs;
}