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
        public Vector2 Position { get; set; }
        public float Pressure { get; set; }
        public float Tilt { get; set; }

        /// <summary>
        /// Gives pen position relative to the top left corner of a defined section.
        /// </summary>
        /// <param name="screenSection">Rectangle defining section to calculate against</param>
        /// <returns>Normalized position where top left is (0, 0) and bottom right is (1, 1)</returns>
        public Vector2 NormalizePosition(Rectangle screenSection) {
            Vector2 movedPos = Position - screenSection.TopLeft;
            Vector2 size = screenSection.Size;
            return new(movedPos.X / size.X, movedPos.Y / size.Y);
        }

        /// <summary>
        /// Gives pen posistion in absolute pixel coordinates.
        /// </summary>
        /// <param name="screenSection">The section to expand to</param>
        /// <returns>Absolute position where top left is (0, 0) and bottom right is (1, 1)</returns>
        public Vector2 DenormalizePosition(Rectangle screenSection) {
            Vector2 size = screenSection.Size;
            return new(Position.X * size.X, Position.Y * size.Y);
        }
    }
}
