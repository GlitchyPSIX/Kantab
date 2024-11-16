using System;
using Kantab.Classes.Messages.Common;
using Kantab.Structs;

namespace Kantab.Classes.Messages;

public abstract class KantabMessage {
    public abstract byte[] ToBytes();

    public KantabMessage(byte[] bytes) {

    }

    public KantabMessage() {

    }

    public static KantabMessage FromBytes(ArraySegment<byte> bytes) {
        switch (bytes[0]) {
            case 02: {
                    return new HelloMessage();
                }
            case 03: {
                    return new CapabilitiesMessage(bytes[1]);
                }
            case 04: {
                    PenState ps = new();
                    bool extended = bytes[1] == 1;
                    ps.Position = new System.Numerics.Vector2(BitConverter.ToSingle(bytes.Slice(2, 4)), BitConverter.ToSingle(bytes.Slice(6, 4)));

                    if (extended) {
                        ps.Pressure = BitConverter.ToSingle(bytes.Slice(0x0A, 4));
                        ps.Tilt = BitConverter.ToSingle(bytes.Slice(0x0E, 4));
                    }
                    else {
                        // in non Extended mode, any value over 0 counts as 0.25f
                        ps.Pressure = bytes[0x0A] > 0 ? 0.25f : 0;
                        ps.Tilt = 0;
                    }
                    return new PenInformationMessage(extended, ps);

                }
            default: {
                    return new GenericKantabMessage(bytes.Array ?? new byte[] { });
                }

        }
    }
}