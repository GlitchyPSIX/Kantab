using System.Numerics;

namespace Kantab.Structs; 

public struct KantabSettings {
    /// <summary>
    /// The rectangle of the screen (virtual desktop space) to use for mouse calculations
    /// </summary>
    public Rectangle ScreenRegion { get; set; }

    /// <summary>
    /// How often to fetch from the saved Pen Provider in ms
    /// </summary>
    public float FetchRate { get; set; }

    /// <summary>
    /// Port where the Kantab Server will start
    /// </summary>
    public short Port { get; set; }

    /// <summary>
    /// Whether Kantab's server will start the moment the app opens
    /// </summary>
    public bool Autostart { get; set; }
}