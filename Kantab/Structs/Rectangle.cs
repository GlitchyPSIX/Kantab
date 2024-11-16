using System;
using System.Numerics;
using Newtonsoft.Json;

namespace Kantab.Structs; 

public struct Rectangle {
    [JsonProperty]
    public Vector2 TopLeft { get; set; }
    [JsonProperty]
    public Vector2 BottomRight { get; set; }
    [JsonIgnore]
    public Vector2 Size => new Vector2(Math.Abs(BottomRight.X - TopLeft.X), Math.Abs(BottomRight.Y - TopLeft.Y));
    [JsonIgnore]
    public bool Empty => Size.X == 0 && Size.Y == 0;

    public Rectangle Normalize() {
        Vector2 tl = TopLeft;
        Vector2 br = BottomRight;
        return new Rectangle(new Vector2(Math.Min(tl.X, br.X), Math.Min(tl.Y, br.Y)),
            new Vector2(Math.Max(tl.X, br.X), Math.Max(tl.Y, br.Y)));
    }

    public Rectangle(float x1, float y1, float x2, float y2) {
        TopLeft = new Vector2(Math.Min(x1, x2), Math.Min(y1, y2));
        BottomRight = new Vector2(Math.Max(x1, x2), Math.Max(y1, x2));
    }
    [JsonConstructor]
    public Rectangle(Vector2 topLeft, Vector2 bottomRight) {
        TopLeft = topLeft;
        BottomRight = bottomRight;
    }
}