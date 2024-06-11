using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Kantab.Structs
{
    public struct PenState
    {
        public Vector2 RawPosition { get; set; }
        public float Pressure { get; set; }
        public float Tilt { get; set; }

        /// <summary>
        /// Gives pen position relative to the top left corner of a defined section.
        /// </summary>
        /// <param name="screenSection">Rectangle defining section to calculate against</param>
        /// <returns>Normalized position where top left is (0, 0) and bottom right is (1, 1)</returns>
        public Vector2 NormalizePosition(Rectangle screenSection) {
            Vector2 movedPos = RawPosition - screenSection.TopLeft;
            Vector2 size = screenSection.Size;
            return new(movedPos.X / size.X, movedPos.Y / size.Y);
        }
    }
}
