using System.Text.Json.Serialization;

namespace Kantab.Structs;

public struct ConstructMetadata {

    public string Id { get; set; } = null;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("author")]
    public string Author { get; set; } = null;

        [JsonPropertyName("description")]
    public string Description { get; set; } = null;

    [JsonPropertyName("fsbase")] public string FilesystemBase { get; set; } = null;

    // May consider making this one hold the actual constructs instead of pointers.
    [JsonPropertyName("constructs")] public ConstructPointers Constructs { get; set; } = default;

    [JsonConstructor]
    public ConstructMetadata(){}

    public ConstructMetadata(string id, string name, string author, string description, ConstructPointers constructs = default) {
        Id = id;
        Name = name;
        Author = author;
        Description = description;
        Constructs = constructs;
    }
}