using System.Numerics;
using Kantab.Interfaces;
using Kantab.Structs;
using Kantab.WinAPI.Enums;
using static Kantab.WinAPI.NativeMethods;
using Kantab.WinAPI.Structs;

namespace Kantab.Classes.PenStateProviders
{
    public sealed class MousePenStateProvider : IPenStateProvider {
        public bool Extensible => false;

        public bool Extended => false;

        public void SetExtended(bool extend) {
            return; // Do nothing; mouse based state cannot be extended
        }

        public PenState CurrentPenState => GetCursorState();

        private bool _mouseButtonsSwapped;

        public MousePenStateProvider() {
            _mouseButtonsSwapped = GetSystemMetrics(SystemMetric.SM_SWAPBUTTON) != 0;
        }

        private PenState GetCursorState()
        {
            CursorPoint pt = new CursorPoint();
            // NOTE: The below method to get a cursor position is Windows specific.
            // Find a way (probably using compiler flags) to make this method truly crossplatorm.
            GetCursorPos(out pt);
            bool mousePressed = (GetAsyncKeyState(_mouseButtonsSwapped ? Keys.RBUTTON : Keys.LBUTTON) & 0x8000) > 0;
            return new()
            {
                Pressure = mousePressed ? 0.25f : 0,
                Position = new Vector2(pt.X, pt.Y)
            };
        }
    }
}
