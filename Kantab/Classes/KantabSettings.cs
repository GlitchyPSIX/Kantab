using System.Numerics;
using System.Text.Json.Serialization;
using Kantab.Structs;

namespace Kantab.Classes;

public class KantabSettings
{
    /// <summary>
    /// The rectangle of the screen (virtual desktop space) to use for mouse calculations
    /// </summary>
    [JsonPropertyName("region")]
    public Rectangle ScreenRegion { get; set; } = new();

    /// <summary>
    /// The Pen Provider. 0 is Mouse/WinTab, 1 is Relay
    /// </summary>
    [JsonPropertyName("penProvider")]
    public byte PenProvider { get; set; } = 1;

    /// <summary>
    /// How often to fetch from the saved Pen Provider in ms
    /// </summary>
    [JsonPropertyName("fetchRate")]
    public float FetchRate { get; set; } = 13f;

    /// <summary>
    /// Port where the Kantab Server will start
    /// </summary>
    [JsonPropertyName("port")]
    public short Port { get; set; } = 7329;

    /// <summary>
    /// Whether Kantab's server will start the moment the app opens
    /// </summary>
    [JsonPropertyName("autostart")]
    public bool Autostart { get; set; } = false;

    /// <summary>
    /// Name of the folder that contains the Construct to use
    /// </summary>
    [JsonPropertyName("construct")]
    public string? ConstructFolder { get; set; } = null;

    /// <summary>
    /// The Scale of the Construct displayed
    /// </summary>
    [JsonPropertyName("scale")]
    public float Scale { get; set; } = 1.0f;

    [JsonConstructor]
    public KantabSettings()
    {
        FetchRate = 13f;
        Port = 7329;
        Autostart = false;
        ConstructFolder = null;
        Scale = 1.0f;
        PenProvider = 1;
    }
}