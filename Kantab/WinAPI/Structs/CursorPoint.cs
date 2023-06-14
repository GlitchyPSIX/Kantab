using System.Runtime.InteropServices;
using System;

namespace Kantab.WinAPI.Structs;

[StructLayout(LayoutKind.Sequential)]
public struct CursorPoint
{
    public int X;
    public int Y;
}