using System;
using Kantab.Classes.Messages.Common;

namespace Kantab.Classes.Messages;

public abstract class KantabMessage
{
    public abstract byte[] ToBytes();

    public KantabMessage(byte[] bytes) {

    }

    public KantabMessage() {

    }

    public static KantabMessage FromBytes(ArraySegment<byte> bytes) {
        switch (bytes[0]) {
            case 02:
            {
                return new HelloMessage();
            }
            default:
            {
                return new GenericKantabMessage(bytes.Array ?? new byte[]{});
            }   

        }
    }
}