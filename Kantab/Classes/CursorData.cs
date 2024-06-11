using System.Numerics;
using Kantab.WinAPI.Structs;
using static Kantab.WinAPI.NativeMethods;

namespace Kantab.Classes; 

public static class CursorData {
    /// <summary>
    /// Gets the cursor position as a Vector2.
    /// </summary>
    /// <param name="topLeftOffset">Top-left offset</param>
    /// <returns></returns>
    public static Vector2 CursorPosition(Vector2 topLeftOffset) {
        CursorPoint pt = new CursorPoint();
        GetCursorPos(out pt);
        Vector2 outVec2 = new Vector2(pt.X, pt.Y);
        return outVec2 - topLeftOffset;
    }

    /// <summary>
    /// Simulates the behavior you'd get from asking World Position to Screen Point from Unity; gets the cursor within an area and returns a value between 0 and 1.
    /// </summary>
    /// <param name="topLeftOffset">Top-left point of the "screen area"</param>
    /// <param name="botRightOffset">Bottom-right point of the "screen area"</param>
    /// <returns></returns>
    public static Vector2 CursorPositionAtScreenPoint(Vector2 topLeftOffset, Vector2 botRightOffset) {
        // Coordinate system for Windows cursor (and pixiJS) is Right +X, Down +Y
        Vector2 squareMags = botRightOffset - topLeftOffset;
        Vector2 pos = CursorPosition(topLeftOffset);
        return new Vector2((pos.X - squareMags.X / 2) / squareMags.X/2, (pos.Y - squareMags.Y / 2) / squareMags.Y/2);
    }
}