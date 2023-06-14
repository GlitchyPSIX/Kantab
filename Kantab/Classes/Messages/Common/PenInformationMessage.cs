using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kantab.Classes.Messages.Common;

public class PenInformationMessage : KantabMessage
{
    public bool Extended { get; }
    public float X { get; }
    public float Y { get; }
    public float Pressure { get; }
    public float Tilt { get; }

    public PenInformationMessage(bool extended, float x, float y, float pressure, float tilt) {
        Extended = extended;
        X = x;
        Y = y;
        Pressure = pressure;
        Tilt = tilt;
    }

    public override byte[] ToBytes() {
        List<byte> bytesBaton = (new byte[] {04, (byte) (Extended ? 1 : 0)}).Concat(BitConverter.GetBytes(X)).Concat(BitConverter.GetBytes(Y)).ToList();
        if (Extended) {
            return bytesBaton.Concat(BitConverter.GetBytes(Pressure)).Concat(BitConverter.GetBytes(Tilt)).ToArray();
        }
        else {
            return bytesBaton.ToArray();
        }

        
    }
}