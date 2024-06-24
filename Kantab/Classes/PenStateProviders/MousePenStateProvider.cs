using System.Numerics;
using Kantab.Interfaces;
using Kantab.Structs;
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

        private PenState GetCursorState()
        {
            CursorPoint pt = new CursorPoint();
            // NOTE: The below method to get a cursor position is Windows specific.
            // Find a way (probably using compiler flags) to make this method truly crossplatorm.
            GetCursorPos(out pt);
            return new()
            {
                Pressure = 1,
                Position = new Vector2(pt.X, pt.Y)
            };
        }
    }
}
