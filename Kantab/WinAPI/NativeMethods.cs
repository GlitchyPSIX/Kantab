using Kantab.WinAPI.Structs;
using System.Runtime.InteropServices;

namespace Kantab.WinAPI;

public static class NativeMethods
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out CursorPoint point);

}