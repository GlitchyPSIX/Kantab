using System;

namespace Kantab.Classes.Messages.Common;

public class PingMessage : KantabMessage
{
    public override byte[] ToBytes()
    {
        return new byte[] { 01, 00 };
    }
}