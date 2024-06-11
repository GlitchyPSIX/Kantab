using System;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace Kantab.Structs; 

public struct Rectangle {
    public Vector2 TopLeft { get; set; }
    public Vector2 BottomRight { get; set; }

    public Vector2 Size => new Vector2(Math.Abs(BottomRight.X - TopLeft.X), Math.Abs(BottomRight.Y - TopLeft.Y));

    public Rectangle(float x1, float y1, float x2, float y2) {
        TopLeft = new Vector2(x1, y1);
        BottomRight = new Vector2(x2, y2);
    }

    public Rectangle(Vector2 topLeft, Vector2 bottomRight) {
        TopLeft = topLeft;
        BottomRight = bottomRight;
    }
}