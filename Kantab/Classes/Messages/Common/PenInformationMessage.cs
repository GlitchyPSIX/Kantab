using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Kantab.Structs;

namespace Kantab.Classes.Messages.Common;

public class PenInformationMessage : KantabMessage
{
    public bool Extended { get; }
    public PenState State { get; set; }
    public Rectangle? NormalizationRect { get; private set; }
    public bool AbsolutePositioning => NormalizationRect.HasValue;

    public PenInformationMessage(bool extended, float x, float y, float pressure, float tilt, Rectangle? normalizationRect = null) {
        Extended = extended;
        State = new() {
            Position = new(x, y),
            Tilt = tilt,
            Pressure = pressure
        };
        if (normalizationRect.HasValue) {
            NormalizationRect = normalizationRect;
        }
    }

    public PenInformationMessage(bool extended, PenState state, Rectangle? normalizationRect = null)
    {
        Extended = extended;
        State = state;
        if (normalizationRect.HasValue)
        {
            NormalizationRect = normalizationRect;
        }
    }

    public override byte[] ToBytes() {

        Vector2 posVector = NormalizationRect.HasValue
            ? State.NormalizePosition(NormalizationRect.Value)
            : State.Position;

        List<byte> bytesBaton = (new byte[] {04, (byte) (Extended ? 1 : 0)})
            .Concat(BitConverter.GetBytes(posVector.X))
            .Concat(BitConverter.GetBytes(posVector.Y))
            .ToList();

        if (Extended) {
            return bytesBaton.Concat(BitConverter.GetBytes(State.Pressure)).Concat(BitConverter.GetBytes(State.Tilt)).ToArray();
        }
        else {
            return bytesBaton.Append((byte)(State.Pressure > 0 ? 1 : 0)).ToArray();
        }
    }
}