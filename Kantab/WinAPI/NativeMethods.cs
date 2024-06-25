using Kantab.WinAPI.Structs;
using System.Runtime.InteropServices;
using Kantab.WinAPI.Enums;

namespace Kantab.WinAPI;

public static class NativeMethods
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out CursorPoint point);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(SystemMetric smIndex);

    [DllImport("user32.dll")]

    public static extern short GetAsyncKeyState(Keys vKeys);

}